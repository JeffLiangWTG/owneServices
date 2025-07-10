using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(SenderInfo))]
class SenderInfoTest : RegistryBusinessObjectTemplateTestCase<SenderInfo>
{
	public void TestValidateOrganizationPK()
	{
		var org1 = Factory.New<OrgHeader>();
		org1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, ZString.Empty, Core.Constants.CountryCodes.Netherlands);
		var coll = new SenderInfoCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
		var mapping = coll.AddNew();
		mapping.OrganizationPK = ZGuid.Empty;
		var mustBeEnteredMessage = MandatoryValidation.MustBeEnteredMessage("Organization");
		AssertHasError(mapping.OrganizationPKInfo, mustBeEnteredMessage);
		AssertNoErrorContaining(mapping.OrganizationPKInfo, ListValidation.InvalidCodeError);
		mapping.OrganizationPK = ZGuid.Invalid;
		AssertNoError(mapping.OrganizationPKInfo, mustBeEnteredMessage);
		AssertHasErrorContaining(mapping.OrganizationPKInfo, ListValidation.InvalidCodeError);
		mapping.OrganizationPK = org1.PK;
		AssertNoError(mapping.OrganizationPKInfo, mustBeEnteredMessage);
		AssertNoErrorContaining(mapping.OrganizationPKInfo, ListValidation.InvalidCodeError);
	}

	public void TestValidateDefaultFlag()
	{
		OrgHeader org1 = Factory.New<OrgHeader>();
		org1.OH_FullName = "Test Company 1";
		org1.OH_Code = "TestComp1";
		OrgHeader org2 = Factory.New<OrgHeader>();
		org2.OH_FullName = "Test Company 2";
		org2.OH_Code = "TestComp2";
		OrgHeader org3 = Factory.New<OrgHeader>();
		org2.OH_FullName = "Test Company 3";
		org2.OH_Code = "TestComp3";
		string multipleDefaults = (NoResString)"You can only set one Sender ID as default";

		var coll = new SenderInfoCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
		var mapping1 = coll.AddNew();
		mapping1.OrganizationPK = org1.PK;
		mapping1.SenderID = "123";
		mapping1.DefaultSenderID = true;
		AssertNoErrorContaining(mapping1.DefaultSenderIDInfo, multipleDefaults);

		var mapping2 = coll.AddNew();
		mapping2.OrganizationPK = org2.PK;
		mapping2.SenderID = "456";
		mapping2.DefaultSenderID = false;
		AssertNoErrorContaining(mapping2.DefaultSenderIDInfo, multipleDefaults);

		var mapping3 = coll.AddNew();
		mapping3.OrganizationPK = org3.PK;
		mapping3.SenderID = "789";
		mapping3.DefaultSenderID = true;
		AssertHasErrorContaining(mapping3.DefaultSenderIDInfo, multipleDefaults);
	}

	public void TestValidateSenderID()
	{
		OrgHeader org1 = Factory.New<OrgHeader>();
		org1.OH_FullName = "Test Company 1";
		org1.OH_Code = "TestComp1";
		OrgHeader org2 = Factory.New<OrgHeader>();
		org2.OH_FullName = "Test Company 2";
		org2.OH_Code = "TestComp2";
		var mustBeEnteredMessage = MandatoryValidation.MustBeEnteredMessage("Sender ID");

		var coll = new SenderInfoCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
		var mapping1 = coll.AddNew();
		mapping1.OrganizationPK = org1.PK;
		mapping1.SenderID = "123";
		mapping1.DefaultSenderID = true;
		AssertNoError(mapping1.SenderIDInfo, mustBeEnteredMessage);

		var mapping2 = coll.AddNew();
		mapping2.OrganizationPK = org2.PK;
		mapping2.SenderID = "";
		mapping2.DefaultSenderID = false;
		AssertHasError(mapping2.SenderIDInfo, mustBeEnteredMessage);
	}

	protected override bool RequiresFactory
	{
		get { return true; }
	}

	protected override bool RequiresFallbackLevel
	{
		get { return true; }
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var coll = new SenderInfoCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
		return coll.AddNew();
	}

	protected override SenderInfo GetBusinessObjectToClone()
	{
		return (SenderInfo)GetNewBusinessObject();
	}

	protected override SenderInfo GetBusinessObjectToSerialise()
	{
		return GetBusinessObjectToClone();
	}
}
