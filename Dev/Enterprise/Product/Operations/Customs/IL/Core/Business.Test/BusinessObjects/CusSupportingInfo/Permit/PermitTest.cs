using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(Permit))]
	sealed class PermitTest : CusSupportingInfoTest<Permit>
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewPermit();

		protected override IEnumerable<Permit> GetBizObjsForCorrectlyTypeDecideTest(
			BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var permit = invoiceLine.Permits.AddNew();
			yield return permit;
		}

		Permit GetNewPermit()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			return invoiceLine.Permits.AddNew();
		}
	}
}
