using System;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class RepresentativeWrapperTest : TestCaseWithFactory
{
	public void TestNewOrNull()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => RepresentativeWrapper.NewOrNull(null));

		declaration.JE_DeclarantType = "";
		declaration.JE_OA_Representative = ZGuid.Empty;
		AssertNull(nameof(IRepresentative), RepresentativeWrapper.NewOrNull(declaration));
	}

	public void TestRepresentativeType()
	{
		declaration.JE_DeclarantType = ZString.Empty;
		var representative = GetNewRepresentative();
		AssertNull("When JE_DeclarantType is empty, " + nameof(IRepresentative.RepresentativeType), representative.RepresentativeType);

		declaration.JE_DeclarantType = "ABC";
		representative = GetNewRepresentative();
		AssertNull("When JE_DeclarantType is unknown, " + nameof(IRepresentative.RepresentativeType), representative.RepresentativeType);

		declaration.JE_DeclarantType = "SEL";
		representative = GetNewRepresentative();
		AssertNull("When JE_DeclarantType is 'SEL', " + nameof(IRepresentative.RepresentativeType), representative.RepresentativeType);

		declaration.JE_DeclarantType = "DIR";
		representative = GetNewRepresentative();
		AssertEquals("When JE_DeclarantType is 'DIR', " + nameof(IRepresentative.RepresentativeType), 2, representative.RepresentativeType);

		declaration.JE_DeclarantType = "IND";
		representative = GetNewRepresentative();
		AssertEquals("When JE_DeclarantType is 'IND', " + nameof(IRepresentative.RepresentativeType), 3, representative.RepresentativeType);
	}

	public void TestAddress()
	{
		var representative = GetNewRepresentative();
		AssertType<AddressWrapper>(nameof(IRepresentative.Address), representative.Address);
	}

	public void TestIdentificationNumber()
	{
		var representative = GetNewRepresentative();
		AssertNull($"When Declarant Type is empty, {nameof(IRepresentative.IdentificationNumber)}", representative.IdentificationNumber);

		declaration.JE_DeclarantType = "DIR";
		var codCusCode = representativeOrgHeader.CustomsCodes.AddNew();
		codCusCode.OK_CodeType = "EOR";
		codCusCode.OK_RN_NKCodeCountry = "IT";
		codCusCode.OK_CustomsRegNo = "123456789";
		representative = GetNewRepresentative();
		AssertEquals($"When Declarant Type is DIR, {nameof(IRepresentative.IdentificationNumber)}", "IT123456789", representative.IdentificationNumber);

		declaration.JE_DeclarantType = "IND";
		AssertEquals($"When Declarant Type is IND, {nameof(IRepresentative.IdentificationNumber)}", "IT123456789", representative.IdentificationNumber);

		declaration.JE_DeclarantType = "SEL";
		representative = GetNewRepresentative();
		AssertNull($"When Declarant Type is SEL, {nameof(IRepresentative.IdentificationNumber)}", representative.IdentificationNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();

		representativeOrgHeader = Factory.New<OrgHeader>();
		representativeMainAddress = representativeOrgHeader.MainAddress;

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_OA_Representative = representativeMainAddress.PK;
	}

	JobDeclaration declaration;
	OrgHeader representativeOrgHeader;
	OrgAddress representativeMainAddress;

	IRepresentative GetNewRepresentative() => RepresentativeWrapper.NewOrNull(declaration);
}
