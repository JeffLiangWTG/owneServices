using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(UAInvoice))]
	public class UAInvoiceTestExtra : APInvoiceTest
	{
		public new void TestResetBankAccountCollectionOnAH_GB()
		{
			Assert("Not Applicable", true);
		}

		public new void TestValidationForIncompleteTransaction()
		{
			Assert(true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			UAInvoice result = Factory.New<UAInvoice>();
			return result;
		}

		protected override bool CouldHaveAssociatedDraftInvoice => false;
	}
}
