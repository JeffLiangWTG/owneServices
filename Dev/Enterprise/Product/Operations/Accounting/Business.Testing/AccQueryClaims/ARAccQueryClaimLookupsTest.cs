using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccQueryClaims.Testing
{
	public class ARAccQueryClaimLookupsTest : MasterFiles.Business.Testing.AccQueryClaimLookupsTest
	{
		public override AccQueryClaim CreateBusinessObjectForTest()
		{
			return (AccQueryClaim)Factory.New<Enterprise.Integration.Accounting.IARAccQueryClaim>();
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
				QueryClaimStatusCodeList.Codes.QCStatus6CancelledAndClosed
			};

			AssertContainsExactElementsInAnyOrder(expectedList, list.GetAllCodes());
		}
	}
}
