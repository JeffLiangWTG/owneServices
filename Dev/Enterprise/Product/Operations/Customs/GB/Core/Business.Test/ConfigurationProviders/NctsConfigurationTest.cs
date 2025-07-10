using System;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(GoodsItemsConfiguration))]
	sealed class NctsConfigurationTest : EU.NCTS.Business.Testing.NctsConfigurationAbstractTest<NctsConfiguration, GoodsItemsConfiguration>
	{
		public override void TestUseAdditionalDeclarationType() => AssertEquals(true, configuration.UseAdditionalDeclarationType);

		public override void TestUsePresentationDateTime() => AssertEquals(false, configuration.UsePresentationDateTime);

		public override void TestDocDataPlugInSupport()
		{
			CombineAssertions(() =>
			{
				AssertEquals("When nctsHeader is null, DocDataPlugInSupport", false, configuration.DocDataPlugInSupport(nctsHeader: null));
				AssertEquals("When nctsHeader is departure, DocDataPlugInSupport", true, configuration.DocDataPlugInSupport(header));
				var arrivalHeader = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				AssertEquals("When nctsHeader is not departure, DocDataPlugInSupport", false, configuration.DocDataPlugInSupport(arrivalHeader));
			});
		}

		public override void TestFullLoadPortSupport() => AssertEquals(false, configuration.FullLoadPortSupport);

		public override void TestAllowMixedCaseAuthorisationNumbers() => AssertEquals(true, configuration.AllowMixedCaseAuthorisationNumbers);

		public override void TestMiscAdditionalInfosSupport() => AssertEquals(true, configuration.MiscAdditionalInfosSupport(header));

		public override void TestMiscGuaranteesSupport() => AssertEquals(false, configuration.MiscGuaranteesSupport(header));

		public override void TestMiscPreviousDocumentsSupport() => AssertEquals(true, configuration.MiscPreviousDocumentsSupport(header));

		public override void TestMiscSupportingDocumentsSupport() => AssertEquals(true, configuration.MiscSupportingDocumentsSupport(header));

		public override void TestMiscTabPageSupport() => AssertEquals(false, configuration.MiscTabPageSupport(header));

		public override void TestReceiveIE043UnloadingPermissionDetailsMessage() => AssertEquals(true, configuration.ReceiveIE043UnloadingPermissionDetailsMessage);

		public override void TestUseUniversalFeeCalculation() => AssertEquals(false, configuration.UseUniversalFeeCalculation);

		protected override Type GetMovementHeaderConfigurationForTest() => typeof(MovementHeaderConfiguration);

		protected override Type GetGuaranteeConfigurationTypeForTest() => typeof(GuaranteeConfiguration);

		protected override Type GetMessageSendingConfigurationTypeForTest() => typeof(MessageSendingConfiguration);

		protected override Type GetCountryOfRoutingConfigurationTypeForTest() => typeof(CountryOfRoutingConfiguration);

		protected override Type GetNctsEuOfficeCodeConfigurationTypeForTest() => typeof(NctsEuOfficeCodeConfiguration);

		public void TestConfigurationIsCorrectlyLoaded()
		{
			AssertType<NctsConfiguration>("NctsConfiguration Type", EU.NCTS.Business.NctsConfiguration.GetConfiguration(Factory, Core.Constants.CountryCodes.UnitedKingdom));
		}

		protected override Type GetExpectedValidationRuleConfigurationTypeForTest() => typeof(ValidationRuleConfiguration);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		}

		NctsHeader header;
	}
}
