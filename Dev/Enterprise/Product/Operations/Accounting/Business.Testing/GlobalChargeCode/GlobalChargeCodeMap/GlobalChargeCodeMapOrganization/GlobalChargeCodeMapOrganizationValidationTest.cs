namespace Enterprise.Accounting.Business.GlobalChargeCode.Testing
{
	using System;
	using Enterprise.MasterFiles.Business;

	internal class GlobalChargeCodeMapOrganizationValidationTest : AccGlobalChargeCodeMapValidationTest
	{
		protected override Type globalChargeCodeType
		{
			get
			{
				return typeof(GlobalChargeCodeMapOrganization);
			}
		}

		public void TestCheckYG_OH()
		{
			GlobalChargeCodeMap globalChargeCode = (GlobalChargeCodeMap)Factory.New(globalChargeCodeType);
			globalChargeCode.Validation.ValidateAll();
			AssertHasError(globalChargeCode.YG_OHInfo, "Please enter an Organization.");
			globalChargeCode.YG_OH = Factory.New<OrgHeader>().PK;
			AssertNoError(globalChargeCode.YG_OHInfo, "Please enter an Organization.");
		}
	}
}