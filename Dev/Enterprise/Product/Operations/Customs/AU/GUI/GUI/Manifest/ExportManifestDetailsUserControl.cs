using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.AU.ExportManifest.GUI
{
	/// <summary>
	/// Summary description for ExportManifestDetailsUserControl.
	/// </summary>
	public partial class ExportManifestDetailsUserControl : ZUserControl
	{
		public ExportManifestDetailsUserControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			normalLinesGrid.CurrentCellChanged += LineVisibilityChanged;
			normalLinesGrid.VisibleChanged += ExportManifestDetailsUserControl_VisibleChanged;

			eSMAirGrid.CurrentCellChanged += LineVisibilityChanged;
			eSMAirGrid.VisibleChanged += ExportManifestDetailsUserControl_VisibleChanged;

			lineDetailsTabPage.BindingOrFirstShown += delegate
			{
				lineControlsAreLoaded = true;
				SetLineItemsVisibility();
			};
		}

		bool lineControlsAreLoaded;

		public ExportCustomsManifestHeader Header
		{
			get
			{
				return fHeader;
			}
			set
			{
				if (fHeader != value)
				{
					UnhookVisibilityChangedEvents();
					fHeader = value;
					HookVisibilityChangedEvents();
					VisibilityChanged(this, new EventArgs());
				}
			}
		}
		protected ExportCustomsManifestHeader fHeader;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			airCTOLinesGrid.ContextMenu.MenuItems.Add("-");
			airCTOLinesGrid.ContextMenu.MenuItems.Add(new ZMenuItem("Request Customs CAN Status", new EventHandler(RequestCustomsStatus)));
			seaLinesGrid.ContextMenu.MenuItems.Add("-");
			seaLinesGrid.ContextMenu.MenuItems.Add(new ZMenuItem("Request Customs CAN Status", new EventHandler(RequestCustomsStatus)));
			airMainManifestLinesGrid.ContextMenu.MenuItems.Add("-");
			airMainManifestLinesGrid.ContextMenu.MenuItems.Add(new ZMenuItem("Request Customs CAN Status", new EventHandler(RequestCustomsStatus)));
			normalLinesGrid.ContextMenu.MenuItems.Add("-");
			normalLinesGrid.ContextMenu.MenuItems.Add(new ZMenuItem("Request Customs CAN Status", new EventHandler(RequestCustomsStatus)));
			eSMAirGrid.ContextMenu.MenuItems.Add("-");
			eSMAirGrid.ContextMenu.MenuItems.Add(new ZMenuItem("Request Customs CAN Status", new EventHandler(RequestCustomsStatus)));
			eSMSeaGrid.ContextMenu.MenuItems.Add("-");
			eSMSeaGrid.ContextMenu.MenuItems.Add(new ZMenuItem("Request Customs CAN Status", new EventHandler(RequestCustomsStatus)));
		}

		void RequestCustomsStatus(object sender, EventArgs args)
		{
			List<BusinessObject> lines = new List<BusinessObject>();
			if (airCTOLinesGrid.Visible)
			{
				lines.AddRange(airCTOLinesGrid.SelectedElements);
			}
			else if (seaLinesGrid.Visible)
			{
				lines.AddRange(seaLinesGrid.SelectedElements);
			}
			else if (airMainManifestLinesGrid.Visible)
			{
				lines.AddRange(airMainManifestLinesGrid.SelectedElements);
			}
			else if (normalLinesGrid.Visible)
			{
				lines.AddRange(normalLinesGrid.SelectedElements);
			}
			else if (eSMAirGrid.Visible)
			{
				lines.AddRange(eSMAirGrid.SelectedElements);
			}
			else if (eSMSeaGrid.Visible)
			{
				lines.AddRange(eSMSeaGrid.SelectedElements);
			}

			if (Header.HasChanges)
			{
				Globals.Message.ShowError("Please save the form before using this feature.");
			}
			else if (lines.Count < 1)
			{
				Globals.Message.ShowError("Please select one or more lines for which you wish to send status requests.");
			}
			else
			{
				if (ShouldSendStatusMessages(lines))
				{
					SendStatusMessages(lines);
				}
			}
		}

		bool ShouldSendStatusMessages(List<BusinessObject> lines)
		{
			ZStringBuilder builder = new ZStringBuilder();
			int count = 0;
			foreach (ExportCustomsManifestLines line in lines)
			{
				if (line.EL_TypeOfCAN != CANType.CustomsAuthorityNumber.Code)
				{
					builder.Append("Line type must be 'CAN' to request status, message will not be sent for line " + line.EL_LineNo);
				}
				else if (line.EL_CAN.Length < 9)
				{
					builder.Append("Invalid CAN (" + line.EL_CAN + "), message will not be sent for line " + line.EL_LineNo);
				}
				else
				{
					StatusRequestManager manager = new StatusRequestManager(line);
					if (manager.CanSendOriginal)
					{
						builder.Append("Send " + manager.MessageFriendlyName);
						count++;
					}
					else
					{
						builder.Append("Cannot send " + manager.MessageFriendlyName);
					}
				}
			}
			builder.Append(count.ToString() + " messages will be sent.");
			return Globals.Message.Show(builder.ToStringWithNewLineBetweenAppends(), "Confirm Send Request", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
		}

		void SendStatusMessages(List<BusinessObject> lines)
		{
			foreach (ExportCustomsManifestLines line in lines)
			{
				if (line.EL_CAN.Length > 8)
				{
					StatusRequestManager manager = new StatusRequestManager(line);
					if (manager.CanSendOriginal)
					{
						manager.GenerateOriginalMessages(line);
						try
						{
							line.Factory.Save();
						}
						catch (ZSaveException e)
						{
							ZExceptionReporting.HandleSaveException(e);
						}
					}
				}
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookVisibilityChangedEvents();
			}
			base.Dispose(disposing);
		}

		void HookVisibilityChangedEvents()
		{
			if (fHeader != null)
			{
				fHeader.ED_ManifestTypeInfo.ValueChanged += VisibilityChanged;
				fHeader.ED_TransportModeInfo.ValueChanged += VisibilityChanged;
				fHeader.Lines.TypeOfCANChanged += LineVisibilityChanged;
			}
		}

		void UnhookVisibilityChangedEvents()
		{
			if (fHeader != null)
			{
				fHeader.ED_ManifestTypeInfo.ValueChanged -= VisibilityChanged;
				fHeader.ED_TransportModeInfo.ValueChanged -= VisibilityChanged;
				fHeader.Lines.TypeOfCANChanged -= LineVisibilityChanged;
			}
		}

		private void VisibilityChanged(object sender, EventArgs e)
		{
			SetHeaderItemsVisibility();
			SetLineItemsVisibility();
		}

		void ReplaceHeaderDetailsWith<T>() where T : BaseHeaderDetails, new()
		{
			if (!(headerDetailsControl is T))
			{
				BaseHeaderDetails newControl = new T();
				headerDetailsGroupBox.Controls.Remove(headerDetailsControl);
				if (headerDetailsControl != null)
				{
					headerDetailsControl.Dispose();
				}

				headerDetailsControl = newControl;
				headerDetailsControl.Dock = DockStyle.Fill;
				headerDetailsControl.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
				headerDetailsControl.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader";
				headerDetailsGroupBox.Controls.Add(headerDetailsControl);

				if (hasBeenBound)
				{
					headerDetailsControl.SetDataBinding(Header, "");
				}
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			hasBeenBound = dataSource != null;
		}
		bool hasBeenBound;

		private void SetHeaderItemsVisibility()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				bool showDepartureReportStatus = true;
				bool showDocumentStatus = true;
				bool showLines = true;

				switch (Header.ED_ManifestType)
				{
					case AirManifestTypeList.Codes.CtoReceivalRemovalStandAlone:
						ReplaceHeaderDetailsWith<CTOHeaderDetails>();
						showDepartureReportStatus = false;
						showDocumentStatus = false;
						break;
					case ManifestTypeList.Codes.ConsolidationExportSubManifest:
						if (Header.IsAirCTOHeader)
						{
							ReplaceHeaderDetailsWith<CTOESMHeaderDetails>();
						}
						else
						{
							ReplaceHeaderDetailsWith<ESMHeaderDetails>();
						}
						break;
					case ManifestTypeList.Codes.DepartureReport:
						ReplaceHeaderDetailsWith<DEPHeaderDetails>();
						showLines = false;
						showDocumentStatus = false;
						break;
					case ManifestTypeList.Codes.ExportMainManifest:
						if (Header.IsAirCTOHeader)
						{
							ReplaceHeaderDetailsWith<CTOEMMHeaderDetails>();
						}
						else
						{
							ReplaceHeaderDetailsWith<EMMHeaderDetails>();
						}
						break;
					case ManifestTypeList.Codes.SlotExportSubManifest:
						ReplaceHeaderDetailsWith<SLTHeaderDetails>();
						break;
					default:
						ReplaceHeaderDetailsWith<BlankHeaderDetails>();
						break;
				}

				this.departureReportStatusDropEdit.Visible = showDepartureReportStatus;
				this.departureReportStatusLabel.Visible = showDepartureReportStatus;

				this.linesTabControl.Visible = showLines;
				this.normalLinesGrid.Visible = showLines;
				this.eSMAirGrid.Visible = showLines;

				this.documentStatusConditionsDropEdit.Visible = showDocumentStatus;
				this.documentStatusConditionsLabel.Visible = showDocumentStatus;
				this.documentStatusDropEdit.Visible = showDocumentStatus;
				this.documentStatusLabel.Visible = showDocumentStatus;

				if (Header.IsSea)
				{
					this.headerDetailsControl.ShowForSea();
				}
				else
				{
					this.headerDetailsControl.ShowForAir();
				}

				this.headerDetailsControl.ShowForConsolidation(Header.IsConsolidation, Header.IsAirCTOHeader);
				if (!Header.IsConsolidation && !Header.PackDepotAddress.IsEmpty)
				{
					Header.PackDepotAddress.OrganisationPK = ZGuid.Empty;
					Header.PackDepotAddress.RefreshBinding();
				}

				const int ShowNone = 0;
				const int ShowNormalLines = 1;
				const int ShowSeaLines = 2;
				const int ShowAirCTOLines = 3;
				const int ShowAirMainLines = 4;
				const int ShowESMAirLines = 5;
				const int ShowESMSeaLines = 6;

				int linesToShow;
				if ((Header.IsAir && Header.IsCTO) || ((Header.IsMainManifest || Header.IsConsolidation) && Header.IsAirCTOHeader))
				{
					linesToShow = ShowAirCTOLines;
				}
				else if (Header.IsSea && !Header.IsDeparture && Header.IsManifestDeclaredAtCustoms && !Header.IsConsolidation)
				{
					linesToShow = ShowSeaLines;
				}
				else if (Header.IsAir && Header.IsMainManifest && !(Header.IsAirCTOHeader))
				{
					linesToShow = ShowAirMainLines;
				}
				else if (!Header.IsDeparture && !Header.IsConsolidation)
				{
					linesToShow = ShowNormalLines;
				}
				else if (Header.IsConsolidation && Header.IsAir)
				{
					linesToShow = ShowESMAirLines;
				}
				else if (Header.IsConsolidation && Header.IsSea)
				{
					linesToShow = ShowESMSeaLines;
				}
				else
				{
					linesToShow = ShowNone;
				}

				this.airCTOLinesGrid.Visible = (linesToShow == ShowAirCTOLines);
				this.seaLinesGrid.Visible = (linesToShow == ShowSeaLines);
				this.airMainManifestLinesGrid.Visible = (linesToShow == ShowAirMainLines);
				this.normalLinesGrid.Visible = (linesToShow == ShowNormalLines);

				this.eSMAirGrid.Visible = (linesToShow == ShowESMAirLines);
				this.eSMSeaGrid.Visible = (linesToShow == ShowESMSeaLines);
			}
		}

		private void SetLineItemsVisibility()
		{
			if (!DesignModeFinder.IsDesigning && lineControlsAreLoaded && Header != null)
			{
				SetGridCloumHeader();
				bool isPersonalEffectsOrLowValue = false;
				bool isExemption = false;

				int rowIndex = (Header.IsConsolidation && Header.IsAir) ? eSMAirGrid.CurrentRowIndex : normalLinesGrid.CurrentRowIndex;
				ExportCustomsManifestLines line = null;

				if (rowIndex >= 0 && rowIndex < Header.Lines.Count)
				{
					line = Header.Lines[rowIndex];
				}
				else if (Header.Lines.Count > 0)
				{
					line = Header.Lines[0];
				}

				if (line != null)
				{
					isPersonalEffectsOrLowValue = line.IsPersonalEffectsOrLowValue && !line.Header.IsMainManifest;
					isExemption = line.IsExemptLine;
				}

				SetLineItemsVisibility(isPersonalEffectsOrLowValue, isExemption, Header.IsSea);
			}
		}

		private void SetGridCloumHeader()
		{
			if (Header.IsAirCTOHeader)
			{
				if (airCTOLinesGrid.Columns[ExportCustomsManifestLinesSchema.Constants.EL_AirWayBill] != null)
				{
					airCTOLinesGrid.Columns[ExportCustomsManifestLinesSchema.Constants.EL_AirWayBill].ColumnStyle.HeaderText = Header.ED_ManifestType == ManifestTypeList.Codes.ConsolidationExportSubManifest ? "House Bill" : "Air Way Bill";
				}
			}
		}

		private void SetLineItemsVisibility(bool isLowValueOrPersonalEffects, bool isExemption, bool isSea)
		{
			if (isLowValueOrPersonalEffects != oldIsLowValueOrPersonalEffects)
			{
				descriptionLabel.Visible = isLowValueOrPersonalEffects;
				descriptionTextBox.Visible = isLowValueOrPersonalEffects;
				lineCountryOfDestinationLabel.Visible = isLowValueOrPersonalEffects;
				lineCountryOfDestinationCodeFindBox.Visible = isLowValueOrPersonalEffects;
				ownerLabel.Visible = isLowValueOrPersonalEffects;
				ownerOrganisationFindBox.Visible = isLowValueOrPersonalEffects;
				ownerNameLabel.Visible = isLowValueOrPersonalEffects;
				goodsOwnerTextBox.Visible = isLowValueOrPersonalEffects;
				ownerIDLabel.Visible = isLowValueOrPersonalEffects;
				goodsOwnerIDTextBox.Visible = isLowValueOrPersonalEffects;
				oldIsLowValueOrPersonalEffects = isLowValueOrPersonalEffects;
			}

			if (isExemption != oldIsExemption)
			{
				lineCANLabel.Visible = !isExemption;
				lineCANTextBox.Visible = !isExemption;
				oldIsExemption = isExemption;
			}

			if (isSea != oldIsSea)
			{
				containersLabel.Visible = isSea;
				lineContainersCalcEdit.Visible = isSea;
				oldIsSea = isSea;
			}
		}

		protected bool oldIsLowValueOrPersonalEffects = true;
		protected bool oldIsExemption;
		protected bool oldIsSea = true;

		void LineVisibilityChanged(object sender, EventArgs e)
		{
			SetLineItemsVisibility();
		}

		void ExportManifestDetailsUserControl_VisibleChanged(object sender, EventArgs e)
		{
			SetLineItemsVisibility();
		}

		ZGrid CurrentGrid
		{
			get
			{
				if (normalLinesGrid.Visible)
				{
					return normalLinesGrid;
				}

				if (airCTOLinesGrid.Visible)
				{
					return airCTOLinesGrid;
				}

				if (airMainManifestLinesGrid.Visible)
				{
					return airMainManifestLinesGrid;
				}

				if (seaLinesGrid.Visible)
				{
					return seaLinesGrid;
				}

				if (eSMAirGrid.Visible)
				{
					return eSMAirGrid;
				}

				if (eSMSeaGrid.Visible)
				{
					return eSMSeaGrid;
				}

				return null;
			}
		}

		#region ShowHawbDetailsForm

		void LinesGrid_DoubleClick(object sender, EventArgs e)
		{
			ShowHawbDetailsForm();
		}

		void ShowHawbDetailsForm()
		{
			AirCTOExportCustomsManifestHeader airCTOHeader = Header as AirCTOExportCustomsManifestHeader;
			if (CurrentGrid != null && CurrentGrid.ListManager.Position >= 0 && airCTOHeader != null)
			{
				if (airCTOHeader.HasChanges)
				{
					Globals.Message.ShowError("The current form has changes, you will need to save before opening this HAWB.");
				}
				else
				{
					ExportCustomsManifestLines hawb = (ExportCustomsManifestLines)CurrentGrid.ListManager.GetCurrent();
					ZController controller = ZControllerFactory.Create(ControllerIDs.Customs.AU.AirCTOHawbExport);

#if DEBUG
					lastHawbController = controller;
#endif

					controller.ShowEditForm(hawb);
				}
			}
		}

#if DEBUG
		public ZController lastHawbController;
#endif

		#endregion
	}
}
