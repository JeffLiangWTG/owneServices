using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CusEntryHeaderLookupsTest : TestCaseWithFactory
	{
		public void TestCPDecQuestionViewTypeList()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			CusEntryHeaderLookups lookups = new CusEntryHeaderLookups(entryHeader);
			AssertNotNull("CPDecQuestionViewTypeList", lookups.CPDecQuestionViewTypeList);
		}

		public void TestCH_MessageTypeList()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			CusEntryHeaderLookups lookups = new CusEntryHeaderLookups(entryHeader);
			AssertEquals("CH_MessageTypeList", "DRW, EXP, EXW, IMP, MSC, REF, CAN", lookups.CH_MessageTypeList.CodesAsString);
			AssertEquals("CAN Description", "Customs Authority Number", lookups.CH_MessageTypeList.GetDescriptionFromCode(CusEntryNumberTypes.Australia.CAN));
		}
	}
}
