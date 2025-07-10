using System;
using Enterprise.Customs.FR.NCTS.Configurations;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(NctsConfiguration))]
	sealed class NctsConfigurationTest : EU.NCTS.Business.Testing.NctsConfigurationAbstractTest<NctsConfiguration, GoodsItemsConfiguration>
	{
		public override void TestUseAdditionalDeclarationType() => AssertEquals(true, configuration.UseAdditionalDeclarationType);

		public override void TestUsePresentationDateTime() => AssertEquals(true, configuration.UsePresentationDateTime);

		public override void TestMiscAdditionalInfosSupport()
		{
			AssertEquals("MiscAdditionalInfosSupport", true, configuration.MiscAdditionalInfosSupport(header));
		}

		public override void TestMiscSupportingDocumentsSupport()
		{
			AssertEquals("MiscSupportingDocumentsSupport", true, configuration.MiscSupportingDocumentsSupport(header));
		}

		public override void TestMiscPreviousDocumentsSupport()
		{
			AssertEquals("MiscPreviousDocumentsSupport", true, configuration.MiscPreviousDocumentsSupport(header));
		}

		public override void TestMiscGuaranteesSupport()
		{
			AssertEquals("MiscGuaranteesSupport", false, configuration.MiscGuaranteesSupport(header));
		}

		public override void TestUseUniversalFeeCalculation()
		{
			AssertEquals("UseUniversalFeeCalculation", false, configuration.UseUniversalFeeCalculation);
		}

		public override void TestReceiveIE043UnloadingPermissionDetailsMessage()
		{
			AssertEquals("ReceiveIE043UnloadingPermissionDetailsMessage", true, configuration.ReceiveIE043UnloadingPermissionDetailsMessage);
		}

		public void TestConfigurationIsCorrectlyLoaded()
		{
			AssertType<NctsConfiguration>("NctsConfiguration Type", EU.NCTS.Business.NctsConfiguration.GetConfiguration(Factory, Core.Constants.CountryCodes.France));
		}

		public override void TestDocDataPlugInSupport()
		{
			CombineAssertions(() =>
			{
				AssertEquals("When nctsHeader is null, DocDataPlugInSupport", false, configuration.DocDataPlugInSupport(nctsHeader: null));
				AssertEquals("When nctsHeader is departure, DocDataPlugInSupport", true, configuration.DocDataPlugInSupport(header));
				var arrivalHeader = Factory.New<NctsHeader>();
				arrivalHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
				AssertEquals("When nctsHeader is not departure, DocDataPlugInSupport", false, configuration.DocDataPlugInSupport(arrivalHeader));
			});
		}

		public override void TestFullLoadPortSupport()
		{
			AssertEquals(true, configuration.FullLoadPortSupport);
		}

		public override void TestMiscTabPageSupport()
		{
			AssertEquals(false, configuration.MiscTabPageSupport(header));
		}

		public override void TestUseDeclarantFallBack()
		{
			AssertEquals(true, configuration.UseDeclarantFallBack);
		}

		public override void TestUseBranchOrgProxyFallBack()
		{
			AssertEquals(true, configuration.UseBranchOrgProxyFallBack);
		}

		public override void TestUseCompanyOrgProxyFallBack()
		{
			AssertEquals(true, configuration.UseCompanyOrgProxyFallBack);
		}

		public override void TestUseGuaranteeGridValidation()
		{
			AssertEquals(true, configuration.UseGuaranteeGridValidation);
		}

		public override void TestClearExistingGuaranteesConfiguration()
		{
			AssertEquals(true, configuration.ClearExistingGuaranteesConfiguration);
		}

		protected override Type GetExpectedValidationRuleConfigurationTypeForTest() => typeof(ValidationRuleConfiguration);

		protected override Type GetNctsEuOfficeCodeConfigurationTypeForTest() => typeof(NctsFrOfficeCodeConfiguration);

		protected override Type GetMessageSendingConfigurationTypeForTest() => typeof(TP5MessageSendingConfiguration);

		protected override Type GetMovementHeaderConfigurationForTest() => typeof(MovementHeaderConfiguration);

		protected override Type GetBillConfigurationTypeForTest() => typeof(BillConfiguration);

		protected override Type GetGuaranteeConfigurationTypeForTest() => typeof(GuaranteeConfiguration);

		protected override Type GetNctsContainerConfigurationTypeForTest() => typeof(NctsContainerConfiguration);

		protected override Type GetNctsPackageConfigurationTypeForTest() => typeof(NctsPackageConfiguration);

		public void TestGetNewGoodsItemsConfiguration() => AssertType(typeof(GoodsItemsConfiguration), configuration.GoodsItemsConfiguration);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		}
		NctsHeader header;
	}
}
