using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class ManifestForwardFilterControlTest : TestCaseWithFactory
	{
		public void TestCurrentCellChangedEvent()
		{
			var message1 = Factory.New<ACIHouseBillMessage>();
			message1.EM_MessageSubType = ACIForwarderReceivedMessageTypes.Codes.ManifestForward;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageText = "UNH+1+GOVCBR:D:11B:UN'BGM+714+8036X555+4'RFF+AFM:10207:CB'RFF+UCN:UCR555'UNT+4+1'UNE+1+13'";
			var message2 = Factory.New<ACIHouseBillMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;
			message2.EM_MessageSubType = ACIForwarderReceivedMessageTypes.Codes.ManifestForward;
			message2.EM_ApplicationReference = "8036X556";
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageText = "UNH+1+GOVCBR:D:11B:UN'BGM+714+8036X556+4'RFF+AFM:10207:CB'RFF+UCN:UCR555'UNT+4+1'UNE+1+13'";
			message2.EM_Status = EDIMessage.Status.Received;
			var message3 = Factory.New<ACIHouseBillMessage>();
			message3.EM_MessageSubType = ACIForwarderReceivedMessageTypes.Codes.ManifestForward;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message3.EM_MessageText = "UNH+1+GOVCBR:D:11B:UN'BGM+714+8036X557+4'RFF+AFM:10207:CB'RFF+UCN:UCR555'UNT+4+1'UNE+1+13'";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var number = declaration.AdditionalReferenceNumbers.AddNew();
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			number.CE_EntryType = Customs.Common.CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "8036X556";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			var shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
			message3.EM_LinkedObject = shipment;
			Factory.Save();
			AssertEquals("Declaration attached to message2", entryHeader.PK, message2.EM_LinkedObject.PK);
			AssertEquals("Shipment attached to message3", shipment.PK, message3.EM_LinkedObject.PK);

			var collection = new ManifestForwardEDIMessageCollection(Factory);
			collection.Add(message1);
			collection.Add(message2);
			collection.Add(message3);
			collection.Sort(EDIMessage.Schema.EM_MessageText, System.ComponentModel.ListSortDirection.Ascending);

			using (var manifestForwardFilterControl = new ManifestForwardFilterControlForTesting(collection, new ManifestForwardFilterBusinessObject()))
			{
				manifestForwardFilterControl.OnLoadInternal(EventArgs.Empty);
#if WINZOR
				manifestForwardFilterControl.FilteredGrid.CreateControl();
#endif
				var linkedObjectReferenceIndex = manifestForwardFilterControl.FilteredGrid.ColumnStyles.IndexOf(manifestForwardFilterControl.FilteredGrid.GetColumnStyle(EDIMessage.Schema.LinkedObjectReference));
				manifestForwardFilterControl.IsTestingDeclarationOrShipmentCalling = true;
				manifestForwardFilterControl.TestingDeclarationOrShipmentCallingResult = ZString.Empty;
				var rect = manifestForwardFilterControl.FilteredGrid.GetCellBounds(0, linkedObjectReferenceIndex);
				Cursor.Position = manifestForwardFilterControl.FilteredGrid.PointToScreen(new Point(rect.Left, rect.Top));
				manifestForwardFilterControl.FilteredGrid.CurrentCell = new DataGridCell(0, linkedObjectReferenceIndex);
				AssertEquals("No Linked object to open", ZString.Empty, manifestForwardFilterControl.TestingDeclarationOrShipmentCallingResult);
				rect = manifestForwardFilterControl.FilteredGrid.GetCellBounds(1, linkedObjectReferenceIndex);
				Cursor.Position = manifestForwardFilterControl.FilteredGrid.PointToScreen(new Point(rect.Left, rect.Top));
				manifestForwardFilterControl.FilteredGrid.CurrentCell = new DataGridCell(1, linkedObjectReferenceIndex);
				AssertEquals("Declaration was opened", "DECLARATION", manifestForwardFilterControl.TestingDeclarationOrShipmentCallingResult);
				manifestForwardFilterControl.TestingDeclarationOrShipmentCallingResult = ZString.Empty;
				rect = manifestForwardFilterControl.FilteredGrid.GetCellBounds(2, linkedObjectReferenceIndex);
				Cursor.Position = manifestForwardFilterControl.FilteredGrid.PointToScreen(new Point(rect.Left, rect.Top));
				manifestForwardFilterControl.FilteredGrid.CurrentCell = new DataGridCell(2, linkedObjectReferenceIndex);
				AssertEquals("Shipment was opened", "SHIPMENT", manifestForwardFilterControl.TestingDeclarationOrShipmentCallingResult);
				AssertEquals("Should set Shipment State", ChildEditableServiceStates.Shipment, ChildEditableService.GetState(shipment.Factory));
			}
		}

		public void TestFilterControl()
		{
			using (var module = new ManifestForwardModule())
			{
				using (var form = new ZChildForm(module.GridCollection))
				{
					var control = (ManifestForwardFilterControl)module.EmbeddedControl;
					form.Controls.Add(control);
					form.Show();
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.SNPType));
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.CargoControlNumber));
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.LinkedObjectReference));
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.TransactionNumber));
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.PrimaryCargoControlNumber));
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.SubLocation));
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.CBSAOffice));

					var grid = control.FilteredGrid;
					grid.ResetColumns();
					Assert("ManifestForwardFilterControl grid count must have at least " + ExpectedColumnNamesInSortOrderList.Count.ToString(), ExpectedColumnNamesInSortOrderList.Count <= grid.Columns.Count);
					for (int i = 0; i < ExpectedColumnNamesInSortOrderList.Count; i++)
					{
						var column = grid.Columns[i];
						AssertNotNull(column);
						var expectedColumnName = ExpectedColumnNamesInSortOrderList[i];
						AssertEquals("Expected", expectedColumnName, column.ColumnStyle.MappingName);
						AssertEquals("IsVisible", true, column.IsVisible);
					}
				}
			}
		}

		List<ZString> ExpectedColumnNamesInSortOrderList
		{
			get
			{
				if (expectedColumnNamesInSortOrderList == null)
				{
					expectedColumnNamesInSortOrderList = new List<ZString>();
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.SNPType);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.CargoControlNumber);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.LinkedObjectReference);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.TransactionNumber);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.PrimaryCargoControlNumber);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.SubLocation);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.CBSAOffice);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageDateTime);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageType);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageSubType);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageSubTypeDescription);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_InterchangeSender);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_SendingUser);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_Status);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_SendOrReceiveHumanReadable);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageNum);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_DateTimeInterchangeSent);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_InterchangeNumber);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_InterchangeStatus);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_ApplicationCode);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_ApplicationReference);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageTextShort);
				}
				return expectedColumnNamesInSortOrderList;
			}
		}
		List<ZString> expectedColumnNamesInSortOrderList;

		sealed class ManifestForwardFilterControlForTesting : ManifestForwardFilterControl
		{
			internal ManifestForwardFilterControlForTesting(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
				: base(gridCollection, filterBusinessObject)
			{
			}

			internal void OnLoadInternal(EventArgs e) => OnLoad(e);

			internal bool IsTestingDeclarationOrShipmentCalling { get; set; }

			internal string TestingDeclarationOrShipmentCallingResult { get; set; }

			protected override void ShowDeclaration(JobDeclaration declaration)
			{
				TestingDeclarationOrShipmentCallingResult = "DECLARATION";
			}

			protected override IZForm ShowShipment(Freight.Forwarding.Business.ForwardingShipment shipment)
			{
				base.ShowShipment(shipment).Dispose();
				TestingDeclarationOrShipmentCallingResult = "SHIPMENT";
				return null;
			}

			protected override bool IsCursorWithinBoundsOfCurrentCell => true;
		}
	}
}
