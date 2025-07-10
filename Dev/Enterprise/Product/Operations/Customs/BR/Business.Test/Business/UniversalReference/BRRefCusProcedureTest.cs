using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class BRRefCusProcedureTest : TestCaseWithFactory
	{
		public void TestGetRefCusProcedureListExport()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("AU", "099", "90", ZString.Empty, ZString.Empty, ZString.Empty, Common.BR.BRJobMessageTypeList.Codes.Export);
			helper.CreateRefCusProcedure("BR", "099", "80", ZString.Empty, ZString.Empty, ZString.Empty, Common.BR.BRJobMessageTypeList.Codes.Export);
			helper.CreateRefCusProcedure("BR", "099", "81", ZString.Empty, ZString.Empty, ZString.Empty, Common.BR.BRJobMessageTypeList.Codes.Export);
			helper.CreateRefCusProcedure("BR", "099", "82", ZString.Empty, ZString.Empty, ZString.Empty, Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex);
			helper.CreateRefCusProcedure("BR", "099", "83", ZString.Empty, ZString.Empty, ZString.Empty, Common.BR.BRJobMessageTypeList.Codes.ImportLicense);
			helper.CreateRefCusProcedure("BR", "099", "84", ZString.Empty, ZString.Empty, ZString.Empty, Common.BR.BRJobMessageTypeList.Codes.LPCO);
			helper.CreateRefCusProcedure("BR", "099", "85", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			Factory.Save();

			AssertEquals("Getting List for ISW", 0, BRRefCusProcedure.GetRefCusProcedureList(Factory, Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex).Count);
			AssertEquals("Getting List for LIC", 0, BRRefCusProcedure.GetRefCusProcedureList(Factory, Common.BR.BRJobMessageTypeList.Codes.ImportLicense).Count);
			AssertEquals("Getting List for LPC", 0, BRRefCusProcedure.GetRefCusProcedureList(Factory, Common.BR.BRJobMessageTypeList.Codes.LPCO).Count);

			var exportProcedures = BRRefCusProcedure.GetRefCusProcedureList(Factory, Common.BR.BRJobMessageTypeList.Codes.Export);
			AssertContainsExactElementsInExactOrder("Getting List for EXP", new string[] { "80", "81" }, exportProcedures.GetAllCodes());

			AssertSame("RefCusProcedureList cached", exportProcedures, BRRefCusProcedure.GetRefCusProcedureList(Factory, Common.BR.BRJobMessageTypeList.Codes.Export));
		}

		public void TestGetRefCusProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Brazil, Constants.ProcedureCategories.Duty, MessageSubTypeList.Codes._01, "1", ZString.Empty, ZString.Empty, Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Brazil, Constants.ProcedureCategories.PisCofins, MessageSubTypeList.Codes._01, "1", ZString.Empty, ZString.Empty, Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex);
			Factory.Save();

			var refCusProcedure = BRRefCusProcedure.GetRefCusProcedure(Factory, Constants.ProcedureCategories.Duty, Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex, MessageSubTypeList.Codes._01, "1", ZDateTime.Today);
			AssertNotNull("RefCusProcedure should not be null", refCusProcedure);
			AssertEquals("Category should be", "DTY", refCusProcedure.ZZ6_Category);
			AssertEquals("PreviousProcedureCode should be", "1", refCusProcedure.ZZ6_PreviousProcedureCode);
			AssertEquals("DataGrouping should be", "BR", refCusProcedure.ZZ6_ZZZ_NKDataGrouping);

			refCusProcedure = BRRefCusProcedure.GetRefCusProcedure(Factory, Constants.ProcedureCategories.PisCofins, Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex, MessageSubTypeList.Codes._01, "1", ZDateTime.Today);
			AssertNotNull("RefCusProcedure should not be null", refCusProcedure);
			AssertEquals("Category should be", "PIS", refCusProcedure.ZZ6_Category);
			AssertEquals("PreviousProcedureCode should be", "1", refCusProcedure.ZZ6_PreviousProcedureCode);
			AssertEquals("DataGrouping should be", "BR", refCusProcedure.ZZ6_ZZZ_NKDataGrouping);

			Factory.Save();
		}

		public void TestGetRefCusProcedureList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Brazil, Constants.ProcedureCategories.Duty, MessageSubTypeList.Codes._01, "1", ZString.Empty, ZString.Empty, Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Brazil, Constants.ProcedureCategories.Duty, MessageSubTypeList.Codes._01, "2", ZString.Empty, ZString.Empty, Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex);
			Factory.Save();

			var refCusProcedureList = BRRefCusProcedure.GetRefCusProcedureList(Factory, Constants.ProcedureCategories.Duty, Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex, MessageSubTypeList.Codes._01, ZDateTime.Today);

			AssertEquals("Count should be", 2, refCusProcedureList.Count);
			AssertSame("RefCusProcedureList cached", refCusProcedureList, BRRefCusProcedure.GetRefCusProcedureList(Factory, Constants.ProcedureCategories.Duty, Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex, MessageSubTypeList.Codes._01, ZDateTime.Today));
		}
	}
}
