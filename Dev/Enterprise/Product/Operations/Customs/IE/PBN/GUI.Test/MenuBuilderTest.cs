using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.PBN.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.GUI.Testing
{
	[TestedType(typeof(MenuBuilder))]
	class MenuBuilderTest : TestCaseWithFactory
	{
		public void TestBuildMenu()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			using var form = new ZForm(header);
			var builder = new MenuBuilder(header, form);
			var menuItems = builder.BuildMenu();
			AssertEquals("Menu item count", 6, menuItems.Length);
			AssertEquals("Send PBN menu item", "Send PBN", menuItems[0].Caption);
			AssertEquals("Create PBN menu item", "Create PBN", menuItems[1].Caption);
			AssertEquals("Update PBN menu item", "Update PBN", menuItems[2].Caption);
			AssertEquals("Update PBN Declarations menu item", "Update PBN Declarations", menuItems[3].Caption);
			AssertEquals("Lookup PBN menu item", "Lookup PBN", menuItems[4].Caption);
			AssertEquals("Lookup Channel menu item", "Lookup Channel", menuItems[5].Caption);
		}

		public void TestSendManifest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			using var form = new ZForm(header);
			var builder = new MenuBuilder(header, form);
			var menuItems = builder.BuildMenu();
			var menuItem = menuItems.First(x => x.Caption == "Send PBN");
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(shownForm =>
			{
				if (shownForm is PBNMessageSendingForm messageSendingForm)
				{
					var sendingObjectParent = messageSendingForm.DataSource as PBNMessageSendingObjectParent;
					var sendingObject = sendingObjectParent.SendingObjectsCollection[0];
					sendingObject.ShouldSend = true;
					sendingObject.MessageType = PBNMessageTypes.Codes.CreatePBN;
					sendingObject.JobNumber = "PBN001";
					sendingObject.MessageStatus = "SNT";
				}
			});

			menuItem.PerformClick();

			var messages = header.Messages;
			AssertEquals("Message Count", 1, header.Messages.Count);
			var message = header.Messages[0];
			AssertEquals("EM_MessageType", "CPB", message.EM_MessageType);
			AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", message.EM_Status);
			AssertEquals("EM_LinkedObject", header, message.EM_LinkedObject);
		}

		public void TestCreatePBN()
		{
			AssertSend(PBNMessageTypes.Codes.CreatePBN, PBNMessageTypes.Descriptions.CreatePBN);
		}

		public void TestUpdatePBN()
		{
			AssertSend(PBNMessageTypes.Codes.UpdatePBN, PBNMessageTypes.Descriptions.UpdatePBN);
		}

		public void TestUpdatePBNDeclarations()
		{
			AssertSend(PBNMessageTypes.Codes.UpdatePBNDeclarations, PBNMessageTypes.Descriptions.UpdatePBNDeclarations);
		}

		public void TestLookupPBN()
		{
			AssertSend(PBNMessageTypes.Codes.LookupPBN, PBNMessageTypes.Descriptions.LookupPBN);
		}

		public void TestLookupChannel()
		{
			AssertSend(PBNMessageTypes.Codes.LookupPBNChannel, "Lookup Channel");
		}

		void AssertSend(string messageType, string caption)
		{
			var header = Factory.New<AsycudaManifestHeader>();
			using var form = new ZForm(header);
			var builder = new MenuBuilder(header, form);
			var menuItems = builder.BuildMenu();
			var menuItem = menuItems.First(x => x.Caption == caption);
			Factory.Save();

			menuItem.PerformClick();

			var messages = header.Messages;
			AssertEquals("Message Count", 1, messages.Count);
			var message = messages[0];
			AssertEquals("EM_MessageType", messageType, message.EM_MessageType);
			AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", message.EM_Status);
			AssertEquals("EM_LinkedObject", header, message.EM_LinkedObject);
		}
	}
}
