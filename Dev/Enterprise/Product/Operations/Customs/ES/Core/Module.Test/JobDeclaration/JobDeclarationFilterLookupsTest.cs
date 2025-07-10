using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Module.Testing
{
	class JobDeclarationFilterLookupsTest : TestCaseWithFactory
	{
		public void TestParallelListCodeList()
		{
			var lookups = new JobDeclarationFilterLookups(new JobDeclarationFilterBusinessObject());
			CombineAssertions(() =>
			{
				var list = lookups.ParallelList;
				AssertEquals(typeof(YesNoList), list.GetType());
				AssertEquals("N, Y", list.CodesAsString);
				AssertEquals("Yes", list.GetDescriptionFromCode("Y"));
				AssertEquals("ParallelList returns the same object", true, ReferenceEquals(list, lookups.ParallelList));
			});
		}

		public void TestEntrySubTypes()
		{
			var lookups = new JobDeclarationFilterLookups(new JobDeclarationFilterBusinessObject());
			var entrySubTypes = lookups.EntrySubTypes;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "A, B, C, EXS, T2C, T2L, X, Y, Z", entrySubTypes.CodesAsString);
				AssertSame("Cached.", entrySubTypes, lookups.EntrySubTypes);
			});
		}

		public void TestCustomsOfficePurposeList()
		{
			var lookups = new JobDeclarationFilterLookups(new JobDeclarationFilterBusinessObject());
			var list = lookups.CustomsOfficePurposeList;
			CombineAssertions(() =>
			{
				AssertEquals("List is correct for ES", "ENT, EXT, EXP", list.CodesAsString);
				AssertSame("Cached.", list, lookups.CustomsOfficePurposeList);
			});
		}

		public void TestDeclarationTypeList()
		{
			var filterBizObj = new JobDeclarationFilterBusinessObject();
			var entryInstruction = Factory.GetNull<Business.Declaration.CusEntryInstruction>();
			AssertEquals(entryInstruction.Lookups.StyleList.GetType(), filterBizObj.Lookups.DeclarationTypeList.GetType());
		}
	}
}
