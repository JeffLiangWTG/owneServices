using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SADDeclarantTraderWrapperTest : TestCaseWithFactory
{
	public void TestRepresentativeType()
	{
		jobDeclaration.JE_DeclarantType = "SEL";
		AssertEquals("1", declarantTraderWrapper.RepresentativeType);
		jobDeclaration.JE_DeclarantType = "DIR";
		AssertEquals("2", declarantTraderWrapper.RepresentativeType);
		jobDeclaration.JE_DeclarantType = "IND";
		AssertEquals("3", declarantTraderWrapper.RepresentativeType);
		jobDeclaration.JE_DeclarantType = ZString.Empty;
		AssertEquals(ZString.Empty, declarantTraderWrapper.RepresentativeType);
	}

	public void TestName()
	{
		orgAddress.CompanyName = "CAD MOLLICA SRL";
		AssertEquals("CAD MOLLICA SRL", declarantTraderWrapper.Name);

		orgAddress.CompanyName = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		AssertEquals("ABCDEFGHIJKLMNOPQRSTUVWXYZ012345678", declarantTraderWrapper.Name);

		jobDeclaration.JE_DeclarantType = "SEL";
		AssertEquals("", declarantTraderWrapper.Name);
	}

	public void TestNullDeclarant()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_DeclarantType = ZString.Empty;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		var emptyDeclarantTraderWrapper = new SADDeclarantTraderWrapper(declaration);

		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, emptyDeclarantTraderWrapper.RepresentativeType);
			AssertEquals(ZString.Empty, emptyDeclarantTraderWrapper.IdCountryCode);
			AssertEquals(ZString.Empty, emptyDeclarantTraderWrapper.ID);
			AssertEquals(ZString.Empty, emptyDeclarantTraderWrapper.Name);
			AssertEquals(ZString.Empty, emptyDeclarantTraderWrapper.Address);
			AssertEquals(ZString.Empty, emptyDeclarantTraderWrapper.Postcode);
			AssertEquals(ZString.Empty, emptyDeclarantTraderWrapper.City);
			AssertEquals(ZString.Empty, emptyDeclarantTraderWrapper.CountryCode);
		});
	}

	public void TestFullObjectWhenRepresentativeTypeIsSEL()
	{
		orgAddress.OA_RN_NKCountryCode = "DE";
		orgHeader.OH_Category = "NAT";
		orgCusCode.OK_CodeType = "COD";
		orgCusCode.OK_RN_NKCodeCountry = "DE";
		orgCusCode.OK_CustomsRegNo = "385040449";
		orgAddress.CompanyName = "CAD MOLLICA SRL";
		orgAddress.Address1 = "VIALE";
		orgAddress.Address2 = "G.GALEAZZO 16";
		orgAddress.Postcode = "20100";
		orgAddress.City = "MILANO";
		orgAddress.OA_RN_NKCountryCode = "IT";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OA_DeclarantAddress = orgAddress.PK;
		declaration.JE_DeclarantType = "SEL";

		var wrapper = new SADDeclarantTraderWrapper(declaration);
		CombineAssertions("When Representative Type is SEL, properties must be empty", () =>
		{
			AssertEquals("1", wrapper.RepresentativeType);
			AssertEquals(ZString.Empty, wrapper.IdCountryCode);
			AssertEquals(ZString.Empty, wrapper.ID);
			AssertEquals(ZString.Empty, wrapper.Name);
			AssertEquals(ZString.Empty, wrapper.Address);
			AssertEquals(ZString.Empty, wrapper.Postcode);
			AssertEquals(ZString.Empty, wrapper.City);
			AssertEquals(ZString.Empty, wrapper.CountryCode);
		});
	}

	public void TestFullObjectWhenRepresentativeTypeIsNotSEL()
	{
		orgHeader.OH_Category = "NAT";
		orgCusCode.OK_CodeType = "COD";
		orgCusCode.OK_RN_NKCodeCountry = "IT";
		orgCusCode.OK_CustomsRegNo = "385040449";
		orgAddress.CompanyName = "CAD MOLLICA SRL";
		orgAddress.Address1 = "VIALE";
		orgAddress.Address2 = "G.GALEAZZO 16";
		orgAddress.Postcode = "20100";
		orgAddress.City = "MILANO";
		orgAddress.OA_RN_NKCountryCode = "IT";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OA_DeclarantAddress = orgAddress.PK;
		declaration.JE_DeclarantType = "DIR";

		var wrapper = new SADDeclarantTraderWrapper(declaration);
		CombineAssertions("When Representative Type is not SEL, properties should be populated", () =>
		{
			AssertEquals("2", wrapper.RepresentativeType);
			AssertEquals("IT", wrapper.IdCountryCode);
			AssertEquals("385040449", wrapper.ID);
			AssertEquals("CAD MOLLICA SRL", wrapper.Name);
			AssertEquals("VIALE G.GALEAZZO 16", wrapper.Address);
			AssertEquals("20100", wrapper.Postcode);
			AssertEquals("MILANO", wrapper.City);
			AssertEquals("IT", wrapper.CountryCode);
		});
	}

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new SADDeclarantTraderWrapper(null));
		AssertNoExceptionThrown(() => new SADDeclarantTraderWrapper(Factory.New<JobDeclaration>()));
	}

	protected override void SetUp()
	{
		base.SetUp();
		orgHeader = Factory.New<OrgHeader>();
		orgAddress = orgHeader.Addresses.AddNew();
		orgCusCode = orgHeader.CustomsCodes.AddNew();
		jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_OA_DeclarantAddress = orgAddress.PK;

		declarantTraderWrapper = new SADDeclarantTraderWrapper(jobDeclaration);
	}
	OrgHeader orgHeader;
	OrgAddress orgAddress;
	OrgCusCode orgCusCode;
	JobDeclaration jobDeclaration;
	SADDeclarantTraderWrapper declarantTraderWrapper;
}
