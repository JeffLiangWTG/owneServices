using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	[TestedType(typeof(DocRateEntryCollection))]
	sealed class DocRateEntryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocRateEntryCollection>
	{
		public void TestSort()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			ClientRate rate = Helper.NewClientRate(Helper.NewOrgHeader());

			RateEntry[] entries =
			{
				rate.AddRateEntry("AIR", "LSE", "USLAX", "GBLON"),
				rate.AddRateEntry("AIR", "LSE", "AUSYD", "INBOM"),
				rate.AddRateEntry("AIR", "LSE", "AUBNE", "USLAX"),
				rate.AddRateEntry("SDE", "FCL", "USSFO", "AUMEL"),
				rate.AddRateEntry("AIR", "LSE", "USSFO", "AUMEL")
			};

			DocRateEntryCollection collection = new DocRateEntryCollection(null, entries, Factory);

			StringBuilder builder = new StringBuilder();
			builder.AppendLine();

			foreach (DocRateEntry wrapper in collection)
			{
				builder.AppendLine(wrapper.Origin);
			}

			const string expected = @"
San Francisco
Sydney
Brisbane
Los Angeles
";

			AssertMultilineASCIIEquals("", expected, builder.ToString());
		}

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			RateEntry rateEntry = Factory.New<RateEntry>();
			return DocRateEntry.New(rateEntry, Factory);
		}

		protected override DocRateEntryCollection GetCollectionToTest()
		{
			return new DocRateEntryCollection(Factory);
		}

		TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}

		TestHelper helper;

		#endregion
	}
}
