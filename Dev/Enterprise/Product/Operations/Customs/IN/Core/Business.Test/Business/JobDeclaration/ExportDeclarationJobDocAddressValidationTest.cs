using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(ExportDeclarationJobDocAddressValidation))]
public class ExportDeclarationJobDocAddressValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckOrganisationPK_TRS()
	{
		var warning = "PAN No. is missing for selected organisation.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var orgHeader = Factory.New<OrgHeader>();

		var transhipper = declaration.TranshipperDocAddress;
		var propertyInfo = transhipper.OrganisationPKInfo;

		var otherDocAddress = declaration.ExportOrientedUnitsDocAddress;
		otherDocAddress.OrganisationPK = orgHeader.PK;
		var otherPropertyInfo = otherDocAddress.OrganisationPKInfo;
		CombineAssertions(() =>
		{
			AssertNoWarning("PAN exists", propertyInfo, warning);

			transhipper.OrganisationPK = orgHeader.PK;
			transhipper.Validation.ValidateOrganisationPK();
			AssertHasWarning("No PAN", propertyInfo, warning);
			AssertNoWarning("Other doc address", otherPropertyInfo, warning);

			orgHeader.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.PAN, "123", Core.Constants.CountryCodes.India);
			transhipper.Validation.ValidateOrganisationPK();
			AssertNoWarning("PAN exists", propertyInfo, warning);
		});
	}
}
