using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CustomsRegistry))]
sealed class CustomsRegistryTest : RegistryBusinessObjectTemplateTestCase<CustomsRegistry>
{
	public void TestValidateBranch()
	{
		var org1 = Factory.New<OrgHeader>();
		org1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, ZString.Empty, Core.Constants.CountryCodes.Belgium);
		var coll = new CustomsRegistryCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
		var mapping = coll.AddNew();
		mapping.Organization = ZGuid.Empty;

		CombineAssertions(() =>
		{
			mapping.Organization = ZGuid.Invalid;
			AssertHasErrorContaining("Invalid code error expected", mapping.OrganizationInfo, ListValidation.InvalidCodeError);
			mapping.Organization = org1.PK;
			AssertNoErrorContaining("No invalid code error expected", mapping.OrganizationInfo, ListValidation.InvalidCodeError);
		});
	}

	public void TestValidateDeclarationType()
	{
		var coll = new CustomsRegistryCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
		var mapping = coll.AddNew();

		CombineAssertions(() =>
		{
			mapping.DeclarationType = "XX";
			AssertHasErrorContaining("Invalid code error expected", mapping.DeclarationTypeInfo, ListValidation.InvalidCodeError);
			mapping.DeclarationType = BERegistryDeclarationTypeList.Codes.ProbablyNoControlRequired;
			AssertNoErrorContaining("No invalid code error expected", mapping.DeclarationTypeInfo, ListValidation.InvalidCodeError);
		});
	}

	public void TestValidateStartingNo()
	{
		var coll = new CustomsRegistryCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
		var mapping = coll.AddNew();

		CombineAssertions(() =>
		{
			mapping.StartingNo = -2;
			AssertHasErrorContaining("Negative number error expected", mapping.StartingNoInfo, MandatoryValidation.ValueCannotBeNegative);
			mapping.StartingNo = 1;
			AssertNoErrorContaining("No negative number error expected", mapping.StartingNoInfo, MandatoryValidation.ValueCannotBeNegative);
		});
	}

	public void TestValidateStartingNo_DA()
	{
		var coll = new CustomsRegistryCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
		var mapping = coll.AddNew();
		mapping.DeclarationType = "DA";

		CombineAssertions(() =>
		{
			mapping.StartingNo = -2;
			AssertHasErrorContaining("Negative number error expected", mapping.StartingNoInfo, MandatoryValidation.ValueCannotBeNegative);
			mapping.StartingNo = 0;
			AssertHasErrorContaining("Zero number error expected", mapping.StartingNoInfo, MandatoryValidation.ValueCannotBeZero);
			mapping.StartingNo = 1;
			AssertNoErrorContaining("No negative number error expected", mapping.StartingNoInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertNoErrorContaining("No zero number error expected", mapping.StartingNoInfo, MandatoryValidation.ValueCannotBeZero);
		});
	}

	public void TestCurrentNoReadOnly()
	{
		Assert(new CustomsRegistry().CurrentNoInfo.ReadOnly);
	}

	public void TestBranchReadOnly()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Organization is not readonly", false, new CustomsRegistry { CurrentNo = 0 }.OrganizationInfo.ReadOnly);
			var org1 = Factory.New<OrgHeader>();
			Env.NumberFountains.BECustomsRegistryNumberFountain(GlbBranch.CurrentBranch.PK.ToGuid(), org1.PK.ToGuid(), BERegistryDeclarationTypeList.Codes.TransitArrival, ZDateTime.Today, 1).GetNextFormatted(Factory);
			AssertEquals("Organization is readonly", true, new CustomsRegistry { Organization = org1.PK.ToGuid(), StartingNo = 1, DeclarationType = "TA", StartingDate = ZDateTime.Today }.OrganizationInfo.ReadOnly);
		});
	}

	public void TestDeclarationTypeReadOnly()
	{
		CombineAssertions(() =>
		{
			AssertEquals("DeclarationType is not readonly", false, new CustomsRegistry { CurrentNo = 0 }.DeclarationTypeInfo.ReadOnly);
			var org1 = Factory.New<OrgHeader>();
			Env.NumberFountains.BECustomsRegistryNumberFountain(GlbBranch.CurrentBranch.PK.ToGuid(), org1.PK.ToGuid(), BERegistryDeclarationTypeList.Codes.TransitArrival, ZDateTime.Today, 1).GetNextFormatted(Factory);
			AssertEquals("DeclarationType is readonly", true, new CustomsRegistry { Organization = org1.PK.ToGuid(), StartingNo = 1, DeclarationType = "TA", StartingDate = ZDateTime.Today }.DeclarationTypeInfo.ReadOnly);
		});
	}

	public void TestStartingNoReadOnly()
	{
		CombineAssertions(() =>
		{
			AssertEquals("StartingNo is not readonly", false, new CustomsRegistry { CurrentNo = 0 }.StartingNoInfo.ReadOnly);
			var org1 = Factory.New<OrgHeader>();
			Env.NumberFountains.BECustomsRegistryNumberFountain(GlbBranch.CurrentBranch.PK.ToGuid(), org1.PK.ToGuid(), BERegistryDeclarationTypeList.Codes.TransitArrival, ZDateTime.Today, 1).GetNextFormatted(Factory);
			AssertEquals("StartingNo is readonly", true, new CustomsRegistry { Organization = org1.PK.ToGuid(), StartingNo = 1, DeclarationType = "TA", StartingDate = ZDateTime.Today }.StartingNoInfo.ReadOnly);
		});
	}

	public void TestStartingDateReadOnly()
	{
		CombineAssertions(() =>
		{
			AssertEquals("StartingDate is not readonly", false, new CustomsRegistry { CurrentNo = 0 }.StartingDateInfo.ReadOnly);
			var org1 = Factory.New<OrgHeader>();
			Env.NumberFountains.BECustomsRegistryNumberFountain(GlbBranch.CurrentBranch.PK.ToGuid(), org1.PK.ToGuid(), BERegistryDeclarationTypeList.Codes.TransitArrival, ZDateTime.Today, 1).GetNextFormatted(Factory);
			AssertEquals("StartingDate is readonly", true, new CustomsRegistry { Organization = org1.PK.ToGuid(), StartingNo = 1, DeclarationType = "TA", StartingDate = ZDateTime.Today }.StartingDateInfo.ReadOnly);
		});
	}

	public void TestDeclarationType()
	{
		var customsRegistry = new CustomsRegistry();
		var codeList = customsRegistry.DeclarationTypes;

		CombineAssertions(() =>
		{
			AssertType<BERegistryDeclarationTypeList>(codeList);
			AssertEquals("CodesAsString", codeList.CodesAsString, "H1, H2, H3, H4, H5, H6, H7, B1, B2, B3, B4, TD, TA, DA, OTH");
			AssertSame("Cached", codeList, customsRegistry.DeclarationTypes);
		});
	}

	public void TestValidateTypeAndDate()
	{
		var message = "There already exists a registry with declaration type DA and a starting date later then the date filled. Cancel the registration or fill a starting date that is later.";
		var org = Factory.New<OrgHeader>();
		Env.NumberFountains.BECustomsRegistryNumberFountain(GlbBranch.CurrentBranch.PK.ToGuid(), org.PK.ToGuid(), BERegistryDeclarationTypeList.Codes.AllTransitDepartureDeclarations, new ZDateTime(DateTime.Now), 1).GetNextFormatted(Factory);

		var coll = new CustomsRegistryCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
		var mapping1 = coll.AddNew();
		mapping1.Organization = org.PK;
		mapping1.DeclarationType = BERegistryDeclarationTypeList.Codes.AllTransitDepartureDeclarations;
		mapping1.StartingNo = 1;
		mapping1.StartingDate = new ZDateTime(DateTime.Now);

		var mapping2 = coll.AddNew();
		mapping2.Organization = org.PK;
		mapping2.DeclarationType = BERegistryDeclarationTypeList.Codes.AllTransitDepartureDeclarations;
		mapping2.StartingNo = 1;
		mapping2.StartingDate = mapping1.StartingDate.AddDays(2);
		var info1 = mapping1.DeclarationTypeInfo;
		var info2 = mapping2.DeclarationTypeInfo;

		CombineAssertions(() =>
		{
			mapping1.ValidateDeclarationType();
			mapping2.ValidateDeclarationType();
			AssertNoErrorContaining("Previous has earlier date. No error expected item1", info1, message);
			AssertNoErrorContaining("Previous has earlier date. No error expected item2", info2, message);
			mapping2.StartingDate = mapping1.StartingDate.AddDays(-2);
			mapping1.ValidateDeclarationType();
			mapping2.ValidateDeclarationType();
			AssertNoErrorContaining("Previous has later date. No error expected item1", info1, message);
			AssertHasErrorContaining("Previous has later date. Error expected item2", info2, message);
		});
	}

	protected override bool RequiresFactory => true;

	protected override bool RequiresFallbackLevel => true;

	protected override CustomsRegistry GetBusinessObjectToClone() => GetBusinessObjectToSerialise();

	protected override CustomsRegistry GetBusinessObjectToSerialise() => BizObj;
}

[TestedType(typeof(CustomsRegistryItem))]
sealed class CustomsRegistryItemTest : StronglyTypedRegistryItemTestCase<CustomsRegistryCollection>
{
	protected override StronglyTypedRegistryItem<CustomsRegistryCollection, CustomsRegistryCollection> GetNewRegistryItem() => new CustomsRegistryItem("", null, null, null, RegistryStorageFlags.All, new CustomsRegistryCollection());
}
