using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.ZMGermany;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.ZMGermany
{
	[TestedType(typeof(ZMTaxReturnLinesCollectionView))]
	public class ZMTaxReturnLinesCollectionViewTest : BusinessObjectCollectionViewTestCase<ZMTaxReturnLinesCollectionView>
	{
		public void TestFiltering()
		{
			var dummyCollection = new DummyBusinessObjectCollection(Factory);
			dummyCollection.AddNew();
			dummyCollection.AddNew();

			var taxReturnLinesCollectionView = new ZMTaxReturnLinesCollectionView(dummyCollection, (reportLine) => true);
			AssertContainsExactElementsInAnyOrder("Not a ZMTaxReturnLines collection to filter", System.Array.Empty<AccComplianceReportLine>(), taxReturnLinesCollectionView);

			var taxReturnLinesCollection = new ZMTaxReturnLinesCollection(Factory);
			var line1 = CreateTaxReturnLine(1);
			var line2 = CreateTaxReturnLine(1);
			var line3 = CreateTaxReturnLine(2);
			taxReturnLinesCollection.Add(line1);
			taxReturnLinesCollection.Add(line2);
			taxReturnLinesCollection.Add(line3);

			taxReturnLinesCollectionView = new ZMTaxReturnLinesCollectionView(taxReturnLinesCollection, null);
			AssertContainsExactElementsInAnyOrder("No predicate => no tax return line", System.Array.Empty<AccTaxReturnLine>(), taxReturnLinesCollectionView);

			taxReturnLinesCollectionView = new ZMTaxReturnLinesCollectionView(taxReturnLinesCollection, (line) => line.TaxReturn.ATR_Version == 1);
			AssertContainsExactElementsInAnyOrder(new AccTaxReturnLine[] { line1, line2 }, taxReturnLinesCollectionView);

			var line4 = CreateTaxReturnLine(1);
			taxReturnLinesCollection.Add(line4);
			AssertContainsExactElementsInAnyOrder(new AccTaxReturnLine[] { line1, line2, line4 }, taxReturnLinesCollectionView);

			var line5 = CreateTaxReturnLine(2);
			taxReturnLinesCollection.Add(line5);
			AssertContainsExactElementsInAnyOrder(new AccTaxReturnLine[] { line1, line2, line4 }, taxReturnLinesCollectionView);
		}

		AccTaxReturnLine CreateTaxReturnLine(ZByte version)
		{
			var line = (AccTaxReturnLine)GetNewElementToAddToTheCollection();
			line.TaxReturn.ATR_Version = version;
			return line;
		}

		protected override ZMTaxReturnLinesCollectionView GetCollectionToTest()
		{
			return new ZMTaxReturnLinesCollectionView(new ZMTaxReturnLinesCollection(Factory), (reportLine) => true);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var taxReturn = new TestObjectCreator(Factory).CreateAccTaxReturn(addTaxReturnLine: true);
			return taxReturn.Lines[0];
		}
	}
}
