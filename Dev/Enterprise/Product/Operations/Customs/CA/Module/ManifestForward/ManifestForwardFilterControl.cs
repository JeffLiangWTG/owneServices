using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public partial class ManifestForwardFilterControl : Enterprise.Messaging.Module.EDIMessageFilterControl
	{
		public ManifestForwardFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			var zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("ManifestForwardFilterControl|0A6F3DD8-E669-4870-A56E-BDAF2F63A867", "Job");
			zTextBoxColumnStyleInfo4.ColumnName = EDIMessage.Schema.LinkedObjectReference;
			zTextBoxColumnStyleInfo4.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo4, 70, true);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			var zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("ManifestForwardFilterControl|70F14288-CBBD-41B0-BE9B-DA791204369E", "Tran. #", "Transaction #", "Transaction Number", "");
			zTextBoxColumnStyleInfo5.ColumnName = EDIMessage.Schema.TransactionNumber;
			zTextBoxColumnStyleInfo5.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo5, 100, true);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("ManifestForwardFilterControl|CFA9FFA1-76EF-43F0-A91D-4D97213BAB48", "SNP Type", "Secondary Notify Party Type", "");
			zTextBoxColumnStyleInfo1.ColumnName = EDIMessage.Schema.SNPType;
			zTextBoxColumnStyleInfo1.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo1, 50, true);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			var zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("ManifestForwardFilterControl|1DDF2733-B243-457E-A131-ACFB6FFC3884", "House CCN", "House Cargo Control Number", "");
			zTextBoxColumnStyleInfo2.ColumnName = EDIMessage.Schema.CargoControlNumber;
			zTextBoxColumnStyleInfo2.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo2, 100, true);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			var zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("ManifestForwardFilterControl|C4E858E2-300C-4D25-B415-162709A57F7E", "Primary CCN", "Primary Cargo Control Number", "");
			zTextBoxColumnStyleInfo3.ColumnName = EDIMessage.Schema.PrimaryCargoControlNumber;
			zTextBoxColumnStyleInfo3.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo3, 100, true);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			var zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("ManifestForwardFilterControl|C5C3426D-EF1C-454A-9B6E-8BEB38FAAA71", "Sub-Loc.", "Sub-Location Code", "");
			zTextBoxColumnStyleInfo7.ColumnName = EDIMessage.Schema.SubLocation;
			zTextBoxColumnStyleInfo7.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo7, 50, true);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			var zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("ManifestForwardFilterControl|EA39DD62-F65D-41B4-977F-537B517835A1", "CBSA Office", "CBSA Office Code", "");
			zTextBoxColumnStyleInfo8.ColumnName = EDIMessage.Schema.CBSAOffice;
			zTextBoxColumnStyleInfo8.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo8, 50, true);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);

			using (FilteredGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				FilteredGrid.SetColumnWidth(EDIMessage.Schema.EM_MessageType, 40);
				FilteredGrid.SetColumnWidth(EDIMessage.Schema.EM_MessageSubTypeDescription, 100);
				FilteredGrid.SetColumnWidth(EDIMessage.Schema.EM_SendOrReceiveHumanReadable, 55);
				FilteredGrid.SetColumnWidth(EDIMessage.Schema.EM_Status, 40);
				FilteredGrid.SetColumnWidth(EDIMessage.Schema.EM_MessageDateTime, 95);
				FilteredGrid.SetColumnWidth(EDIMessage.Schema.EM_MessageSubType, 40);
				FilteredGrid.ReOrderColumns(EDIMessageFilterControlDefaultColumnsSequence);
			}
			FilteredGrid.CurrentCellChanged += new EventHandler(this.CurrentCellChanged_Click);
			FilteredGrid.FontDeciding += new EventHandler<FontDecidingEventArgs>(UnderlineLinkedObjectReference);
		}

		string[] EDIMessageFilterControlDefaultColumnsSequence
		{
			get
			{
				if (eDIMessageFilterControlDefaultColumnsSequence == null)
				{
					var columnList = new List<string>
					{
						EDIMessage.Schema.SNPType,
						EDIMessage.Schema.CargoControlNumber,
						EDIMessage.Schema.LinkedObjectReference,
						EDIMessage.Schema.TransactionNumber,
						EDIMessage.Schema.PrimaryCargoControlNumber,
						EDIMessage.Schema.SubLocation,
						EDIMessage.Schema.CBSAOffice,
						EDIMessage.Schema.EM_MessageDateTime,
						EDIMessage.Schema.EM_MessageType,
						EDIMessage.Schema.EM_MessageSubType,
						EDIMessage.Schema.EM_MessageSubTypeDescription,
						EDIMessage.Schema.EM_InterchangeSender,
						EDIMessage.Schema.EM_SendingUser,
						EDIMessage.Schema.EM_Status,
						EDIMessage.Schema.EM_SendOrReceiveHumanReadable,
						EDIMessage.Schema.EM_MessageNum,
						EDIMessage.Schema.EM_DateTimeInterchangeSent,
						EDIMessage.Schema.EM_InterchangeNumber,
						EDIMessage.Schema.EM_InterchangeStatus,
						EDIMessage.Schema.EM_ApplicationCode,
						EDIMessage.Schema.EM_ApplicationReference,
						EDIMessage.Schema.EM_MessageTextShort
					};
					eDIMessageFilterControlDefaultColumnsSequence = columnList.ToArray();
				}
				return eDIMessageFilterControlDefaultColumnsSequence;
			}
		}
		string[] eDIMessageFilterControlDefaultColumnsSequence;

		void CurrentCellChanged_Click(object sender, EventArgs e)
		{
			if (FilteredGrid.CurrentRowIndex >= 0)
			{
				int columnNumber = FilteredGrid.CurrentCell.ColumnNumber;
				if (columnNumber >= 0 && columnNumber < FilteredGrid.Columns.Count &&
					FilteredGrid.Columns[columnNumber].ColumnStyle.MappingName == EDIMessage.Schema.LinkedObjectReference &&
					IsCursorWithinBoundsOfCurrentCell)
				{
					var rowObject = ((ManifestForwardEDIMessageCollection)FilteredGrid.DataSource)[FilteredGrid.CurrentRowIndex];
					if (rowObject != null)
					{
						ChildEditableService.SetState(rowObject.Factory, ChildEditableServiceStates.Shipment);
						var shipment = rowObject.EM_LinkedObject as Freight.Forwarding.Business.ForwardingShipment;
						var entryHeader = rowObject.EM_LinkedObject as CusEntryHeader;
						if (entryHeader != null && entryHeader.Declaration != null)
						{
							if (entryHeader.Declaration.Shipment != null)
							{
								shipment = entryHeader.Declaration.Shipment;
							}
							else
							{
								ShowDeclaration(entryHeader.Declaration);
							}
						}
						if (shipment != null)
						{
							ShowShipment(shipment);
						}
					}
				}
			}
		}

		protected virtual void ShowDeclaration(JobDeclaration declaration)
		{
			var controller = (JobDeclarationController)ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
			controller.ShowEditForm(declaration);
		}

		protected virtual IZForm ShowShipment(Freight.Forwarding.Business.ForwardingShipment shipment)
		{
			ZController controller;
			if (shipment.JS_UniqueConsignRef.StartsWith("H"))
			{
				controller = (Freight.CFS.Module.ShipmentReceivalController)ZControllerFactory.Create(ControllerIDs.ShipmentReceival);
			}
			else
			{
				controller = (Freight.Forwarding.Module.JobShipmentController)ZControllerFactory.Create(ControllerIDs.JobShipment);
			}
			return controller.ShowEditForm(shipment);
		}

		protected virtual bool IsCursorWithinBoundsOfCurrentCell
		{
			get { return FilteredGrid.GetCurrentCellBounds().Contains(FilteredGrid.PointToClient(Control.MousePosition)); }
		}

		void UnderlineLinkedObjectReference(object sender, FontDecidingEventArgs e)
		{
			if (e.DataMember == EDIMessage.Schema.LinkedObjectReference)
			{
				e.Font = new Font(e.OriginalFont, FontStyle.Underline);
			}
		}
	}
}
