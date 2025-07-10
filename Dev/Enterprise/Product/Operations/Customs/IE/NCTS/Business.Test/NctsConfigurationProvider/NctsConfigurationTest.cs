using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsConfiguration))]
	sealed class NctsConfigurationTest : NctsConfigurationAbstractTest<NctsConfiguration, GoodsItemsConfiguration>
	{
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

		public override void TestUseAdditionalDeclarationType() => AssertEquals(true, configuration.UseAdditionalDeclarationType);

		public override void TestUsePresentationDateTime() => AssertEquals(true, configuration.UsePresentationDateTime);

		public override void TestFullLoadPortSupport()
		{
			AssertEquals(false, configuration.FullLoadPortSupport);
		}

		public override void TestMiscAdditionalInfosSupport()
		{
			AssertEquals(true, configuration.MiscAdditionalInfosSupport(header));
		}

		public override void TestMiscGuaranteesSupport()
		{
			AssertEquals(false, configuration.MiscGuaranteesSupport(header));
		}

		public override void TestMiscPreviousDocumentsSupport()
		{
			AssertEquals(true, configuration.MiscPreviousDocumentsSupport(header));
		}

		public override void TestMiscSupportingDocumentsSupport()
		{
			AssertEquals(true, configuration.MiscSupportingDocumentsSupport(header));
		}

		public override void TestMiscTabPageSupport()
		{
			CombineAssertions(() =>
			{
				AssertEquals("When nctsHeader is null, MiscTabPageSupport", false, configuration.MiscTabPageSupport(nctsHeader: null));
				AssertEquals("When nctsHeader is not null, MiscTabPageSupport", false, configuration.MiscTabPageSupport(header));
			});
		}

		public override void TestReceiveIE043UnloadingPermissionDetailsMessage()
		{
			AssertEquals(true, configuration.ReceiveIE043UnloadingPermissionDetailsMessage);
		}

		public override void TestUseUniversalFeeCalculation()
		{
			AssertEquals(false, configuration.UseUniversalFeeCalculation);
		}

		protected override Type GetGuaranteeConfigurationTypeForTest() => typeof(GuaranteeConfiguration);

		protected override Type GetLocationOfGoodsFromAuthorisationDefaulterConfigurationTypeForTest() => typeof(LocationOfGoodsFromAuthorisationDefaulterConfiguration);

		protected override Type GetMessageSendingConfigurationTypeForTest() => typeof(MessageSendingConfiguration);

		protected override Type GetMovementHeaderConfigurationForTest() => typeof(MovementHeaderConfiguration);

		protected override Type GetExpectedValidationRuleConfigurationTypeForTest() => typeof(ValidationRuleConfiguration);

		protected override Type GetCountryOfRoutingConfigurationTypeForTest() => typeof(CountryOfRoutingConfiguration);

		protected override Type GetNctsEuOfficeCodeConfigurationTypeForTest() => typeof(NctsIEOfficeCodeConfiguration);

		protected override Type GetBillConfigurationTypeForTest() => typeof(BillConfiguration);

		public void TestGetNewGoodsItemsConfiguration() => AssertType(typeof(GoodsItemsConfiguration), configuration.GoodsItemsConfiguration);

		public void TestValidationDecider_DeparturePhase5()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType(typeof(NctsHeaderDeparturePhase5ValidationDecider), configuration.GetValidationDecider(header));
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
		}

		NctsHeader header;
	}
}
