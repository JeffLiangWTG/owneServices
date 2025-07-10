using System;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class IE141TransitOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<IE141TransitOperationProvider>
	{
		protected override IE141TransitOperationProvider GetProvider() => new IE141TransitOperationProvider(nctsHeader);

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("NctsHeader missing", () => new IE141TransitOperationProvider(null));
		}

		public void TestMRN()
		{
			const string mrn = "IE231234567898765";
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = mrn;
			var provider = GetProvider();
			AssertEquals("MRN", mrn, provider.MRN);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		}
		NctsHeader nctsHeader;
	}
}
