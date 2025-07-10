using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.GUI
{
	public partial class ComplianceRiskLogViewerUserControl : ZUserControl
	{
		public ComplianceRiskLogViewerUserControl()
		{
			InitializeComponent();
			ComplianceRiskStatusChangesLogsGrid.AfterBind += ComplianceRiskStatusChangesLogsGrid_AfterBind;
			SnapshotPartiesGrid.FontDeciding += SnapshotPartiesGrid_FontDeciding;
			SnapshotLocationsGrid.FontDeciding += SnapshotLocationsGrid_FontDeciding;
			AddControls();
		}

		void AddControls()
		{
			var commodityAssessmentLogPanelUserControl = new CommodityRiskLogPanelUserControl() { Dock = DockStyle.Fill };
			var snapshotCommoditiesGrid = commodityAssessmentLogPanelUserControl.CommodityRiskLogGrid.SnapshotCommoditiesGrid;
			if (snapshotCommoditiesGrid.GetColumnStyle("DateAddedUtc") != null)
			{
				snapshotCommoditiesGrid.GetColumnStyle("DateAddedUtc").IsVisible = false;
			}

			CommodityTableLayoutPanel.Controls.Add(commodityAssessmentLogPanelUserControl, 0, 1);
			BindingSource.SetBindingMember(commodityAssessmentLogPanelUserControl, "CommodityRiskLogCollection");

			snapshotCommoditiesGrid.GetColumnStyle("NomenclatureCondition").IsVisible = false;
			snapshotCommoditiesGrid.GetColumnStyle("SpecificCondition").IsVisible = false;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is IComplianceItemRiskStatusProvider provider)
			{
				StatusChangeLogCollection = new ComplianceRiskStatusChangeLogCollection(provider);

				base.SetDataBinding(StatusChangeLogCollection, dataMember);

				StatusChangeLogCollection.Factory.Saved -= Factory_Saved;
				StatusChangeLogCollection.Factory.Saved += Factory_Saved;
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			ReloadCollectionIfNeeded();
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				ReloadCollectionIfNeeded();
			}
		}

		void ReloadCollectionIfNeeded()
		{
			if (Visible && !IsDisposing && !IsDisposed)
			{
				StatusChangeLogCollection?.LoadCollection();
			}
		}

		ComplianceRiskStatusChangeLogCollection StatusChangeLogCollection { get; set; }

		void ComplianceRiskStatusChangesLogsGrid_AfterBind(object sender, EventArgs e)
		{
			if (ComplianceRiskStatusChangesLogsGrid.ListManager != null)
			{
				SetPartyLocationAssessmentPanelAndLabelColor();
				ComplianceRiskStatusChangesLogsGrid.ListManager.CurrentItemChanged -= ListManager_CurrentItemChanged;
				ComplianceRiskStatusChangesLogsGrid.ListManager.CurrentItemChanged += ListManager_CurrentItemChanged;
			}
		}

		void ListManager_CurrentItemChanged(object sender, EventArgs e)
		{
			SetPartyLocationAssessmentPanelAndLabelColor();
		}

		void SetPartyLocationAssessmentPanelAndLabelColor()
		{
			if (ComplianceRiskStatusChangesLogsGrid.GetCurrent() is ComplianceRiskStatusChangeLog selectedLog)
			{
				SetPartyLocationCommodityPanel(selectedLog);

				PartyRiskLabel.BackColor = ComplianceRiskColorHelper.GetColorForRiskStatus(selectedLog.PartyRisk);
				LocationRiskLabel.BackColor = ComplianceRiskColorHelper.GetColorForRiskStatus(selectedLog.LocationRisk);
				CommodityRiskLabel.BackColor = ComplianceRiskColorHelper.GetColorForRiskStatus(selectedLog.CommodityRisk);
			}
			else
			{
				SetPartyLocationCommodityPanel(null);
			}
		}

		void SetPartyLocationCommodityPanel(ComplianceRiskStatusChangeLog selectedLog)
		{
			var collapsed = selectedLog == null || !selectedLog.SnapshotExists;

			InnerSplitContainer1.Panel2Collapsed = collapsed || !selectedLog.IsCompliancePartyRiskProvider;
			InnerSplitContainer2.Panel2Collapsed = collapsed || !selectedLog.IsComplianceLocationRiskProvider;
			InnerSplitContainer3.Panel2Collapsed = collapsed || !selectedLog.ComplianceJobDirection.IsInternational || !selectedLog.IsComplianceCommodityRiskProvider;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string CodeColumnName = "Code";
		const string LocationDescriptionColumnName = "LocationDescription";

		void SnapshotPartiesGrid_FontDeciding(object sender, FontDecidingEventArgs e)
		{
			if (e.DataMember == CodeColumnName)
			{
				e.Font = new Font(e.OriginalFont, FontStyle.Underline);
			}
		}

		void SnapshotLocationsGrid_FontDeciding(object sender, FontDecidingEventArgs e)
		{
			if (e.DataMember == LocationDescriptionColumnName)
			{
				e.Font = new Font(e.OriginalFont, FontStyle.Underline);
			}
		}

		void SnapshotPartiesGrid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Clicks == 1)
			{
				var hitTest = SnapshotPartiesGrid.HitTest(e.X, e.Y);
				var row = hitTest.Row;
				var col = hitTest.Column;
				if (row >= 0 && col >= 0)
				{
					var column = SnapshotPartiesGrid.Columns[col];
					if (column.ColumnName == CodeColumnName)
					{
						var listManager = SnapshotPartiesGrid.ListManager.List;
						if (row < listManager.Count)
						{
							var partyLog = listManager[row] as CompliancePartyRiskLog;

							if (partyLog != null)
							{
								if (partyLog.Party.TableCode == OrgHeaderSchema.Constants.Prefix)
								{
									OpenForm(partyLog, typeof(OrgHeader));
								}
								else if (partyLog.Party.TableCode == RefVesselSchema.Constants.Prefix)
								{
									OpenForm(partyLog, typeof(RefVessel));
								}
								else
								{
									Globals.Message.Show(Res.GetString("b2ec6166-08ba-4afd-851f-761208f44001", "The selected party is not an organization or vessel."));
								}
							}
						}
					}
				}
			}
		}

		void SnapshotLocationsGrid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Clicks == 1)
			{
				var hitTest = SnapshotLocationsGrid.HitTest(e.X, e.Y);
				var row = hitTest.Row;
				var col = hitTest.Column;
				if (row >= 0 && col >= 0)
				{
					var column = SnapshotLocationsGrid.Columns[col];
					if (column.ColumnName == LocationDescriptionColumnName)
					{
						var listManager = SnapshotLocationsGrid.ListManager.List;
						if (row < listManager.Count)
						{
							var locationLog = listManager[row] as ComplianceLocationRiskLog;

							if (locationLog != null)
							{
								OpenForm(locationLog, typeof(RefCountry));
							}
						}
					}
				}
			}
		}

		protected void OpenForm(CompliancePartyRiskLog partyLog, Type type)
		{
			if (StatusChangeLogCollection != null)
			{
				var bizO = StatusChangeLogCollection.Factory.Load(partyLog.Party.TableCode, partyLog.Party.PK);
				if (bizO == null)
				{
					Globals.Message.Show(Res.GetString("cd2e2c06-9281-4715-88f8-c42ffe04e76f", "The selected party is no longer exists."));
				}
				else
				{
					OpenFormCore(bizO, type);
				}
			}
		}

		protected void OpenForm(ComplianceLocationRiskLog locationLog, Type type)
		{
			if (StatusChangeLogCollection != null)
			{
				var bizO = StatusChangeLogCollection.Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, locationLog.Location);
				if (bizO == null)
				{
					Globals.Message.Show(Res.GetString("b8f4c74c-1634-4840-a156-6ab118922f6d", "The selected country is no longer exists."));
				}
				else
				{
					OpenFormCore(bizO, type);
				}
			}
		}

		void OpenFormCore(BusinessObject bizO, Type type)
		{
			var controller = ZControllerFactory.Instance.GetControllerForBizo(bizO) ?? ZControllerFactory.Instance.GetControllerForType(type);

			if (controller == null)
			{
				var ex = new Exception("Controller can not be null");
				var message = Res.GetString("A2D82FAC-0255-415D-A36F-4BE930BFF80D", "Can't load appropriate Controller for type: {0}, PK: {1}", bizO.GetType(), bizO.PK);
				Globals.Message.ShowError(message);
				ExceptionReporter.Instance.ReportDeveloperException("ComplianceRiskLogViewerUserControl|CannotOpenForm", message, ex);
			}
			else
			{
				if (controller.ShowViewForm(bizO) is ISupportViewDpsLogsTab supportViewDPsLogsTab)
				{
					supportViewDPsLogsTab.SwitchToDpsLogsTab();
				}
			}
		}
	}
}
