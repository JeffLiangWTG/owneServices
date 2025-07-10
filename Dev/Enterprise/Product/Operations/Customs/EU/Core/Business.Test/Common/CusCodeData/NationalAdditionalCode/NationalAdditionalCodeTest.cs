using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(NationalAdditionalCode))]
	sealed class NationalAdditionalCodeTest : CusCodeDataWithOrderAbstractTest<NationalAdditionalCode>
	{
		protected override IEnumerable<NationalAdditionalCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			yield return invoiceLine.NationalAdditionalCodes.AddNew();
		}

		protected override string ExpectedCusCodeDataType => CusCodeDataTypeList.Codes.NationalAdditionalCode;
	}
}
