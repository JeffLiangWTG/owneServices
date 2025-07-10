using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE014TransitOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<IE014TransitOperationProvider>
	{
		public void TestLRN()
		{
			movementHeader.BM_PaperlessInbondNum = "LRN2343234242";
			AssertEquals("LRN", "LRN2343234242", Provider.LRN);

			var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Ireland);
			newEntryNumber.CE_EntryNum = "MRN1233219876";
			AssertEquals("LRN", string.Empty, Provider.LRN);
		}

		public void TestMRN()
		{
			var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Ireland);
			newEntryNumber.CE_EntryNum = "MRN1233219876";
			AssertEquals("MRN", "MRN1233219876", Provider.MRN);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader = nctsHeader.MovementHeader;
		}

		protected override IE014TransitOperationProvider GetProvider() => new IE014TransitOperationProvider(nctsHeader);

		NctsHeader nctsHeader;
		NctsDepartureMovementHeader movementHeader;
	}
}
