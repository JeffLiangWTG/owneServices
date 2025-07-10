using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class CusSupplyChainActorReferenceValidationTest : TestCaseWithFactory
{
	public void TestCheckOwnerOrgPK()
	{
		const string messageError = "Organization is missing a Registration Number of type 'AEO'.";
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var cusSupplyChainActorReference = entryInstruction.CusSupplyChainActorReferences.AddNew();
		var orgHeader = Factory.New<OrgHeader>();
		var propertyInfo = cusSupplyChainActorReference.OwnerOrgPKInfo;
		CombineAssertions(() =>
		{
			cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;
			AssertHasMessageError("Missing AEO customs code", propertyInfo, messageError);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "regnum", Core.Constants.CountryCodes.Germany);
			cusSupplyChainActorReference.Validation.ValidateOwnerOrgPK();
			AssertNoMessageError("AEO customs code exists and not care specific country", propertyInfo, messageError);
		});
	}
}
