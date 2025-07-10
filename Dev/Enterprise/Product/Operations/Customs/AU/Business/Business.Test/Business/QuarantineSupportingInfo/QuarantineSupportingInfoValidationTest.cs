using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class QuarantineSupportingInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDoNotAllowDuplicatedDescriptions()
		{
			string errMessage = "This Code already exists in this Job";
			quarantineSupportingInfo1.CSI_Description = "D11";
			quarantineSupportingInfo2.CSI_Description = "D11";
			quarantineSupportingInfo1.Validation.ValidateCSI_Description();
			quarantineSupportingInfo2.Validation.ValidateCSI_Description();
			AssertHasError(quarantineSupportingInfo1.CSI_DescriptionInfo, errMessage);
			AssertHasError(quarantineSupportingInfo2.CSI_DescriptionInfo, errMessage);
			quarantineSupportingInfo2.CSI_Description = "D22";
			quarantineSupportingInfo1.Validation.ValidateCSI_Description();
			quarantineSupportingInfo2.Validation.ValidateCSI_Description();
			AssertNoError(quarantineSupportingInfo1.CSI_DescriptionInfo, errMessage);
			AssertNoError(quarantineSupportingInfo2.CSI_DescriptionInfo, errMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			var eXDOCHeader = invoiceHeader.QuarantineExDocHeader;
			quarantineSupportingInfo1 = eXDOCHeader.SupportingInfos.AddNew();
			quarantineSupportingInfo2 = eXDOCHeader.SupportingInfos.AddNew();
		}

		QuarantineSupportingInfo quarantineSupportingInfo1;
		QuarantineSupportingInfo quarantineSupportingInfo2;
	}
}
