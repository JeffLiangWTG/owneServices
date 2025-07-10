using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing;

[TestedType(typeof(CusGoodsLocation))]
public class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
{
	public void TestAddressType()
	{
		AssertType<CusGoodsLocationAddress>(Factory.New<CusGoodsLocation>().Address);
	}

	public void TestLookups()
	{
		AssertType<CusGoodsLocationLookups>(Factory.New<CusGoodsLocation>().Lookups);
	}

	public void TestValidation()
	{
		AssertType<CusGoodsLocationValidation>(Factory.New<CusGoodsLocation>().Validation);
	}

	public void TestIsImportAndH2()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var location = (CusGoodsLocation)instruction.GoodsLocation;

		CombineAssertions(() =>
		{
			AssertEquals("False when not IMP and not H2", false, location.IsImportAndH2);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("False when IMP and not H2", false, location.IsImportAndH2);

			instruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			AssertEquals("True when IMP and H2", true, location.IsImportAndH2);
		});
	}

	public void TestIsImportAndH2_ParentIsTempStorageHeader()
	{
		var tempStorageHeader = Factory.New<TemporaryStorageHeader>();
		var cusGoodsLocation = (CusGoodsLocation)tempStorageHeader.GoodsLocation;

		AssertEquals("IsImportAndH2 is false when the parent is not an EntryInstruction", false, cusGoodsLocation.IsImportAndH2);
	}

	public void TestNamePhoneAndEmailVisible_ParentIsTemporaryStorageHeader()
	{
		var tempStorageHeader = Factory.New<TemporaryStorageHeader>();
		var cusGoodsLocation = (CusGoodsLocation)tempStorageHeader.GoodsLocation;

		CombineAssertions("NamePhoneAndEmailVisible is true in all cases when Parent is TemporaryStorageHeader", () =>
		{
			AssertEquals(true, cusGoodsLocation.NamePhoneAndEmailVisible);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			cusGoodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			AssertEquals(" Qualifier Y, Type B", true, cusGoodsLocation.NamePhoneAndEmailVisible);

			cusGoodsLocation.CGL_Qualifier = "X";
			AssertEquals("Qualifier not Y, Type B", true, cusGoodsLocation.NamePhoneAndEmailVisible);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			cusGoodsLocation.CGL_Type = "X";
			AssertEquals("Qualifier Y, Type not B", true, cusGoodsLocation.NamePhoneAndEmailVisible);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			cusGoodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;

			AssertEquals("Qualifier Z, Type A C or D", true, cusGoodsLocation.NamePhoneAndEmailVisible);

			cusGoodsLocation.CGL_Qualifier = "X";
			AssertEquals("Qualifier not Z, Type A C or D", true, cusGoodsLocation.NamePhoneAndEmailVisible);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			cusGoodsLocation.CGL_Type = "X";
			AssertEquals("Qualifier Y, Type not A C or D", true, cusGoodsLocation.NamePhoneAndEmailVisible);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			AssertEquals("Qualifier V", true, cusGoodsLocation.NamePhoneAndEmailVisible);

			cusGoodsLocation.CGL_Qualifier = "X";
			AssertEquals("Qualifier not V", true, cusGoodsLocation.NamePhoneAndEmailVisible);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			cusGoodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			cusGoodsLocation.Address.AuthorisationNumber = "ESAH3";
			AssertEquals("Qualifier Y, Type B, Authorisation starts with ES", true, cusGoodsLocation.NamePhoneAndEmailVisible);

			cusGoodsLocation.CGL_Qualifier = "X";
			AssertEquals("Qualifier not Y, Type B, Authorisation starts with ES", true, cusGoodsLocation.NamePhoneAndEmailVisible);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			cusGoodsLocation.CGL_Type = "X";
			AssertEquals("Qualifier Y, Type not B, Authorisation starts with ES", true, cusGoodsLocation.NamePhoneAndEmailVisible);

			cusGoodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			cusGoodsLocation.Address.AuthorisationNumber = "AH3";
			AssertEquals("Qualifier Y, Type B, Authorisation not starts with ES", true, cusGoodsLocation.NamePhoneAndEmailVisible);
		});
	}

	public void TestIsQualifierYAndTypeB()
	{
		var declaration = Factory.New<JobDeclaration>();
		var location = (CusGoodsLocation)declaration.CustomsEntryInstructions.AddNew().GoodsLocation;

		CombineAssertions(() =>
		{
			AssertEquals("False when Qualifier not Y and Type not B", false, location.IsQualifierYAndTypeB);

			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			AssertEquals("False when Qualifier Y and Type not B", false, location.IsQualifierYAndTypeB);

			location.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			AssertEquals("True when Qualifier Y and Type B", true, location.IsQualifierYAndTypeB);
		});
	}

	public void TestNamePhoneAndEmailVisible()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var location = (CusGoodsLocation)instruction.GoodsLocation;

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			instruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			location.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			AssertEquals("IMP, H2, Qualifier Y, Type B", false, location.NamePhoneAndEmailVisible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("not IMP, H2, Qualifier Y, Type B", true, location.NamePhoneAndEmailVisible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			instruction.CEI_Style = "X";
			AssertEquals("IMP, not H2, Qualifier Y, Type B", true, location.NamePhoneAndEmailVisible);

			instruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			location.CGL_Qualifier = "X";
			AssertEquals("IMP, H2, Qualifier not Y, Type B", true, location.NamePhoneAndEmailVisible);

			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			location.CGL_Type = "X";
			AssertEquals("IMP, H2, Qualifier Y, Type not B", true, location.NamePhoneAndEmailVisible);

			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			location.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
			AssertEquals("IMP, H2, Qualifier Z, Type A C or D", false, location.NamePhoneAndEmailVisible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("not IMP, H2, Qualifier Z, Type A C or D", true, location.NamePhoneAndEmailVisible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			instruction.CEI_Style = "X";
			AssertEquals("IMP, not H2, Qualifier Z, Type A C or D", true, location.NamePhoneAndEmailVisible);

			instruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			location.CGL_Qualifier = "X";
			AssertEquals("IMP, H2, Qualifier not Z, Type A C or D", true, location.NamePhoneAndEmailVisible);

			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			location.CGL_Type = "X";
			AssertEquals("IMP, H2, Qualifier Z, Type not A C or D", true, location.NamePhoneAndEmailVisible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			AssertEquals("EXP, Qualifier V", false, location.NamePhoneAndEmailVisible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("not EXP, Qualifier V", true, location.NamePhoneAndEmailVisible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			location.CGL_Qualifier = "X";
			AssertEquals("EXP, Qualifier not V", true, location.NamePhoneAndEmailVisible);

			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			location.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			location.Address.AuthorisationNumber = "ESAH3";
			AssertEquals("EXP, Qualifier Y, Type B, Authorisation starts with ES", false, location.NamePhoneAndEmailVisible);

			instruction.CEI_Style = "X";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("not EXP, Qualifier Y, Type B, Authorisation starts with ES", true, location.NamePhoneAndEmailVisible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			location.CGL_Qualifier = "X";
			AssertEquals("EXP, Qualifier not Y, Type B, Authorisation starts with ES", true, location.NamePhoneAndEmailVisible);

			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			location.CGL_Type = "X";
			AssertEquals("EXP, Qualifier Y, Type not B, Authorisation starts with ES", true, location.NamePhoneAndEmailVisible);

			location.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			location.Address.AuthorisationNumber = "AH3";
			AssertEquals("EXP, Qualifier Y, Type B, Authorisation not starts with ES", true, location.NamePhoneAndEmailVisible);
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var location = declaration.CustomsEntryInstructions.AddNew().GoodsLocation;
		location.CGL_LocationUse = "DEP";
		return location;
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
}
