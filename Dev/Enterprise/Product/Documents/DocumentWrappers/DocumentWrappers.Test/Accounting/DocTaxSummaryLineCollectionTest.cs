using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocTaxSummaryLineCollection))]
	sealed class DocTaxSummaryLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocTaxSummaryLineCollection>
	{
		protected override DocTaxSummaryLineCollection GetCollectionToTest()
		{
			return new DocTaxSummaryLineCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocTaxSummaryLine.New(new TaxSummaryLine("RAT", 10, "", 0, 1, 100, 100, 10, 10, 110, 110, 0, 0), Factory);
		}
	}
}
