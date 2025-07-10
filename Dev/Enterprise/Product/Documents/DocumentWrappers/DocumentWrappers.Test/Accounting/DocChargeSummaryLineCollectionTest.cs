using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocChargeSummaryLineCollection))]
	sealed class DocChargeSummaryLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocChargeSummaryLineCollection>
	{
		protected override DocChargeSummaryLineCollection GetCollectionToTest()
		{
			return new DocChargeSummaryLineCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			DocTaxRate docTaxRate = DocTaxRate.New(taxRate, Factory);
			return DocChargeSummaryLine.New(new ChargeSummaryLine(docTaxRate, 0, 0, "this is a test", 100, 100, 110, 110, 10, 10), Factory);
		}
	}
}
