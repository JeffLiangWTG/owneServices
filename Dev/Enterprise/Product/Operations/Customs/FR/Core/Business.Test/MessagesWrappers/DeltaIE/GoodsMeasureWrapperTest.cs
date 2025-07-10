using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class GoodsMeasureWrapperTest : DataProviderTestCase<GoodsMeasureWrapper>
	{
		public void TestGrossMass()
		{
			AssertEquals("GrossMass should equal line.JI_Weight.", 11d, Provider.GrossMass);
		}

		public void TestNationalSupplementaryUnits()
		{
			AssertContainsExactElementsInAnyOrder("NationalSupplementaryUnits should map second and third qty if any.", new string[] { "44|TNE1", "22|KG" }, Provider.NationalSupplementaryUnits.Select(x => x.NationalSupplementaryUnits.ToString() + "|" + x.NationalMeasurementUnitAndQualifier));
		}

		public void TestNationalSupplementaryUnitsWhenNoSecondQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_CustomsThirdUnitQty = "KG";
			line1.JI_CustomsThirdQuantity = 2d;

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = (CusEntryLine)line1.CusEntryLine;

			var provider = GoodsMeasureWrapper.New(entryLine);
			AssertContainsExactElementsInAnyOrder("NationalSupplementaryUnits should map third qty only when second qty is empty.", new string[] { "2|KG" }, provider.NationalSupplementaryUnits.Select(x => x.NationalSupplementaryUnits.ToString() + "|" + x.NationalMeasurementUnitAndQualifier));
		}

		public void TestNationalSupplementaryUnitsWhenNoThirdQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_CustomsSecondUnitQty = "TNE1";
			line1.JI_CustomsSecondQuantity = 4d;

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = (CusEntryLine)line1.CusEntryLine;

			var provider = GoodsMeasureWrapper.New(entryLine);
			AssertContainsExactElementsInAnyOrder("NationalSupplementaryUnits should map second qty only when third qty is empty.", new string[] { "4|TNE1" }, provider.NationalSupplementaryUnits.Select(x => x.NationalSupplementaryUnits.ToString() + "|" + x.NationalMeasurementUnitAndQualifier));
		}

		public void TestNationalSupplementaryUnits_SecondQuantityAndThirdQuantity_BothEmpty()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = invoiceHeader.InvoiceLines.AddNew();
	
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = (CusEntryLine)line1.CusEntryLine;

			var provider = GoodsMeasureWrapper.New(entryLine);
			AssertContainsExactElementsInAnyOrder("NationalSupplementaryUnits should be empty when both second qty and third qty are empty.", System.Array.Empty<string>(), provider.NationalSupplementaryUnits.Select(x => x.NationalSupplementaryUnits.ToString() + "|" + x.NationalMeasurementUnitAndQualifier));
		}

		public void TestNetMass()
		{
			AssertEquals("NetMass should equal sum of lines JI_Weight.", 33d, Provider.NetMass);
		}

		public void TestSupplementaryUnits()
		{
			AssertEquals("SupplementaryUnits should equal sum of lines JI_CustomsSecondQuantity.", 44d, Provider.SupplementaryUnits);
		}

		protected override GoodsMeasureWrapper GetProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_Weight = 1d;
			line1.JI_CustomsThirdUnitQty = "KG";
			line1.JI_CustomsThirdQuantity = 2d;
			line1.JI_NetWeight = 3d;
			line1.JI_CustomsSecondUnitQty = "TNE1";
			line1.JI_CustomsSecondQuantity = 4d;

			var line2 = invoiceHeader.InvoiceLines.AddNew();
			line2.JI_Weight = 10d;
			line2.JI_CustomsThirdUnitQty = "KG";
			line2.JI_CustomsThirdQuantity = 20d;
			line2.JI_NetWeight = 30d;
			line2.JI_CustomsSecondUnitQty = "TNE1";
			line2.JI_CustomsSecondQuantity = 40d;

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = (CusEntryLine)line1.CusEntryLine;

			return GoodsMeasureWrapper.New(entryLine);
		}
	}
}
