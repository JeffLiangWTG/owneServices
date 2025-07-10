using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ContactNotifyModes = Enterprise.Core.Constants.ContactNotifyModes;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(AutoBulkDeliveryMethod))]
	internal sealed class AutoDeliveryMethodTest : BulkDeliveryMethodTest<AutoBulkDeliveryMethod>
	{
		public void TestSetRecipients()
		{
			DocDeliveryContactCollection contacts = new DocDeliveryContactCollection(Factory);
			contacts.Add(PrintContact);
			contacts.Add(FaxContact);
			contacts.Add(EmailContact);
			Method.SetRecipients(Instructions, contacts);
			AssertEquals(3, Instructions.Recipients.Count);
			AssertEquals(Instructions.Recipients[0], PrintContact);
			AssertEquals(Instructions.Recipients[1], FaxContact);
			AssertEquals(Instructions.Recipients[2], EmailContact);
			AssertEquals(ContactNotifyModes.Print, PrintContact.DeliveryMethod);
			AssertEquals(ContactNotifyModes.Fax, FaxContact.DeliveryMethod);
			AssertEquals(ContactNotifyModes.Email, EmailContact.DeliveryMethod);
		}

		#region Implementation
		protected override bool UsesPrinter
		{
			get
			{
				return true;
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
