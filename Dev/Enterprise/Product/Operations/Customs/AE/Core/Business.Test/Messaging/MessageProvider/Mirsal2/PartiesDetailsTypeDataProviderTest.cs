using CargoWise.Customs.AE.MessageContracts.Mirsal2;
using Enterprise.Customs.AE.Business.MessageSending.Mirsal2.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(DeclarationRequestDataProvider))]
sealed class PartiesDetailsTypeDataProviderTest : Mirsal2PartiesDetailsTypeDataProviderAbstractClassBase
{
	public override void TestAssociatedOwnerCode()
	{
		Assert("to do in future WI", true);
	}

	public override void TestBrokerBusinessCode() => CombineAssertions(() =>
	{
		AssertEquals(string.Empty, CreateDataProvider().BrokerBusinessCode);
		var org = Factory.New<OrgHeader>();
		var code = org.CustomsCodes.AddNew();
		code.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
		code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedArabEmirates;
		code.OK_CustomsRegNo = "123";
		Declaration.JE_OA_DeclarantAddress_ZAddress.OrgPK = org.PK;
		AssertEquals(code.OK_CustomsRegNo, CreateDataProvider().BrokerBusinessCode);
	});

	public override void TestConsigneeImporterTransfereeCode() => CombineAssertions(() =>
	{
		AssertEquals(string.Empty, CreateDataProvider().ConsigneeImporterTransfereeCode);
		var importer = Factory.New<OrgHeader>();
		var code = importer.CustomsCodes.AddNew();
		code.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
		code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedArabEmirates;
		code.OK_CustomsRegNo = "123";
		Declaration.JE_OH_Importer = importer.PK;
		AssertEquals(code.OK_CustomsRegNo, CreateDataProvider().ConsigneeImporterTransfereeCode);
	});

	public override void TestConsignorExporterTransferorCode() => CombineAssertions(() =>
	{
		AssertEquals(string.Empty, CreateDataProvider().ConsignorExporterTransferorCode);
		var exporter = Factory.New<OrgHeader>();
		var code = exporter.CustomsCodes.AddNew();
		code.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
		code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedArabEmirates;
		code.OK_CustomsRegNo = "123";
		Declaration.JE_OH_Supplier = exporter.PK;
		AssertEquals(code.OK_CustomsRegNo, CreateDataProvider().ConsignorExporterTransferorCode);
	});

	public override void TestCTOCargoHandlerPremisesCode() => CombineAssertions(() =>
	{
		AssertEquals(string.Empty, CreateDataProvider().CTOCargoHandlerPremisesCode);
		var cto = Factory.New<OrgHeader>();
		var code = cto.CustomsCodes.AddNew();
		code.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
		code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedArabEmirates;
		code.OK_CustomsRegNo = "123";
		Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = cto.MainAddress.PK;
		AssertEquals(string.Empty, CreateDataProvider().CTOCargoHandlerPremisesCode);
		Declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
		AssertEquals(string.Empty, CreateDataProvider().CTOCargoHandlerPremisesCode);
		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		AssertEquals(code.OK_CustomsRegNo, CreateDataProvider().CTOCargoHandlerPremisesCode);
	});

	public override void TestMRAAEOIndicator()
	{
		Assert("to do in future WI", true);
	}

	public override void TestNotifyPartyAddress() => CombineAssertions(() =>
	{
		AssertNull(CreateDataProvider().NotifyPartyAddress);
		var notify = Factory.New<OrgHeader>();
		var orgAddress = notify.MainAddress;
		orgAddress.Address1 = "Address1";
		Declaration.JE_OH_NotifyParty = notify.PK;
		AssertEquals("Address1", CreateDataProvider().NotifyPartyAddress);
	});

	public override void TestNotifyPartyCode()
	{
		Assert("to do in future WI", true);
	}

	public override void TestNotifyPartyName() => CombineAssertions(() =>
	{
		AssertNull(CreateDataProvider().NotifyPartyName);
		var notify = Factory.New<OrgHeader>();
		notify.OH_FullName = "notify";
		Declaration.JE_OH_NotifyParty = notify.PK;
		AssertEquals("notify", CreateDataProvider().NotifyPartyName);
	});

	public override void TestShippingAirlineAgentBusinessCode() => CombineAssertions(() =>
	{
		AssertEquals(string.Empty, CreateDataProvider().ShippingAirlineAgentBusinessCode);
		var shipline = Factory.New<OrgHeader>();
		var code = shipline.CustomsCodes.AddNew();
		code.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
		code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedArabEmirates;
		code.OK_CustomsRegNo = "123";
		Declaration.JE_OH_ShippingLine = shipline.PK;

		AssertEquals(string.Empty, CreateDataProvider().ShippingAirlineAgentBusinessCode);
		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		AssertEquals("123", CreateDataProvider().ShippingAirlineAgentBusinessCode);

		Declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
		AssertEquals(string.Empty, CreateDataProvider().ShippingAirlineAgentBusinessCode);
	});

	protected override PartiesDetailsTypeDataProviderAbstractClass CreateDataProvider()
	{
		return DeclarationRequestDataProvider.CreateProvider(header, additionalDataProvider).Declaration.PartiesDetails;
	}

	JobDeclaration Declaration => declaration ??= GetJobDeclaration();
	JobDeclaration declaration;

	JobDeclaration GetJobDeclaration()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		jobDeclaration.ActiveEntryHeaders.Add(header);
		return jobDeclaration;
	}

	protected override void TearDown()
	{
		base.TearDown();
		Declaration.JE_TransportMode = null;
	}
}
