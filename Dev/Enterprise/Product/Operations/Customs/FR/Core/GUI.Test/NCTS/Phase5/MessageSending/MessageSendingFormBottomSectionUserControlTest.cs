using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.FR.NCTS.Messaging;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	class MessageSendingFormBottomSectionUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			using (var control = new MessageSendingFormBottomSectionUserControl())
			{
				AssertEquals(typeof(TP5MessageSendingObjectParent), control.BindingSource.DataSourceType);
			}
		}

		public void TestJustificationTextBoxAndBinding()
		{
			using (var form = new ZForm(messageSendingObjectParent))
			using (var control = new MessageSendingFormBottomSectionUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var justificationTextBox = control.JustificationTextBox;
				CombineAssertions(() =>
				{
					AssertType<ZTextBox>("Control Type", justificationTextBox);
					AssertEquals("BindTo", "SendingObjectsCollection.Justification", justificationTextBox.BindTo);
				});
			}
		}

		public void TestJustificationTextBoxVisibility()
		{
			CheckJustificationTextBoxVisibilityByMessageType("007", false);
			CheckJustificationTextBoxVisibilityByMessageType("015", false);
			CheckJustificationTextBoxVisibilityByMessageType("034", false);
			CheckJustificationTextBoxVisibilityByMessageType("044", false);
			CheckJustificationTextBoxVisibilityByMessageType("141", false);
			CheckJustificationTextBoxVisibilityByMessageType("170", false);

			CheckJustificationTextBoxVisibilityByMessageType("013", true);
			CheckJustificationTextBoxVisibilityByMessageType("014", true);
		}

		void CheckJustificationTextBoxVisibilityByMessageType(string messageType, bool expectedVisibility)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var messageSendingObjectParent = new TP5MessageSendingObjectParent(nctsHeader);
			((TP5MessageSendingObject)messageSendingObjectParent.SendingObjectsCollection.First()).MessageType = messageType;
			using (var form = new ZForm(messageSendingObjectParent))
			using (var control = new MessageSendingFormBottomSectionUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var justificationTextBox = control.JustificationTextBox;
				AssertEquals($"Justification control must {(expectedVisibility ? "" : "not")} be visible when message type is \"{messageType}\"", expectedVisibility, justificationTextBox.Visible);
			}
		}

		public void TestTC11DeliveryDate()
		{
			using (var form = new ZForm(messageSendingObjectParent))
			using (var control = new MessageSendingFormBottomSectionUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var tc11DeliveryDateEdit = control.TC11DeliveryDate;
				CombineAssertions(() =>
				{
					AssertType<ZDateEdit>("Control Type", tc11DeliveryDateEdit);
					AssertEquals("BindTo", "SendingObjectsCollection.TC11DeliveryDate", tc11DeliveryDateEdit.BindTo);
					AssertEquals("TC11DeliveryDate should not be visible when message type is not CC141C", false, tc11DeliveryDateEdit.Visible);

					var messageSendingAction = messageSendingObjectParent.SendingObjectsCollection[0];
					messageSendingAction.MessageType = TP5MessageTypeList.Codes.CC141C;
					AssertEquals("TC11DeliveryDate should be visible when message type is CC141C", true, tc11DeliveryDateEdit.Visible);
				});
			}
		}

		public void TestQueryInformationTextBox()
		{
			using (var form = new ZForm(messageSendingObjectParent))
			using (var control = new MessageSendingFormBottomSectionUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var queryInformationTextBox = control.QueryInformationTextBox;
				CombineAssertions(() =>
				{
					AssertType<ZTextBox>("Control Type", queryInformationTextBox);
					AssertEquals("BindTo", "SendingObjectsCollection.QueryInformation", queryInformationTextBox.BindTo);
					AssertEquals("QueryInformationTextBox should not be visible when message type is not CC141C", false, queryInformationTextBox.Visible);

					var messageSendingAction = messageSendingObjectParent.SendingObjectsCollection[0];
					messageSendingAction.MessageType = TP5MessageTypeList.Codes.CC141C;
					AssertEquals("QueryInformationTextBox should be visible when message type is CC141C", true, queryInformationTextBox.Visible);
				});
			}
		}

		public void TestActualConsigneeLabel()
		{
			using (var form = new ZForm(messageSendingObjectParent))
			using (var control = new MessageSendingFormBottomSectionUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var actualConsigneeLabel = control.ActualConsigneeLabel;
				CombineAssertions(() =>
				{
					AssertType<ZLabel>("Control Type", actualConsigneeLabel);
					AssertEquals("ActualConsigneeDocAddressControl should not be visible when message type is not CC141C", false, actualConsigneeLabel.Visible);

					var messageSendingAction = messageSendingObjectParent.SendingObjectsCollection[0];
					messageSendingAction.MessageType = TP5MessageTypeList.Codes.CC141C;
					AssertEquals("ActualConsigneeLabel should be visible when message type is CC141C", true, actualConsigneeLabel.Visible);
				});
			}
		}

		public void TestActualConsigneeDocAddressControl()
		{
			using (var form = new ZForm(messageSendingObjectParent))
			using (var control = new MessageSendingFormBottomSectionUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var actualConsigneeDocAddressControl = control.ActualConsigneeDocAddressControl;
				CombineAssertions(() =>
				{
					AssertType<ZDocAddressControl>("Control Type", actualConsigneeDocAddressControl);
					AssertEquals("BindTo", "SendingObjectsCollection.ActualConsignee", actualConsigneeDocAddressControl.BindTo);
					AssertEquals("ActualConsigneeDocAddressControl should not be visible when message type is not CC141C", false, actualConsigneeDocAddressControl.Visible);

					var messageSendingAction = messageSendingObjectParent.SendingObjectsCollection[0];
					messageSendingAction.MessageType = TP5MessageTypeList.Codes.CC141C;
					AssertEquals("ActualConsigneeDocAddressControl should be visible when message type is CC141C", true, actualConsigneeDocAddressControl.Visible);
				});
			}
		}

		public void TestActualOfficeOfDestinationFindBox()
		{
			using (var form = new ZForm(messageSendingObjectParent))
			using (var control = new MessageSendingFormBottomSectionUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var actualOfficeOfDestinationFindBox = control.ActualOfficeOfDestinationFindBox;
				CombineAssertions(() =>
				{
					AssertType<ZCodeFindBox>("Control Type", actualOfficeOfDestinationFindBox);
					AssertEquals("BindTo", "SendingObjectsCollection.ActualOfficeOfDestination", actualOfficeOfDestinationFindBox.BindTo);
					AssertEquals("ActualOfficeOfDestinationFindBox should not be visible when message type is not CC141C", false, actualOfficeOfDestinationFindBox.Visible);

					var messageSendingAction = messageSendingObjectParent.SendingObjectsCollection[0];
					messageSendingAction.MessageType = TP5MessageTypeList.Codes.CC141C;
					AssertEquals("ActualOfficeOfDestinationFindBox should be visible when message type is CC141C", true, actualOfficeOfDestinationFindBox.Visible);
				});
			}
		}

		public void TestQueryIdentifierDropEdit()
		{
			using (var form = new ZForm(messageSendingObjectParent))
			using (var control = new MessageSendingFormBottomSectionUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var queryIdentifierDropEdit = control.QueryIdentifierDropEdit;
				CombineAssertions(() =>
				{
					AssertType<ZDropEdit>("Control Type", queryIdentifierDropEdit);
					AssertEquals("BindTo", "SendingObjectsCollection.QueryIdentifier", queryIdentifierDropEdit.BindTo);
					AssertEquals("Caption", "Query Identifier", queryIdentifierDropEdit.CaptionResourceString.Caption);
					AssertEquals("ShowDescriptionBox", true, queryIdentifierDropEdit.ShowDescriptionBox);

					AssertEquals("QueryIdentifierDropEdit should not be visible when message type is not CC034C", false, queryIdentifierDropEdit.Visible);

					var messageSendingAction = messageSendingObjectParent.SendingObjectsCollection[0];
					messageSendingAction.MessageType = TP5MessageTypeList.Codes.CC034C;
					AssertEquals("QueryIdentifierDropEdit should be visible when message type is CC034C", true, queryIdentifierDropEdit.Visible);
				});
			}
		}

		public void TestQueryPeriodFromDateEdit()
		{
			using (var form = new ZForm(messageSendingObjectParent))
			using (var control = new MessageSendingFormBottomSectionUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var queryPeriodFromDateEdit = control.QueryPeriodFromDateEdit;
				CombineAssertions(() =>
				{
					AssertType<ZDateEdit>("Control Type", queryPeriodFromDateEdit);
					AssertEquals("BindTo", "SendingObjectsCollection.PeriodFrom", queryPeriodFromDateEdit.BindTo);
					AssertEquals("Caption", "Period From", queryPeriodFromDateEdit.CaptionResourceString.Caption);

					AssertEquals("QueryPeriodFromDateEdit should not be visible when message type is not CC034C", false, queryPeriodFromDateEdit.Visible);

					var messageSendingAction = messageSendingObjectParent.SendingObjectsCollection[0];
					messageSendingAction.MessageType = TP5MessageTypeList.Codes.CC034C;
					AssertEquals("QueryPeriodFromDateEdit should be visible when message type is CC034C", true, queryPeriodFromDateEdit.Visible);
				});
			}
		}

		public void TestQueryPeriodToDateEdit()
		{
			using (var form = new ZForm(messageSendingObjectParent))
			using (var control = new MessageSendingFormBottomSectionUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var queryPeriodToDateEdit = control.QueryPeriodToDateEdit;
				CombineAssertions(() =>
				{
					AssertType<ZDateEdit>("Control Type", queryPeriodToDateEdit);
					AssertEquals("BindTo", "SendingObjectsCollection.PeriodTo", queryPeriodToDateEdit.BindTo);
					AssertEquals("Caption", "To", queryPeriodToDateEdit.CaptionResourceString.Caption);

					AssertEquals("QueryPeriodToDateEdit should not be visible when message type is not CC034C", false, queryPeriodToDateEdit.Visible);

					var messageSendingAction = messageSendingObjectParent.SendingObjectsCollection[0];
					messageSendingAction.MessageType = TP5MessageTypeList.Codes.CC034C;
					AssertEquals("QueryPeriodToDateEdit should be visible when message type is CC034C", true, queryPeriodToDateEdit.Visible);
				});
			}
		}

		public void TestRequesterIDTextBox()
		{
			using (var form = new ZForm(messageSendingObjectParent))
			using (var control = new MessageSendingFormBottomSectionUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var requesterIDTextBox = control.RequesterIDTextBox;
				CombineAssertions(() =>
				{
					AssertType<ZTextBox>("Control Type", requesterIDTextBox);
					AssertEquals("BindTo", "SendingObjectsCollection.RequesterId", requesterIDTextBox.BindTo);
					AssertEquals("Caption", "Requester", requesterIDTextBox.CaptionResourceString.Caption);
					AssertEquals("ReadOnly", true, requesterIDTextBox.ReadOnly);

					AssertEquals("RequesterIDTextBox should not be visible when message type is not CC034C", false, requesterIDTextBox.Visible);

					var messageSendingAction = messageSendingObjectParent.SendingObjectsCollection[0];
					messageSendingAction.MessageType = TP5MessageTypeList.Codes.CC034C;
					AssertEquals("RequesterIDTextBox should be visible when message type is CC034C", true, requesterIDTextBox.Visible);
				});
			}
		}

		public void TestRequesterRoleDropEdit()
		{
			using (var form = new ZForm(messageSendingObjectParent))
			using (var control = new MessageSendingFormBottomSectionUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var requesterRoleDropEdit = control.RequesterRoleDropEdit;
				CombineAssertions(() =>
				{
					AssertType<ZDropEdit>("Control Type", requesterRoleDropEdit);
					AssertEquals("BindTo", "SendingObjectsCollection.RequesterRole", requesterRoleDropEdit.BindTo);
					AssertEquals("Caption", "Role", requesterRoleDropEdit.CaptionResourceString.Caption);
					AssertEquals("ShowDescriptionBox", true, requesterRoleDropEdit.ShowDescriptionBox);

					AssertEquals("RequesterRoleDropEdit should not be visible when message type is not CC034C", false, requesterRoleDropEdit.Visible);

					var messageSendingAction = messageSendingObjectParent.SendingObjectsCollection[0];
					messageSendingAction.MessageType = TP5MessageTypeList.Codes.CC034C;
					AssertEquals("RequesterRoleDropEdit should be visible when message type is CC034C", true, requesterRoleDropEdit.Visible);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			messageSendingObjectParent = new TP5MessageSendingObjectParent(nctsHeader);
		}

		NctsHeader nctsHeader;
		TP5MessageSendingObjectParent messageSendingObjectParent;
	}
}
