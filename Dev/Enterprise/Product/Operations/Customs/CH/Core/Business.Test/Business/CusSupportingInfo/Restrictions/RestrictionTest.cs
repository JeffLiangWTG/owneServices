using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(Restriction))]
class RestrictionTest : Customs.Business.Testing.CusSupportingInfoTest<Restriction>
{
	public void TestCSI_LineNo() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(Restriction.CSI_LineNoInfo, caption: "Item Number");

		var invoiceLine = Restriction.Parent;
		AssertEquals("1 added - 1st", 1, Restriction.CSI_LineNo);

		var restriction2 = invoiceLine.Restrictions.AddNew();
		AssertEquals("2 added - 1st", 1, Restriction.CSI_LineNo);
		AssertEquals("2 added - 2nd", 2, restriction2.CSI_LineNo);

		var restriction3 = invoiceLine.Restrictions.AddNew();
		AssertEquals("3 added-  1st", 1, Restriction.CSI_LineNo);
		AssertEquals("3 added - 2nd", 2, restriction2.CSI_LineNo);
		AssertEquals("3 added - 3rd", 3, restriction3.CSI_LineNo);

		restriction2.Delete();
		AssertEquals("2 deleted - 1st", 1, Restriction.CSI_LineNo);
		AssertEquals("2 deleted - 3rd", 2, restriction3.CSI_LineNo);

