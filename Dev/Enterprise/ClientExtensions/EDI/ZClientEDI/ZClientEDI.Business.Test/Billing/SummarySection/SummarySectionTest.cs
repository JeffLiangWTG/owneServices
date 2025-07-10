using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(SummarySection))]
	internal class SummarySectionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaults()
		{
			SummarySection section = new SummarySection(Factory);
			AssertNotNull(section.Header);
			AssertNotNull(section.Lines);
			AssertEquals(0, section.NumberOfColumnsUsed);
		}

		public void TestNumberOfColumnsUsed()
		{
			SummarySection section = new SummarySection(Factory);
			section.Header.Column1 = "south park";
			AssertEquals(1, section.NumberOfColumnsUsed);

			section.Header.Column2 = "lost";
			AssertEquals(2, section.NumberOfColumnsUsed);

			section.Header.Column6 = "transformer";
			AssertEquals("Counting only consecutive columns from the first one", 2, section.NumberOfColumnsUsed);

			section.Header.Column7 = "superman";
			AssertEquals("Counting only consecutive columns from the first one", 2, section.NumberOfColumnsUsed);

			section.Header.Column5 = "true blood";
			AssertEquals("Counting only consecutive columns from the first one", 2, section.NumberOfColumnsUsed);

			section.Header.Column4 = "simpsons";
			AssertEquals("Counting only consecutive columns from the first one", 2, section.NumberOfColumnsUsed);

			section.Header.Column3 = "simpsons";
			AssertEquals(7, section.NumberOfColumnsUsed);

			section.Header.Column8 = "Column8";
			AssertEquals(8, section.NumberOfColumnsUsed);

			section.Header.Column9 = "Column9";
			AssertEquals(9, section.NumberOfColumnsUsed);

			section.Header.Column10 = "Column10";
			AssertEquals(10, section.NumberOfColumnsUsed);

			section.Header.Column5 = "";
			AssertEquals(4, section.NumberOfColumnsUsed);

			section.Header.Column1 = "";
			AssertEquals("Counting only consecutive columns from the first one", 0, section.NumberOfColumnsUsed);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SummarySection(Factory);
		}

		#endregion
	}
}
