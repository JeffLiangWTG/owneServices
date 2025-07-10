using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	sealed class EntryComparerTest : TestCaseWithFactory
	{
		public void TestEntryComparer()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			EntryComparer comparer = new EntryComparer();
			var entry1 = Factory.New<RateEntry>();
			var entry2 = Factory.New<RateEntry>();

			AssertEquals(0, comparer.Compare(entry1, entry2));
			entry2.TI_OriginLRC = "USLAX";
			AssertEquals(-1, comparer.Compare(entry1, entry2));
			entry1.TI_OriginLRC = "USLAX";
			entry2.TI_OriginLRC = "";
			AssertEquals(1, comparer.Compare(entry1, entry2));
			entry1.TI_OriginLRC = "AU";
			entry1.TI_DestinationLRC = "US";
			entry2.TI_OriginLRC = "AU";
			entry2.TI_DestinationLRC = "GB";
			AssertEquals(1, comparer.Compare(entry1, entry2));
			entry1.TI_OriginLRC = "US";
			entry1.TI_DestinationLRC = "GB";
			entry2.TI_OriginLRC = "CN";
			entry2.TI_DestinationLRC = "IN";
			AssertEquals(1, comparer.Compare(entry1, entry2));
			entry1.TI_OriginLRC = "GB";
			entry1.TI_DestinationLRC = "AU";
			entry2.TI_OriginLRC = "IN";
			entry2.TI_DestinationLRC = "AU";
			AssertEquals(1, comparer.Compare(entry1, entry2));
			entry1.TI_OriginLRC = "AUBNE";
			entry1.TI_DestinationLRC = "USLAX";
			entry2.TI_OriginLRC = "AUMEL";
			entry2.TI_DestinationLRC = "GBLON";
			AssertEquals(1, comparer.Compare(entry1, entry2));
			entry1.TI_OriginLRC = "USLAX";
			entry1.TI_DestinationLRC = "AUSYD";
			entry2.TI_OriginLRC = "USSFO";
			entry2.TI_DestinationLRC = "AUMEL";
			AssertEquals(-1, comparer.Compare(entry1, entry2));
			entry1.TI_OriginLRC = "US";
			entry1.TI_DestinationLRC = "GBLON";
			entry2.TI_OriginLRC = "US";
			entry2.TI_DestinationLRC = "SGSIN";
			AssertEquals(1, comparer.Compare(entry1, entry2));
		}
	}
}
