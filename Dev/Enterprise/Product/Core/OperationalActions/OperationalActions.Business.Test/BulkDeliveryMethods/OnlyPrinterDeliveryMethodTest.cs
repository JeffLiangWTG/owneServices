using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ContactNotifyModes = Enterprise.Core.Constants.ContactNotifyModes;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(OnlyPrinterBulkDeliveryMethod))]
	internal sealed class OnlyPrinterDeliveryMethodTest : BulkDeliveryMethodTest<OnlyPrinterBulkDeliveryMethod>
	{
		public void TestSetRecipients()
		{
			DocDeliveryContactCollection contacts = new DocDeliveryContactCollection(Factory);
			contacts.Add(PrintContact);
			contacts.Add(FaxContact);
			contacts.Add(EmailContact);
			Method.SetRecipients(Instructions, contacts);
			AssertEquals(1, Instructions.Recipients.Count);
			AssertEquals(Instructions.Recipients[0], PrintContact);
			AssertEquals(ContactNotifyModes.Print, PrintContact.DeliveryMethod);
		}

		public void TestAllowOverridePrintDetails()
		{
			var onlyPrinter = new OnlyPrinterBulkDeliveryMethod();
			Assert("Only Printer method should allow override printer", onlyPrinter.AllowOverridePrintDetails);
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
