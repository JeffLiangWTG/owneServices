using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(LinkedeNettEDIMessageController))]
	public class LinkedeNettEDIMessageControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.LinkedeNettEDIMessage;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			LinkedeNettEDIMessage result = Factory.New<InvoiceLinkedeNettEDIMessage>();
			Factory.Save();
			return result;
		}

		public override void TestDeleteForm()
		{
			Assert(true);
		}

		public override void TestEditForm()
		{
			Assert(true);
		}

		public override void TestViewForm()
		{
			Assert(true);
		}

		public void TestGetForm()
		{
			EDIMessage message1 = Factory.NewWithValidTestData<EDIMessage>();
			message1.EM_MessageType = "ENE";
			message1.EM_MessageSubType = eNettMessageSubTypeList.Codes.GetNewInvoices;

			EDIMessage message2 = Factory.NewWithValidTestData<EDIMessage>();
			message2.EM_MessageType = "ENE";
			message2.EM_MessageSubType = eNettMessageSubTypeList.Codes.ProcessDirectDebit;

			Factory.Save();

			using (IZForm form = Controller.ShowViewForm(message1))
			{
				Assert("Should return a LinkedeNettEDIMessageForm", form is LinkedeNettEDIMessageForm);
			}
			using (IZForm form = Controller.ShowViewForm(message2))
			{
				Assert("Should return an EDIMessageForm", form is EDIMessageForm);
			}
		}
	}
}
