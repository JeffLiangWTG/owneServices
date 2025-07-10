using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ContactNotifyModes = Enterprise.Core.Constants.ContactNotifyModes;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(OnlyElectronicBulkDeliveryMethod))]
	internal sealed class OnlyElectronicDeliveryMethodTest : BulkDeliveryMethodTest<OnlyElectronicBulkDeliveryMethod>
	{
		public void TestSetRecipients()
		{
			DocDeliveryContactCollection contacts = new DocDeliveryContactCollection(Factory);
			contacts.Add(PrintContact);
			contacts.Add(FaxContact);
			contacts.Add(EmailContact);
			Method.SetRecipients(Instructions, contacts);
			AssertEquals(2, Instructions.Recipients.Count);
			AssertEquals(Instructions.Recipients[0], FaxContact);
			AssertEquals(Instructions.Recipients[1], EmailContact);
			AssertEquals(ContactNotifyModes.Fax, FaxContact.DeliveryMethod);
			AssertEquals(ContactNotifyModes.Email, EmailContact.DeliveryMethod);
			using (Instructions.EnableOperationalActionDeliverDocumentsInOneEmail(true, Factory.New<DocumentCommand>()))
			{
				Instructions.Recipients.RemoveAll();
				Method.SetRecipients(Instructions, contacts);
				AssertEquals(1, Instructions.Recipients.Count);
				AssertEquals(Instructions.Recipients[0], EmailContact);
			}
		}

		#region Implementation
		protected override bool UsesPrinter
		{
			get
			{
				return false;
			}
		}

		protected override bool AllowCoverNote
		{
			get
			{
				return true;
			}
		}
		#endregion
	}
}
