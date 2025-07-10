using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

[TestedType(typeof(NctsConfiguration))]
sealed class NctsConfigurationTest : EU.NCTS.Business.Testing.NctsConfigurationAbstractTest<NctsConfiguration, GoodsItemsConfiguration>
{
	public override void TestUseAdditionalDeclarationType() => AssertEquals(true, configuration.UseAdditionalDeclarationType);

	public override void TestUsePresentationDateTime() => AssertEquals(false, configuration.UsePresentationDateTime);

	public override void TestMiscAdditionalInfosSupport() => AssertEquals(true, configuration.MiscAdditionalInfosSupport(header));

	public override void TestMiscSupportingDocumentsSupport() => AssertEquals(true, configuration.MiscSupportingDocumentsSupport(header));

	public override void TestMiscPreviousDocumentsSupport() => AssertEquals(true, configuration.MiscPreviousDocumentsSupport(header));

	public override void TestMiscGuaranteesSupport() => AssertEquals(false, configuration.MiscGuaranteesSupport(header));

	public override void TestUseUniversalFeeCalculation() => AssertEquals(false, configuration.UseUniversalFeeCalculation);

	public override void TestReceiveIE043UnloadingPermissionDetailsMessage() => AssertEquals(true, configuration.ReceiveIE043UnloadingPermissionDetailsMessage);

	public override void TestDocDataPlugInSupport()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When nctsHeader is null, DocDataPlugInSupport", false, configuration.DocDataPlugInSupport(nctsHeader: null));
			AssertEquals("When nctsHeader is departure, DocDataPlugInSupport", false, configuration.DocDataPlugInSupport(header));
			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			AssertEquals("When nctsHeader is not departure, DocDataPlugInSupport", false, configuration.DocDataPlugInSupport(arrivalHeader));
			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);
			AssertEquals("When nctsHeader is departure, DocDataPlugInSupport", true, configuration.DocDataPlugInSupport(departureHeader));
		});
	}

	public override void TestFullLoadPortSupport() => AssertEquals(false, configuration.FullLoadPortSupport);

	public override void TestMiscTabPageSupport()
	{
		AssertEquals(false, configuration.MiscTabPageSupport(header));
	}
	protected override Type GetMessageSendingConfigurationTypeForTest() => typeof(MessageSendingConfiguration);

	protected override Type GetExpectedValidationRuleConfigurationTypeForTest() => typeof(ValidationRuleConfiguration);

	protected override Type GetMovementHeaderConfigurationForTest() => typeof(MovementHeaderConfiguration);

	protected override Type GetCountryOfRoutingConfigurationTypeForTest() => typeof(CountryOfRoutingConfiguration);

	protected override Type GetNctsEuOfficeCodeConfigurationTypeForTest() => typeof(NctsEuOfficeCodeConfiguration);

	protected override Type GetBillConfigurationTypeForTest() => typeof(BillConfiguration);

	protected override Type GetGuaranteeConfigurationTypeForTest() => typeof(GuaranteeConfiguration);

	protected override Type GetLocationOfGoodsFromAuthorisationDefaulterConfigurationTypeForTest() => typeof(LocationOfGoodsFromAuthorisationDefaulterConfiguration);

	protected override Type GetNctsPackageConfigurationTypeForTest() => typeof(NctsPackageConfiguration);

	public void TestValidationDecider_DeparturePhase5()
	{
		header.SetMovementType(NctsMovementType.Codes.Departure);
		AssertType(typeof(NctsHeaderDeparturePhase5ValidationDecider), configuration.GetValidationDecider(header));
	}

	protected override ZBool ExpectedUseLocalReferenceNumberIgnoreInDatabaseCheck => true;

	public void TestGetNewGoodsItemsConfiguration() => AssertType(typeof(GoodsItemsConfiguration), configuration.GoodsItemsConfiguration);

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
	}
	NctsHeader header;
}
