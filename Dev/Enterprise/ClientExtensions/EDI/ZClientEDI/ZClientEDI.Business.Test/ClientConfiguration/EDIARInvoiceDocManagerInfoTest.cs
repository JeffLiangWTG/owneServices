using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DocManager.Business.Test
{
	[TestedType(typeof(EDIARInvoiceDocManagerInfo))]
	public class EDIARInvoiceDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<ARInvoice>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Factory.New<ARInvoice>();
		}

		public void TestReadOnly()
		{
			ARInvoice header = Factory.New<ARInvoice>();
			AssertEquals("readonly must always be false", false, ((IDocManagerSupport)header).DocManagerInfo.ReadOnly);

			header.ReadOnly = true;
			AssertEquals("readonly must always be false, even if parent is true", false, ((IDocManagerSupport)header).DocManagerInfo.ReadOnly);
		}
	}
}