		var restriction4 = invoiceLine.Restrictions.AddNew();
		AssertEquals("4 added - 1st", 1, Restriction.CSI_LineNo);
		AssertEquals("4 added - 3rd", 2, restriction3.CSI_LineNo);
		AssertEquals("4 added - 4th", 3, restriction4.CSI_LineNo);
	});

	public void TestCSI_Code() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(Restriction.CSI_CodeInfo, caption: "Code");
		AssertEquals(3, Restriction.CSI_CodeInfo.MaxLength);
	});

	public void TestCSI_ReferenceNumber() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(Restriction.CSI_ReferenceNumberInfo, caption: "Permit Number");
		AssertEquals(35, Restriction.CSI_ReferenceNumberInfo.MaxLength);
	});

	public void TestCSI_Description() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(Restriction.CSI_DescriptionInfo, caption: "Permit Exception Reason");
		AssertEquals(35, Restriction.CSI_DescriptionInfo.MaxLength);
	});

	public void TestPermitOwnerIdentification()
	{
		CaptionTestHelper.AssertCaptions(Restriction.PermitOwnerIdentificationInfo, caption: "Identification");
	}

	public void TestPermitOwnerIdentification_Populated()
	{
		const string bid = "1000088059";
		var permitOwnerOrg = SetupPermitOwner();
		permitOwnerOrg.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, bid, Core.Constants.CountryCodes.Switzerland);
		AssertEquals("PermitOwnerIdentification = BID", bid, Restriction.PermitOwnerIdentification);
	}

	public void TestOverrideIdentification()
	{
		CaptionTestHelper.AssertCaptions(Restriction.OverrideIdentificationInfo, caption: "Override");
	}

	public void TestOverrideIdentification_ReadOnly() => CombineAssertions(() =>
	{
		AssertEquals("readonly if PermitOwner empty", true, Restriction.OverrideIdentification_ReadOnly);

		var permitOwnerOrg = SetupPermitOwner();

		AssertEquals("readonly if PermitOwner entered", false, Restriction.OverrideIdentification_ReadOnly);
	});

	public void TestOverrideIdentification_Untick()
	{
		var permitOwnerOrg = SetupPermitOwner();

		Restriction.OverrideIdentification = true;
		Restriction.CSI_ReferenceNumber2 = "1000088059x";
		Restriction.OverrideIdentification = false;
		AssertEquals("after untick Override", ZString.Empty, Restriction.CSI_ReferenceNumber2);
	}

	public void TestCSI_ReferenceNumber2() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(Restriction.CSI_ReferenceNumber2Info, caption: "Identification");
	});

	public void TestOnLoad() => CombineAssertions(() =>
	{
		var permitOwnerOrg = SetupPermitOwner();
		permitOwnerOrg.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "1000088059", Core.Constants.CountryCodes.Switzerland);

		Restriction.OverrideIdentification = true;
		Restriction.CSI_ReferenceNumber2 = "1000088059x";
		Factory.Save();

		var restReloadFromDB = Factory.Load<Restriction>(Restriction.PK);
		AssertEquals("OverridePermitOwner overridden", true, restReloadFromDB.OverrideIdentification);
		AssertEquals("OverriddenIdentification ", "1000088059x", restReloadFromDB.CSI_ReferenceNumber2);
	});

	public void TestCSI_ReferenceNumber2_ReadOnly() => CombineAssertions(() =>
	{
		var permitOwnerOrg = SetupPermitOwner();

		Restriction.OverrideIdentification = false;
		AssertEquals("readonly if Override unticked", true, Restriction.CSI_ReferenceNumber2_ReadOnly);

		Restriction.OverrideIdentification = true;
		AssertEquals("not readonly if Override ticked", false, Restriction.CSI_ReferenceNumber2_ReadOnly);
	});

	public void TestDefaults() => CombineAssertions(() =>
	{
		var restriction = Factory.New<Restriction>();
		AssertEquals("CSI_Type", Common.CH.CusSupportingInfoTypeList.Codes.Restriction, restriction.CSI_Type);
	});

	public void TestDocAddressesRelatedProperties() => CombineAssertions(() =>
	{
		var docAddress = Factory.New<JobDocAddress>();
		docAddress.E2_ParentTableCode = Restriction.TablePrefix;
		docAddress.E2_ParentID = Restriction.PK;
		Factory.Save();

		AssertEquals("Count", 1, Restriction.DocAddresses.Count);
		AssertSame("Should load the correcte item.", Restriction.DocAddresses[0], docAddress);

		IDocAddresses docAddresses = Restriction;
		Assert("GetSupportedAddressTypes", docAddresses.SupportedAddressTypes.Count == 1 && docAddresses.SupportedAddressTypes[0] == DocAddressType.PermitOwner);
		AssertEquals("GetCanOverrideCheckpoint", Env.Security.None, docAddresses.GetCanOverrideCheckpoint(docAddress));
		AssertNotNull("OrgHeaderList", docAddresses.GetOrgHeaderList(DocAddressType.PermitOwner));
	});

	public void TestPermitOwnerDocAddress() => CombineAssertions(() =>
	{
		AssertEquals("DocAddressType", DocAddressType.PermitOwner, Restriction.PermitOwnerDocAddress.DocAddressType);

		Assert("DocAddresses", Restriction.DocAddresses.Contains(Restriction.PermitOwnerDocAddress));

		IDocAddresses docAddresses = Restriction;
		var supportedAddressTypes = new List<DocAddressType>(docAddresses.SupportedAddressTypes);
		AssertEquals("supportedAddressTypes", true, supportedAddressTypes.Contains(DocAddressType.PermitOwner));
		AssertEquals("DefaultDocAddressType", Restriction.PermitOwnerDocAddress.DocAddressType, docAddresses.GetDocAddressRequirement(DocAddressType.PermitOwner).DefaultDocAddressType);
	});

	public void TestLookups()
	{
		AssertType<RestrictionLookups>(Restriction.Lookups);
	}

	public void TestValidation() => CombineAssertions(() =>
	{
		AssertType<ExportRestrictionValidation>(Restriction.Validation);

		var restriction = Factory.New<Restriction>();
		AssertType<RestrictionValidation>(restriction.Validation);
	});

	public void TestAdditionalInformation()
	{
		AssertSame("Cached", Restriction.AdditionalInformations, Restriction.AdditionalInformations);
		AssertType<RestrictionAdditionalInformationCollection>(Restriction.AdditionalInformations);
		AssertType<RestrictionAdditionalInformation>(Restriction.AdditionalInformations.AddNew());
	}

	public void TestIsPermitNumberAllowed()
	{
		RefCusCodeTestHelper.CreateRestrictionCodeWithPermitNumberAttribute(Factory);
		var restriction = Factory.New<Restriction>();

		restriction.CSI_Code = RefCusCodeTestHelper.RestrictionCodeWithPermitNumberAllowedAttributeY;
		AssertEquals("restriction.IsPermitNumberAllowed", true, restriction.IsPermitNumberAllowed);

		restriction.CSI_Code = RefCusCodeTestHelper.RestrictionCodeWithPermitNumberAllowedAndPermitExceptionReasonAttributesN;
		AssertEquals("restriction.IsPermitNumberAllowed", false, restriction.IsPermitNumberAllowed);

		restriction.CSI_Code = "999";
		AssertEquals("restriction.IsPermitNumberAllowed", true, restriction.IsPermitNumberAllowed);
	}

	public void TestIsPermitExceptionReasonAllowed()
	{
		RefCusCodeTestHelper.CreateRestrictionCodeWithPermitNumberAttribute(Factory);
		var restriction = Factory.New<Restriction>();

		restriction.CSI_Code = RefCusCodeTestHelper.RestrictionCodeWithPermitNumberAllowedAttributeY;
		AssertEquals("restriction.IsPermitExceptionReasonAllowed", false, restriction.IsPermitExceptionReasonAllowed);

		restriction.CSI_Code = RefCusCodeTestHelper.RestrictionCodeWithPermitNumberAllowedAndPermitExceptionReasonAttributesY;
		AssertEquals("restriction.IsPermitExceptionReasonAllowed", true, restriction.IsPermitExceptionReasonAllowed);

		restriction.CSI_Code = "999";
		AssertEquals("restriction.IsPermitExceptionReasonAllowed", false, restriction.IsPermitExceptionReasonAllowed);
	}

	public void TestIsAdditionalInformation() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateRestrictionCodeAdditionalInformationAttribute(Factory);
		var restriction = Factory.New<Restriction>();

		restriction.CSI_Code = RefCusCodeTestHelper.RestrictionCodeWithAdditionalInformationAttributeN;
		AssertEquals("restriction.IsAdditionalInformation", false, restriction.IsAdditionalInformation);

		restriction.CSI_Code = RefCusCodeTestHelper.RestrictionCodeWithAdditionalInformationAttributeY;
		AssertEquals("restriction.IsAdditionalInformation", true, restriction.IsAdditionalInformation);
	});

	protected override BusinessObject GetNewBusinessObject() => GetNewRestriction(Factory);

	protected override IEnumerable<Restriction> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return GetNewRestriction(factory);
	}

	Restriction Restriction => restriction ??= GetNewRestriction(Factory);
	Restriction restriction;

	Restriction GetNewRestriction(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		return declaration.Invoices.AddNew().InvoiceLines.AddNew().Restrictions.AddNew();
	}

	OrgHeader SetupPermitOwner()
	{
		var permitOwner = Factory.New<JobDocAddress>();
		permitOwner.E2_ParentTableCode = Restriction.TablePrefix;
		permitOwner.E2_ParentID = Restriction.PK;
		permitOwner.E2_AddressOverride = true;
		permitOwner.DocAddressType = DocAddressType.PermitOwner;

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "TestOrg";

		var orgAddress = orgHeader.MainAddress;
		orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;

		permitOwner.E2_OA_Address = orgAddress.PK;

		Restriction.PermitOwnerDocAddress.OrganisationPK = orgHeader.PK;

		return orgHeader;
	}
}
