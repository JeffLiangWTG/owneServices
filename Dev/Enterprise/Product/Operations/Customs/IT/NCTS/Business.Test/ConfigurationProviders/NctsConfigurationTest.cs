using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(GoodsItemsConfiguration))]
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
		AssertType<NctsConfiguration>("NctsConfiguration Type", EU.NCTS.Business.NctsConfiguration.GetConfiguration(Factory, Core.Constants.CountryCodes.Italy));
	}

	public override void TestDocDataPlugInSupport()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When nctsHeader is null, DocDataPlugInSupport", false, configuration.DocDataPlugInSupport(nctsHeader: null));
			AssertEquals("When nctsHeader is departure, DocDataPlugInSupport", true, configuration.DocDataPlugInSupport(header));
			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			AssertEquals("When nctsHeader is not departure, DocDataPlugInSupport", false, configuration.DocDataPlugInSupport(arrivalHeader));
		});
	}

	public override void TestFullLoadPortSupport()
	{
		AssertEquals(false, configuration.FullLoadPortSupport);
	}

	public override void TestMiscTabPageSupport()
	{
		CombineAssertions(() =>
		{
			var departureNctsHeader = Factory.New<NctsHeader>();
			departureNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			AssertEquals("When nctsHeader is departure", true, configuration.MiscTabPageSupport(departureNctsHeader));

			var arrivalNctsHeader = Factory.New<NctsHeader>();
			arrivalNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			AssertEquals("When nctsHeader is not departure", false, configuration.MiscTabPageSupport(arrivalNctsHeader));
		});
	}

	protected override Type GetExpectedValidationRuleConfigurationTypeForTest() => typeof(ValidationRuleConfiguration);

	protected override Type GetMovementHeaderConfigurationForTest() => typeof(MovementHeaderConfiguration);

	protected override Type GetMessageSendingConfigurationTypeForTest() => typeof(MessageSendingConfiguration);

	protected override Type GetGuaranteeConfigurationTypeForTest() => typeof(GuaranteeConfiguration);

	protected override Type GetCountryOfRoutingConfigurationTypeForTest() => typeof(CountryOfRoutingConfiguration);

	protected override Type GetNctsEuOfficeCodeConfigurationTypeForTest() => typeof(NctsItOfficeCodeConfiguration);

	protected override Type GetBillConfigurationTypeForTest() => typeof(BillConfiguration);

	protected override Type GetCusSealConfigurationTypeForTest() => typeof(CusSealConfiguration);

	protected override Type GetNctsContainerConfigurationTypeForTest() => typeof(NctsContainerConfiguration);

	protected override Type GetCommonPreviousDocumentConfigurationTypeForTest() => typeof(CommonPreviousDocumentConfiguration);

	protected override Type GetCusTransportMeansConfigurationForTest() => typeof(CusTransportMeansConfiguration);

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
