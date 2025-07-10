using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(NationalAdditionalCodeCollection))]
	class NationalAdditionalCodeCollectionTest : CusCodeDataCollectionTest<NationalAdditionalCode>
	{
		protected override CusCodeDataCollection<NationalAdditionalCode> GetCusCodeDataCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return new NationalAdditionalCodeCollection(invoiceLine.JI_NationalAdditionalCodesInfo, 8);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<NationalAdditionalCode>();
			result.CY_Order = 3;
			return result;
		}
	}
}
