namespace Enterprise.Accounting.Business.GlobalChargeCode.Testing
{
	using System;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.MasterFiles.Business;

	internal abstract class AccGlobalChargeCodeMapValidationTest : BusinessObjectValidationTestCase
	{
		protected abstract Type globalChargeCodeType
		{
			get;
		}

		public void TestCheckYG_Code()
		{
			GlobalChargeCodeMap globalChargeCode = (GlobalChargeCodeMap)Factory.New(globalChargeCodeType);
			globalChargeCode.Validation.ValidateAll();
			AssertHasError(globalChargeCode.YG_CodeInfo, "Please enter a Global Code.");
			globalChargeCode.YG_Code = "TEST";
			AssertNoError(globalChargeCode.YG_CodeInfo, "Please enter a Global Code.");
			Factory.Save();
			globalChargeCode = (GlobalChargeCodeMap)Factory.New(globalChargeCodeType);
			globalChargeCode.YG_Code = "TEST1";
			AssertNoError(globalChargeCode.YG_CodeInfo, "Code must be unique.");
			globalChargeCode.YG_Code = "TEST";
			AssertHasError(globalChargeCode.YG_CodeInfo, "Code must be unique.");
			globalChargeCode.YG_OH = Factory.New<OrgHeader>().PK;
			globalChargeCode.Validation.ValidateAll();
			AssertNoError(globalChargeCode.YG_CodeInfo, "Code must be unique.");
		}

		public void TestCheckYG_Desc()
		{
			GlobalChargeCodeMap globalChargeCode = (GlobalChargeCodeMap)Factory.New(globalChargeCodeType);
			globalChargeCode.Validation.ValidateAll();
			AssertHasError(globalChargeCode.YG_DescInfo, "Please enter a Description.");
			globalChargeCode.YG_Desc = "Test Description";
			AssertNoError(globalChargeCode.YG_DescInfo, "Please enter a Description.");
		}
	}
}