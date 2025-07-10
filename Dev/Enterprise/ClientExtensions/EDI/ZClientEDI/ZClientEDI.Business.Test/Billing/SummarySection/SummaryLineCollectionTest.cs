using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(SummaryLineCollection))]
	internal class SummaryLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SummaryLineCollection>
	{
		public void TestPopulateFromSummarySection()
		{
			SummarySection section = new SummarySection(Factory);
			section.Header.MainDescription = "header";
			section.Lines.AddNew().MainDescription = "line1";
			section.Lines.AddNew().MainDescription = "line2";
			section.Lines.AddNew().MainDescription = "line3";

			SummaryLineCollection collection = new SummaryLineCollection(Factory);
			AssertEquals("Precondition", 0, collection.Count);

			collection.PopulateFromSummarySection(section);
			AssertEquals("All section lines were added", 3, collection.Count);
			AssertSummaryLine(collection[0], "line1", "header");
			AssertSummaryLine(collection[1], "line2", "header");
			AssertSummaryLine(collection[2], "line3", "header");
		}

		public void TestPopulateFromSummarySections()
		{
			SummarySection section1 = new SummarySection(Factory);
			section1.Header.MainDescription = "header1";
			section1.Lines.AddNew().MainDescription = "line11";
			section1.Lines.AddNew().MainDescription = "line12";
			section1.Lines.AddNew().MainDescription = "line13";

			SummarySection section2 = new SummarySection(Factory);
			section2.Header.MainDescription = "header2";
			section2.Lines.AddNew().MainDescription = "line21";
			section2.Lines.AddNew().MainDescription = "line22";

			SummaryLineCollection collection = new SummaryLineCollection(Factory);
			AssertEquals("Precondition", 0, collection.Count);

			collection.PopulateFromSummarySections(new SummarySection[] { section1, section2 });
			AssertEquals("All section lines were added", 5, collection.Count);
			AssertSummaryLine(collection[0], "line11", "header1");
			AssertSummaryLine(collection[1], "line12", "header1");
			AssertSummaryLine(collection[2], "line13", "header1");
			AssertSummaryLine(collection[3], "line21", "header2");
			AssertSummaryLine(collection[4], "line22", "header2");
		}

		void AssertSummaryLine(SummaryLine line, string lineDescription, string headerDescription)
		{
			AssertEquals(lineDescription, line.MainDescription);
			AssertEquals(headerDescription, line.Header.MainDescription);
		}

		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
		}

		public void TestHasMultipleTaxes()
		{
			SummarySection section1 = new SummarySection(Factory);
			section1.Header.MainDescription = "header1";
			section1.Lines.AddNew().TaxCode = "Tax1";

			SummarySection section2 = new SummarySection(Factory);
			section2.Header.MainDescription = "header2";
			section2.Lines.AddNew().TaxCode = "Tax1";

			SummaryLineCollection collection = new SummaryLineCollection(Factory);
			AssertEquals("Precondition", 0, collection.Count);

			collection.PopulateFromSummarySections(new SummarySection[] { section1, section2 });
			AssertEquals("All section lines were added", 2, collection.Count);
			AssertEquals("Pre", "Tax1", collection[0].TaxCode);
			AssertEquals("Pre", "Tax1", collection[1].TaxCode);

			AssertEquals(false, collection.HasMultipleTaxes);

			collection = new SummaryLineCollection(Factory);
			AssertEquals("Precondition", 0, collection.Count);

			section1.Lines[0].TaxCode = "Tax1";
			section2.Lines[0].TaxCode = "Tax2";
			collection.PopulateFromSummarySections(new SummarySection[] { section1, section2 });
			AssertEquals("All section lines were added", 2, collection.Count);
			AssertEquals(true, collection.HasMultipleTaxes);
		}

		public void TestShowTaxCodes()
		{
			SummarySection section1 = new SummarySection(Factory);
			section1.Header.MainDescription = "header1";
			section1.Lines.AddNew().TaxCode = "Tax1";

			SummarySection section2 = new SummarySection(Factory);
			section2.Header.MainDescription = "header2";
			section2.Lines.AddNew().TaxCode = "Tax1";

			SummaryLineCollection collection = new SummaryLineCollection(Factory);
			AssertEquals("Precondition", 0, collection.Count);

			collection.PopulateFromSummarySections(new SummarySection[] { section1, section2 });
			AssertEquals("All section lines were added", 2, collection.Count);
			AssertEquals("Pre", "Tax1", collection[0].TaxCode);
			AssertEquals("Pre", "Tax1", collection[1].TaxCode);

			collection.ShowTaxCodes(false);
			Assert("tax not shown", collection[0].TaxCode.IsEmpty);
			Assert("tax not shown", collection[1].TaxCode.IsEmpty);
			Assert("tax not shown", collection[0].Header.TaxCode.IsEmpty);
			Assert("tax not shown", collection[1].Header.TaxCode.IsEmpty);

			collection = new SummaryLineCollection(Factory);
			AssertEquals("Precondition", 0, collection.Count);

			section1.Lines[0].TaxCode = "Tax1";
			section2.Lines[0].TaxCode = "Tax2";
			collection.PopulateFromSummarySections(new SummarySection[] { section1, section2 });
			AssertEquals("All section lines were added", 2, collection.Count);
			collection.ShowTaxCodes(true);
			AssertEquals("tax shown", "Tax1", collection[0].TaxCode);
			AssertEquals("tax shown", "Tax2", collection[1].TaxCode);
			AssertEquals("tax shown", "Tax Code", collection[0].Header.TaxCode);
			AssertEquals("tax shown", "Tax Code", collection[1].Header.TaxCode);
		}

		#region Implementation

		protected override SummaryLineCollection GetCollectionToTest()
		{
			return new SummaryLineCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SummaryLine(Factory);
		}

		#endregion
	}
}
