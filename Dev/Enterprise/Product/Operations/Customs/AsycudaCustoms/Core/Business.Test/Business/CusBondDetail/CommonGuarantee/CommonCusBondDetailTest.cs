using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class CommonCusBondDetailTest : TestCaseWithFactory
	{
		public void TestPW_BondAmount_DecimalPlacesAttribute()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CommonCusBondDetail), nameof(CommonCusBondDetail.PW_BondAmount), false, attribute => attribute.DecimalPlaces == 2);
		}

		public void TestPW_BondNumber2_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CommonCusBondDetail), nameof(CommonCusBondDetail.PW_BondNumber2), false, attribute => attribute.Caption == "Reference Number");
		}

		public void TestPW_BondEffectiveDate_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CommonCusBondDetail), nameof(CommonCusBondDetail.PW_BondEffectiveDate), false, attribute => attribute.Caption == "Issue Date");
		}

		public void TestPW_BondAmount_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CommonCusBondDetail), nameof(CommonCusBondDetail.PW_BondAmount), false, attribute => attribute.Caption == "Amount");
		}

		public void TestSingleBusinessObjectAroundARow()
		{
			AssertEquals(1, typeof(CommonCusBondDetail).GetCustomAttributes(typeof(SingleObjectAroundARow), false).Length);
		}

		public void TestTypeDecider()
		{
			AssertType<CusBondDetailTypeDecider>(CommonCusBondDetail.TypeDecider);
		}

		public void TestCusGuarantee_AfterDeletion()
		{
			var guarantee = Factory.New<CommonCusBondDetailForTest>();
			CombineAssertions(() =>
			{
				AssertNull("Default", guarantee.CusGuarantee);

				var cusGuarantee = new GuaranteeTestHelper(Factory).CusGuarantee;
				guarantee.PW_CPH_Guarantee = cusGuarantee.PK;
				AssertEquals("Not null", cusGuarantee, guarantee.CusGuarantee);

				guarantee.CusGuarantee.Delete();
				AssertNull("Deleted", guarantee.CusGuarantee);
			});
		}

		public void TestCusGuarantee_AfterModification()
		{
			var guarantee = Factory.New<CommonCusBondDetailForTest>();
			CombineAssertions(() =>
			{
				var cusGuarantee1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				guarantee.PW_CPH_Guarantee = cusGuarantee1.PK;
				AssertEquals("Not null", cusGuarantee1, guarantee.CusGuarantee);

				var cusGuarantee2 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				guarantee.PW_CPH_Guarantee = cusGuarantee2.PK;
				AssertEquals("change value", cusGuarantee2, guarantee.CusGuarantee);
			});
		}

		public void TestIsLinked()
		{
			var guarantee = Factory.New<CommonCusBondDetailForTest>();
			CombineAssertions(() =>
			{
				guarantee.PW_Status = GuaranteeStatusList.Codes.Linked;
				AssertEquals("LIN", true, guarantee.IsLinked);
				guarantee.PW_Status = GuaranteeStatusList.Codes.NotLinked;
				AssertEquals("UNL", false, guarantee.IsLinked);
			});
		}

		public void TestPW_Status_ReadOnly()
		{
			AssertEquals(true, Factory.New<CommonCusBondDetailForTest>().PW_StatusInfo.ReadOnly);
		}

		public void TestPW_Status_DefaultValue()
		{
			AssertEquals(GuaranteeStatusList.Codes.NotLinked, Factory.New<CommonCusBondDetailForTest>().PW_Status);
		}

		public void TestInstruction()
		{
			var guarantee = Factory.New<CommonCusBondDetailForTest>();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			guarantee.Parent = entryInstruction;
			AssertSame(entryInstruction, guarantee.Instruction);
		}

		public void TestInstruction_Cached()
		{
			var guarantee = Factory.New<CommonCusBondDetailForTest>();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			guarantee.Parent = entryInstruction;
			AssertSame(guarantee.Instruction, guarantee.Instruction);
		}

		public void TestInstruction_Recalculated()
		{
			var guarantee = Factory.New<CommonCusBondDetailForTest>();
			var entryInstruction1 = Factory.New<CusEntryInstruction>();
			guarantee.Parent = entryInstruction1;
			var entryInstruction2 = Factory.New<CusEntryInstruction>();
			guarantee.Parent = entryInstruction2;
			AssertSame(entryInstruction2, guarantee.Instruction);
		}

		public void TestLookups()
		{
			AssertType<CommonCusBondDetailLookups>(Factory.New<CommonCusBondDetailForTest>().Lookups);
		}

		public void TestValidation()
		{
			AssertType<CommonCusBondDetailValidation>(Factory.New<CommonCusBondDetailForTest>().Validation);
		}

		public void TestCanDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			Factory.Save();
			var guarantee = Factory.New<CommonCusBondDetailForTest>();
			guarantee.Parent = instruction;
			using (var mutex = new ZGlobalMutex(MutexIDs.GuaranteeManagement, guarantee.Instruction.PK.ToString()))
			{
				mutex.Lock();
				AssertEquals(false, guarantee.CanDelete);
				mutex.Unlock();
			}
		}

		public void TestReasonForNotAbleToDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			Factory.Save();
			var guarantee = Factory.New<CommonCusBondDetailForTest>();
			guarantee.Parent = instruction;
			using (var mutex = new ZGlobalMutex(MutexIDs.GuaranteeManagement, guarantee.Instruction.PK.ToString()))
			{
				mutex.Lock();
				AssertContains("cannot be deleted. The Guarantee is currently being edited by", guarantee.ReasonForNotAbleToDelete);
				mutex.Unlock();
			}
		}
	}

	class CommonCusBondDetailForTest : CommonCusBondDetail
	{
		public CommonCusBondDetailForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
