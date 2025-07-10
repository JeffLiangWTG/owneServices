using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

[TestedType(typeof(NctsConfiguration))]
sealed class NctsConfigurationTest : EU.NCTS.Business.Testing.NctsConfigurationAbstractTest<NctsConfiguration, GoodsItemsConfiguration>
{
	public override void TestUseAdditionalDeclarationType() => AssertEquals(true, configuration.UseAdditionalDeclarationType);

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
		AssertEquals(false, configuration.ReceiveIE043UnloadingPermissionDetailsMessage);
	}

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

	protected override Type GetGuaranteeConfigurationTypeForTest() => typeof(GuaranteeConfiguration);

	protected override Type GetExpectedValidationRuleConfigurationTypeForTest() => typeof(ValidationRuleConfiguration);

	protected override Type GetMovementHeaderConfigurationForTest() => typeof(MovementHeaderConfiguration);

	protected override Type GetCountryOfRoutingConfigurationTypeForTest() => typeof(CountryOfRoutingConfiguration);

	protected override Type GetNctsContainerConfigurationTypeForTest() => typeof(NctsContainerConfiguration);

	protected override Type GetNctsEuOfficeCodeConfigurationTypeForTest() => typeof(NctsESOfficeCodeConfiguration);

	public override void TestFullLoadPortSupport()
	{
		AssertEquals(false, configuration.FullLoadPortSupport);
	}

	public override void TestMiscTabPageSupport()
	{
		AssertEquals(false, configuration.MiscTabPageSupport(header));
	}

	protected override Type GetBillConfigurationTypeForTest() => typeof(BillConfiguration);

	protected override Type GetNctsPackageConfigurationTypeForTest() => typeof(NctsPackageConfiguration);

	protected override Type GetCommonPreviousDocumentConfigurationTypeForTest() => typeof(CommonPreviousDocumentConfiguration);

	public void TestValidationDecider_DeparturePhase5()
	{
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		AssertType(typeof(NctsHeaderDeparturePhase5ValidationDecider), configuration.GetValidationDecider(header));
	}

	public void TestValidationDecider_ArrivalPhase5()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		AssertType<NctsHeaderArrivalPhase5ValidationDecider>(configuration.GetValidationDecider(header));
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
	}
	NctsHeader header;
}
