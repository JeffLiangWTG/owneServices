using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsConfiguration))]
sealed class NctsConfigurationTest : EU.NCTS.Business.Testing.NctsConfigurationAbstractTest<NctsConfiguration, GoodsItemsConfiguration>
{
	public override void TestUseAdditionalDeclarationType() => AssertEquals(false, configuration.UseAdditionalDeclarationType);

	public override void TestUsePresentationDateTime() => AssertEquals(false, configuration.UsePresentationDateTime);

	public override void TestMiscAdditionalInfosSupport()
	{
		AssertEquals(true, configuration.MiscAdditionalInfosSupport(header));
	}

	public override void TestMiscSupportingDocumentsSupport()
	{
		AssertEquals(true, configuration.MiscSupportingDocumentsSupport(header));
	}

	public override void TestMiscPreviousDocumentsSupport()
	{
		AssertEquals(true, configuration.MiscPreviousDocumentsSupport(header));
	}

	public override void TestMiscGuaranteesSupport()
	{
		AssertEquals(false, configuration.MiscGuaranteesSupport(header));
	}

	public override void TestUseUniversalFeeCalculation()
	{
		AssertEquals(false, configuration.UseUniversalFeeCalculation);
	}

	public override void TestReceiveIE043UnloadingPermissionDetailsMessage()
	{
		AssertEquals(true, configuration.ReceiveIE043UnloadingPermissionDetailsMessage);
	}

	public override void TestDocDataPlugInSupport()
	{
		AssertEquals(true, configuration.DocDataPlugInSupport(header));
	}

	public override void TestFullLoadPortSupport()
	{
		AssertEquals(false, configuration.FullLoadPortSupport);
	}

	public override void TestMiscTabPageSupport()
	{
		AssertEquals(false, configuration.MiscTabPageSupport(header));
	}

	public void TestHeaderDepartureValidationDeciderType()
	{
		AssertType<NctsHeaderDeparturePhase5ValidationDecider>(configuration.GetValidationDecider(header));
	}

	protected override Type GetExpectedValidationRuleConfigurationTypeForTest() => typeof(ValidationRuleConfiguration);

	protected override Type GetMovementHeaderConfigurationForTest() => typeof(MovementHeaderConfiguration);

	protected override Type GetCountryOfRoutingConfigurationTypeForTest() => typeof(CountryOfRoutingConfiguration);

	protected override Type GetNctsEuOfficeCodeConfigurationTypeForTest() => typeof(NctsEuOfficeCodeConfiguration);

	protected override Type GetBillConfigurationTypeForTest() => typeof(BillConfiguration);

	protected override Type GetGuaranteeConfigurationTypeForTest() => typeof(GuaranteeConfiguration);

	protected override Type GetNctsPackageConfigurationTypeForTest() => typeof(NctsPackageConfiguration);

	protected override Type GetNctsContainerConfigurationTypeForTest() => typeof(NctsContainerConfiguration);

	protected override Type GetCusSealConfigurationTypeForTest() => typeof(CusSealConfiguration);

	protected override Type GetCusSupplyChainActorReferenceConfigurationTypeForTest() => typeof(CusSupplyChainActorReferenceConfiguration);

	protected override Type GetCommonPreviousDocumentConfigurationTypeForTest() => typeof(CommonPreviousDocumentConfiguration);

	protected override Type GetMessageSendingConfigurationTypeForTest() => typeof(MessageSendingConfiguration);

	public void TestValidationDecider_DeparturePhase5()
	{
		AssertType(typeof(NctsHeaderDeparturePhase5ValidationDecider), configuration.GetValidationDecider(header));
	}

	protected override ZBool ExpectedUseLocalReferenceNumberIgnoreInDatabaseCheck => true;

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
	}
	NctsHeader header;
}
