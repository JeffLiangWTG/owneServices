using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(OverrideMatchStatusHelper))]
	public class OverrideMatchStatusHelperTest : OverrideInvoiceDetailsHelperTest
	{
		protected override void AssertBusinessContext(InvoicingBase invoice)
		{
			Assert("Invoice is in correct context", invoice.HasContext(BusinessContext.OverrideMatchStatus));
		}

		protected override void AssertWritableColumns(InvoicingBase invoice)
		{
			AssertEquals("invoice.AH_MatchStatus.ReadOnly", false, invoice.AH_MatchStatusInfo.ReadOnly);
			AssertEquals("invoice.AH_MatchStatusReasonCode.ReadOnly", false, invoice.AH_MatchStatusReasonCodeInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return InvoicePKs != null ? new OverrideMatchStatusHelper(Factory, InvoicePKs) : new OverrideMatchStatusHelper(Factory, Factory.NewWithValidTestData<ARInvoice>().PK);
		}
	}
}
