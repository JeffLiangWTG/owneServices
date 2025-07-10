using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ARInvoiceDocManagerInfo))]
	public class ARInvoiceDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New(typeof(ARInvoice));
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Factory.New(typeof(ARInvoice));
		}

		public void TestOverriddenNewDelegate()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();

			DocManagerInfo info = ARInvoiceDocManagerInfo.New(invoice, invoice.AH_Ledger);
			AssertEquals("Info instantiated should be the original", typeof(ARInvoiceDocManagerInfo), info.GetType());

			ARInvoiceDocManagerInfoSubclass.RegisterThisSubTypeOverride();
			info = ARInvoiceDocManagerInfo.New(invoice, invoice.AH_Ledger);
			AssertEquals("Info instantiated should be the subclass", typeof(ARInvoiceDocManagerInfoSubclass), info.GetType());
		}
	}
}
