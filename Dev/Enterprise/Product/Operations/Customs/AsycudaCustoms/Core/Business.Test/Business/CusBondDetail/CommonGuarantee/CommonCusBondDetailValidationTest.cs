using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class CommonCusBondDetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPW_BondType_ListValidation()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(Factory.New<CusBondDetail>().PW_BondTypeInfo, "~", GuaranteeBondTypeList.Codes.Continuous);
		}

		public void TestCheckPW_BondAmount_CheckNotNegative()
		{
			var guarantee = Factory.New<CusBondDetail>();
			CombineAssertions(() =>
			{
				guarantee.PW_ActivityCode = GuaranteeActivityCodeList.Codes.ConsumesGuarantee;
				ValidationTestHelper.AssertErrorIfValueIsNegative(guarantee.PW_BondAmountInfo);

				guarantee.PW_ActivityCode = GuaranteeActivityCodeList.Codes.ReleasesGuarantee;
				ValidationTestHelper.AssertErrorIfValueIsNegative(guarantee.PW_BondAmountInfo);
			});
		}

		public void TestCheckPW_ReferenceNumber2_Unique_NotInDB()
		{
			const string message = "Reference Number in combination with linked guarantee must unique in the job.";
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidNotLinkedGuarantee();
			var releaseGuarantee = guarantee.Instruction.ReleaseGuarantees.AddNew();
			CombineAssertions(() =>
			{
				releaseGuarantee.PW_BondNumber2 = guarantee.PW_BondNumber2;
				AssertHasError("Not unique", releaseGuarantee.PW_BondNumber2Info, message);
				releaseGuarantee.PW_BondNumber2 = "XXX";
				AssertNoError("Unique", releaseGuarantee.PW_BondNumber2Info, message);
			});
		}

		public void TestCheckPW_ReferenceNumber2_Unique_InDB()
		{
			const string message = "Reference Number in combination with linked guarantee must unique in the job.";
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			var releaseGuarantee = guarantee.Instruction.ReleaseGuarantees.AddNew();
			CombineAssertions(() =>
			{
				releaseGuarantee.PW_BondNumber2 = guarantee.PW_BondNumber2;
				AssertHasError("Not unique", releaseGuarantee.PW_BondNumber2Info, message);
				releaseGuarantee.PW_BondNumber2 = "XXX";
				AssertNoError("Unique", releaseGuarantee.PW_BondNumber2Info, message);
			});
			releaseGuarantee.Instruction.UnlockGuaranteeManagementMutex();
		}

		public void TestCheckPW_ReferenceNumber2_Unique_DifferentInstructions_ConsumingGuarantee()
		{
			const string message = "Reference Number in combination with linked guarantee must unique in the job.";
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			var instruction2 = guarantee.Instruction.JobDeclaration.CustomsEntryInstructions.AddNew();
			var instruction2Guarantee = instruction2.Guarantee;
			instruction2Guarantee.PW_CPH_Guarantee = guarantee.PW_CPH_Guarantee;
			CombineAssertions(() =>
			{
				instruction2Guarantee.PW_BondNumber2 = "XXX";
				AssertNoError("Unique", instruction2Guarantee.PW_BondNumber2Info, message);
				instruction2Guarantee.PW_BondNumber2 = guarantee.PW_BondNumber2;
				AssertHasError("Not unique", instruction2Guarantee.PW_BondNumber2Info, message);
				Factory.Save();
				instruction2Guarantee.Validation.ValidatePW_BondNumber2();
				AssertNoError("No validation if already saved", instruction2Guarantee.PW_BondNumber2Info, message);
			});
		}

		public void TestCheckPW_ReferenceNumber2_Unique_DifferentInstructions_ReleaseGuarantee()
		{
			const string message = "Reference Number in combination with linked guarantee must unique in the job.";
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			var instruction2 = guarantee.Instruction.JobDeclaration.CustomsEntryInstructions.AddNew();
			var instruction2ReleaseGuarantee = instruction2.ReleaseGuarantees.AddNew();
			instruction2ReleaseGuarantee.PW_CPH_Guarantee = guarantee.PW_CPH_Guarantee;
			CombineAssertions(() =>
			{
				instruction2ReleaseGuarantee.PW_BondNumber2 = "XXX";
				AssertNoError("Unique", instruction2ReleaseGuarantee.PW_BondNumber2Info, message);
				instruction2ReleaseGuarantee.PW_BondNumber2 = guarantee.PW_BondNumber2;
				AssertHasError("Not unique", instruction2ReleaseGuarantee.PW_BondNumber2Info, message);
				Factory.Save();
				instruction2ReleaseGuarantee.Validation.ValidatePW_BondNumber2();
				AssertNoError("No validation if already saved", instruction2ReleaseGuarantee.PW_BondNumber2Info, message);
			});
		}

		public void TestCheckPW_ReferenceNumber2_Unique_DifferentInstances()
		{
			Factory.RefreshEnabled = false;
			const string message = "Reference Number in combination with linked guarantee must unique in the job.";
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var declarationInAnotherFactory = anotherFactory.Load<JobDeclaration>(guarantee.Instruction.JobDeclaration.PK);
			var instructionInAnotherFactory = declarationInAnotherFactory.CustomsEntryInstructions.AddNew();
			var instructionGuaranteeInAnotherFactory = instructionInAnotherFactory.Guarantee;
			instructionGuaranteeInAnotherFactory.PW_CPH_Guarantee = guarantee.PW_CPH_Guarantee;
			instructionGuaranteeInAnotherFactory.PW_BondNumber2 = "XXX";
			anotherFactory.Save();
			CombineAssertions(() =>
			{
				guarantee.PW_BondNumber2 = "XXX";
				AssertHasError("Not unique", guarantee.PW_BondNumber2Info, message);
				instructionGuaranteeInAnotherFactory.PW_BondNumber2 = "XYZ";
				anotherFactory.Save();
				guarantee.Validation.ValidatePW_BondNumber2();
				AssertNoError("Unique", guarantee.PW_BondNumber2Info, message);
			});
			guarantee.Instruction.UnlockGuaranteeManagementMutex();
		}

		public void TestCheckPW_ActivityCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(Factory.New<CusBondDetail>().PW_ActivityCodeInfo, "~", GuaranteeActivityCodeList.Codes.ConsumesGuarantee);
		}

		public void TestCheckPW_Status()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(Factory.New<CusBondDetail>().PW_StatusInfo, "~", GuaranteeStatusList.Codes.Linked);
		}
	}
}
