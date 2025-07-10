using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(PermitOwnerDataProvider))]
sealed class PermitOwnerDataProviderTest : BasePassarDataProviderTest<PermitOwnerDataProvider>
{
	public void TestNew()
	{
		AssertNull("permitNumber == null", PermitOwnerDataProvider.New(null));
		AssertNotNull("permitNumber != null", PermitOwnerDataProvider.New(DocAddress));
	}

	public void TestName() => CombineAssertions(() =>
	{
		const string name = "OH_FullName";

		DocAddress.Organisation.OH_FullName = name;
		var dataProvider = CreateDataProvider();
		AssertEquals("Name is available when no CustomsCode", name, dataProvider.Name);

		DocAddress.Organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DUN");
		dataProvider = CreateDataProvider();
		Assert("Name is null when CustomsCode is available", dataProvider.Name.IsNullOrEmpty());
	});

	public void TestIdentificationNumber() => CombineAssertions(() =>
	{
		const string dunRegNo = "DUN";
		const string uidRegNo = "UID";
		const string bidRegNo = "BID";

		var dataProvider = CreateDataProvider();
		AssertNull("Null when no CustomCode is defined", dataProvider.IdentificationNumber);

		dataProvider = CreateDataProvider();
		DocAddress.Organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, dunRegNo);
		AssertEquals(dunRegNo, dataProvider.IdentificationNumber);

		dataProvider = CreateDataProvider();
		DocAddress.Organisation.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.UID, uidRegNo);
		AssertEquals(uidRegNo, dataProvider.IdentificationNumber);

		dataProvider = CreateDataProvider();
		DocAddress.Organisation.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, bidRegNo);
		AssertEquals(bidRegNo, dataProvider.IdentificationNumber);
	});

	public void TestIdentificationNumber_OverriddenByRestriction() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var restriction = declaration.Invoices.AddNew().InvoiceLines.AddNew().Restrictions.AddNew();

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "TestOrg";

		var orgAddress = orgHeader.MainAddress;
		orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;

		var restrictionPermitOwner = Factory.New<JobDocAddress>();
		restrictionPermitOwner.E2_ParentTableCode = restriction.TablePrefix;
		restrictionPermitOwner.E2_ParentID = restriction.PK;
		restrictionPermitOwner.E2_AddressOverride = true;
		restrictionPermitOwner.DocAddressType = DocAddressType.PermitOwner;
		restrictionPermitOwner.E2_OA_Address = orgAddress.PK;
		restriction.PermitOwnerDocAddress.OrganisationPK = orgHeader.PK;

		orgHeader.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "1000088059");

		var dataProvider = PermitOwnerDataProvider.New(restrictionPermitOwner);
		AssertEquals("Identification Number = BID (default)", "1000088059", dataProvider.IdentificationNumber);

		restriction.OverrideIdentification = true;
		restriction.CSI_ReferenceNumber2 = "1000088059x";

		dataProvider = PermitOwnerDataProvider.New(restrictionPermitOwner);
		AssertEquals("Identification Number = entered value (overridden)", "1000088059x", dataProvider.IdentificationNumber);
	});

	public void TestAddress() => CombineAssertions(() =>
	{
		var dataProvider = CreateDataProvider();

		AssertNotNull("Address is available when no CustomsCode", dataProvider.Address);
		AssertSame("cached", dataProvider.Address, dataProvider.Address);

		DocAddress.Organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DUN");
		dataProvider = CreateDataProvider();
		AssertNull("Address is null when CustomsCode is available", dataProvider.Address);
	});

	JobDocAddress DocAddress => docAddress ?? (docAddress = CreateDocAddress());
	JobDocAddress docAddress;

	JobDocAddress CreateDocAddress()
	{
		var orgAddress = Factory.New<OrgHeader>().MainAddress;
		var address = Factory.New<JobDocAddress>();
		address.E2_OA_Address = orgAddress.PK;
		return address;
	}

	protected override PermitOwnerDataProvider CreateDataProvider() => PermitOwnerDataProvider.New(DocAddress);
}
