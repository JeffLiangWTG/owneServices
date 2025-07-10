using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ContactNotifyModes = Enterprise.Core.Constants.ContactNotifyModes;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(ForcePrinterBulkDeliveryMethod))]
	internal sealed class ForcePrinterDeliveryMethodTest : BulkDeliveryMethodTest<ForcePrinterBulkDeliveryMethod>
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
			AssertEquals(ContactNotifyModes.Print, FaxContact.DeliveryMethod);
			AssertEquals(ContactNotifyModes.Print, EmailContact.DeliveryMethod);
		}

		public void TestAllowOverridePrintDetails()
		{
			var forcePrinter = new ForcePrinterBulkDeliveryMethod();
			Assert("Force Printer method should allow override printer", forcePrinter.AllowOverridePrintDetails);
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
				return false;
			}
		}
		#endregion
	}
}
