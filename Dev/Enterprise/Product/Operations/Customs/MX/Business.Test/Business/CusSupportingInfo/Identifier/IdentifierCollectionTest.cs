using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(IdentifierCollection))]
	class IdentifierCollectionTest : CusSupportingInfoCollectionTest<Identifier>
	{
		protected override Customs.Business.CusSupportingInfoCollection<Identifier> GetCusSupportingInfoCollection()
		{
			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
			return new IdentifierCollection(invoiceLine);
		}
	}
}
