using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class CAReleaseNotificationsFilterControlInLineTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestCurrentCellChangedEvent()
		{
			var message1 = Factory.New<EDIMessage>();
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message1.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageText = "1";
			var message2 = Factory.New<EDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message2.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageText = "2";
			var message3 = Factory.New<EDIMessage>();
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message3.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message3.EM_MessageText = "3";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			message2.EM_LinkedObject = entryHeader;
			var shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
			message3.EM_LinkedObject = shipment;
			Factory.Save();
			AssertEquals("Declaration attached to message2", entryHeader.PK, message2.EM_LinkedObject.PK);
			AssertEquals("Shipment attached to message3", shipment.PK, message3.EM_LinkedObject.PK);

			var collection = new CAReleaseNotificationsCollection(Factory);
			collection.Add(message1);
			collection.Add(message2);
			collection.Add(message3);
			collection.Sort(EDIMessage.Schema.EM_MessageText, System.ComponentModel.ListSortDirection.Ascending);

			using (CAReleaseNotificationsFilterControlForTesting cAReleaseNotificationsFilterControl = new CAReleaseNotificationsFilterControlForTesting(collection, new CAReleaseNotificationsFilterBusinessObject()))
			{
				cAReleaseNotificationsFilterControl.OnLoadInternal(EventArgs.Empty);
#if WINZOR
				cAReleaseNotificationsFilterControl.FilteredGrid.CreateControl();
#endif
				var linkedObjectReferenceIndex = cAReleaseNotificationsFilterControl.FilteredGrid.ColumnStyles.IndexOf(cAReleaseNotificationsFilterControl.FilteredGrid.GetColumnStyle(EDIMessage.Schema.LinkedObjectReference));
				cAReleaseNotificationsFilterControl.IsTestingDeclarationOrShipmentCalling = true;
				cAReleaseNotificationsFilterControl.TestingDeclarationOrShipmentCallingResult = ZString.Empty;
				var rect = cAReleaseNotificationsFilterControl.FilteredGrid.GetCellBounds(0, linkedObjectReferenceIndex);
				Cursor.Position = cAReleaseNotificationsFilterControl.FilteredGrid.PointToScreen(new Point(rect.Left, rect.Top));
				cAReleaseNotificationsFilterControl.FilteredGrid.CurrentCell = new DataGridCell(0, linkedObjectReferenceIndex);
				AssertEquals("No Linked object to open", ZString.Empty, cAReleaseNotificationsFilterControl.TestingDeclarationOrShipmentCallingResult);
				cAReleaseNotificationsFilterControl.IsTestingDeclarationOrShipmentCalling = true;
				cAReleaseNotificationsFilterControl.TestingDeclarationOrShipmentCallingResult = ZString.Empty;
				rect = cAReleaseNotificationsFilterControl.FilteredGrid.GetCellBounds(1, linkedObjectReferenceIndex);
				Cursor.Position = cAReleaseNotificationsFilterControl.FilteredGrid.PointToScreen(new Point(rect.Left, rect.Top));
				cAReleaseNotificationsFilterControl.FilteredGrid.CurrentCell = new DataGridCell(1, linkedObjectReferenceIndex);
				AssertEquals("Declaration was opened", "DECLARATION", cAReleaseNotificationsFilterControl.TestingDeclarationOrShipmentCallingResult);
				cAReleaseNotificationsFilterControl.IsTestingDeclarationOrShipmentCalling = true;
				cAReleaseNotificationsFilterControl.TestingDeclarationOrShipmentCallingResult = ZString.Empty;
				rect = cAReleaseNotificationsFilterControl.FilteredGrid.GetCellBounds(2, linkedObjectReferenceIndex);
				Cursor.Position = cAReleaseNotificationsFilterControl.FilteredGrid.PointToScreen(new Point(rect.Left, rect.Top));
				cAReleaseNotificationsFilterControl.FilteredGrid.CurrentCell = new DataGridCell(2, linkedObjectReferenceIndex);
				AssertEquals("Shipment was opened", "SHIPMENT", cAReleaseNotificationsFilterControl.TestingDeclarationOrShipmentCallingResult);
				AssertEquals("Should set Shipment State", ChildEditableServiceStates.Shipment, ChildEditableService.GetState(shipment.Factory));
			}
		}

		sealed class CAReleaseNotificationsFilterControlForTesting : CAReleaseNotificationsFilterControl
		{
			internal CAReleaseNotificationsFilterControlForTesting(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
				: base(gridCollection, filterBusinessObject) { }

			internal ZBool IsTestingDeclarationOrShipmentCalling { get; set; }

			internal ZString TestingDeclarationOrShipmentCallingResult { get; set; }

			internal void OnLoadInternal(EventArgs e) => OnLoad(e);

			protected override void ShowDeclaration(JobDeclaration declaration)
			{
				TestingDeclarationOrShipmentCallingResult = "DECLARATION";
			}

			protected override IZForm ShowShipment(Freight.Forwarding.Business.ForwardingShipment shipment)
			{
				TestingDeclarationOrShipmentCallingResult = "SHIPMENT";
				base.ShowShipment(shipment).Dispose();
				return null;
			}

			protected override bool IsCursorWithinBoundsOfCurrentCell => true;
		}
	}
}
