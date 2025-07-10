using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccQueryClaims.Testing
{
	public class APAccQueryClaimLookupsTest : MasterFiles.Business.Testing.AccQueryClaimLookupsTest
	{
		public override AccQueryClaim CreateBusinessObjectForTest()
		{
			return (AccQueryClaim)Factory.New<Enterprise.Integration.Accounting.IAPAccQueryClaim>();
		}

		public override void TestClaimStatusLookups()
		{
			var list = QueryClaim.Lookups.ClaimStatus;
			var expectedList = new string[]
			{
				QueryClaimStatusCodeList.Codes.QCStatus1Open,
				QueryClaimStatusCodeList.Codes.QCStatus2Working,
				QueryClaimStatusCodeList.Codes.QCStatus3AcceptedAndCreditNoteIssuedAndClosed,
				QueryClaimStatusCodeList.Codes.QCStatus4RejectedNotClosed,
				QueryClaimStatusCodeList.Codes.QCStatus5RejectedClosed,
				QueryClaimStatusCodeList.Codes.QCStatus6CancelledAndClosed,
				QueryClaimStatusCodeList.Codes.QCStatus7RejectedWithDCRCCRNotClosed,
				QueryClaimStatusCodeList.Codes.QCStatus8AcceptedNotClosed,
				QueryClaimStatusCodeList.Codes.QCStatus9AcceptedClosed
			};

			AssertContainsExactElementsInAnyOrder(expectedList, list.GetAllCodes());
		}
	}
}
