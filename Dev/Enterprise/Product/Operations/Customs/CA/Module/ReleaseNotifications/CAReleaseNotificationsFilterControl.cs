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
	public partial class CAReleaseNotificationsFilterControl : Enterprise.Messaging.Module.EDIMessageFilterControl
	{
		public CAReleaseNotificationsFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("CAReleaseNotificationsFilterControl|AA7862B7-AEA9-4B30-8B4B-DA5A7B1A7FD4", "Job");
			zTextBoxColumnStyleInfo1.ColumnName = EDIMessage.Schema.LinkedObjectReference;
			zTextBoxColumnStyleInfo1.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo1, 70, true);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			var zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("CAReleaseNotificationsFilterControl|1A5B41EF-F6CA-491E-AFE7-AD3DAA291E13", "Tran. #", "Transaction #", "Transaction Number", "");
			zTextBoxColumnStyleInfo2.ColumnName = EDIMessage.Schema.TransactionNumber;
			zTextBoxColumnStyleInfo2.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo2, 100, true);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			var zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("CAReleaseNotificationsFilterControl|B1FE2DD0-C707-47BE-8DD5-9A2341100E1C", "CCN", "Cargo Control Number", "");
			zTextBoxColumnStyleInfo3.ColumnName = EDIMessage.Schema.CargoControlNumber;
			zTextBoxColumnStyleInfo3.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo3, 100, true);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			var zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("CAReleaseNotificationsFilterControl|64944209-98BB-4059-BFB1-2DC881E8B10A", "Sub-Loc.", "Sub-Location Code", "");
			zTextBoxColumnStyleInfo4.ColumnName = EDIMessage.Schema.SubLocation;
			zTextBoxColumnStyleInfo4.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo4, 50, true);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			var zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("CAReleaseNotificationsFilterControl|3FFC6950-C633-41EB-8570-14BC3A662B55", "CBSA Office", "CBSA Office code", "");
			zTextBoxColumnStyleInfo5.ColumnName = EDIMessage.Schema.CBSAOffice;
			zTextBoxColumnStyleInfo5.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo5, 50, true);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			var zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("CAReleaseNotificationsFilterControl|E1407053-65DA-4E83-AF48-F00A9C4BD06A", "Processed", "Processing Date", "");
			zDateEditColumnStyleInfo1.ColumnName = EDIMessage.Schema.RNSProcessingDate;
			zDateEditColumnStyleInfo1.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zDateEditColumnStyleInfo1, 100, true);
			FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			var zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("CAReleaseNotificationsFilterControl|C37F0695-EA6E-45A4-9D3F-54472D211E80", "Released", "Release Date", "");
			zDateEditColumnStyleInfo2.ColumnName = EDIMessage.Schema.RNSReleaseDate;
			zDateEditColumnStyleInfo2.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref zDateEditColumnStyleInfo2, 100, true);
			FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);

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
						EDIMessage.Schema.CargoControlNumber,
						EDIMessage.Schema.LinkedObjectReference,
						EDIMessage.Schema.TransactionNumber,
						EDIMessage.Schema.SubLocation,
						EDIMessage.Schema.CBSAOffice,
						EDIMessage.Schema.EM_MessageType,
						EDIMessage.Schema.EM_Status,
						EDIMessage.Schema.RNSReleaseDate,
						EDIMessage.Schema.RNSProcessingDate,
						EDIMessage.Schema.EM_MessageSubType,
						EDIMessage.Schema.EM_MessageSubTypeDescription,
						EDIMessage.Schema.EM_MessageDateTime,
						EDIMessage.Schema.EM_InterchangeSender,
						EDIMessage.Schema.EM_SendingUser,
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
					var rowObject = ((CAReleaseNotificationsCollection)FilteredGrid.DataSource)[FilteredGrid.CurrentRowIndex];
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
