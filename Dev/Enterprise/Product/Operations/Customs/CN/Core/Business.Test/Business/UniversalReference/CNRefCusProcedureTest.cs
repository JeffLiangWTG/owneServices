using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CNRefCusProcedureTest : TestCaseWithFactory
	{
		[TestDate(2018, 8, 7)]
		public void TestGetRefCusProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure("US", "", "0110", "", "", "US Customs Procedure 0110", "IMP,EXP");
			procedure1.ZZ6_StartDate = new ZDateTime(2018, 1, 1);
			procedure1.ZZ6_EndDate = new ZDateTime(2018, 12, 1);
			var procedure2 = helper.CreateRefCusProcedure("CN", "", "0130", "", "", "CN Customs Procedure 0130", "IMP,EXP");
			procedure2.ZZ6_StartDate = new ZDateTime(2018, 1, 1);
			procedure2.ZZ6_EndDate = new ZDateTime(2018, 12, 1);
			var procedure3 = helper.CreateRefCusProcedure("CN", "", "0110", "", "", "CN Customs Procedure 0110", "IMP,EXP");
			procedure3.ZZ6_StartDate = new ZDateTime(2018, 1, 1);
			procedure3.ZZ6_EndDate = new ZDateTime(2018, 8, 1);
			var procedure4 = helper.CreateRefCusProcedure("CN", "N", "0110", "", "", "CN Customs Procedure 20180801", "IMP,EXP");
			procedure4.ZZ6_StartDate = new ZDateTime(2018, 8, 1);
			procedure4.ZZ6_EndDate = new ZDateTime(2018, 12, 1);
			Factory.Save();
			var testItem = CNRefCusProcedure.GetRefCusProcedure(Factory, "0110", ZDateTime.Today);
			AssertNotNull(testItem);
			AssertEquals("0110", testItem.ZZ6_ProcedureCode);
			AssertEquals("CN Customs Procedure 20180801", testItem.ZZ6_Description);
		}

		public void TestEntryInstructionCPCList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("CN", "A", "10", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			helper.CreateRefCusProcedure("CN", "A", "11", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			helper.CreateRefCusProcedure("US", "H", "60", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ZA"))
			{
				AssertEquals("No Procedure for ZA", 0, CNRefCusProcedure.GetRefCusProcedureList(Factory).Count);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CN"))
			{
				AssertEquals("Getting List for CN", 2, CNRefCusProcedure.GetRefCusProcedureList(Factory).Count);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				AssertEquals("Getting List for US", 1, CNRefCusProcedure.GetRefCusProcedureList(Factory).Count);
			}

			new UniversalReferenceTestDataHelper(Factory).CreateRefCusProcedure("CN", "B", "80", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CN"))
			{
				AssertEquals("Getting List for CN", 2, CNRefCusProcedure.GetRefCusProcedureList(Factory).Count);
				Factory.ClearCachedValue<CodeDescriptionPairList>("CN_RefCusProcedures_ProcedureCode");
				AssertEquals("Clear Cache and Get New List for CN", 3, CNRefCusProcedure.GetRefCusProcedureList(Factory).Count);
			}
		}
	}
}
