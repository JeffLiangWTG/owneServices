using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Test.Declaration
{
	class GBAddInfoCusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAmendmentReasonCodeList()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var amendmentReasonCdpl = entryHeader.AddInfoLookups.AmendmentReasonCodeList;
			AssertSame(amendmentReasonCdpl, entryHeader.AddInfoLookups.AmendmentReasonCodeList);
			AssertEquals("20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 1, 2, 3, 16", amendmentReasonCdpl.CodesAsString);
		}
	}
}

