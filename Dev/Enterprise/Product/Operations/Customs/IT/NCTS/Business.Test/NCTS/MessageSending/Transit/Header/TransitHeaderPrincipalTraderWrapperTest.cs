using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class TransitHeaderPrincipalTraderWrapperTest : NctsSADHeaderPrincipalTraderWrapperTest
{
	public override void TestTIRHolderIdentification()
	{
		AssertEquals(nameof(PrincipalTraderWrapper.TIRHolderIdentification), ZString.Empty, PrincipalTraderWrapper.TIRHolderIdentification);
	}

	public override void TestRepresentativeGuaranteeTaxIdentificationNumber()
	{
		AssertEquals("Representative Guarantee Tax Identification Number should be empty when no Cus Code exists", ZString.Empty, PrincipalTraderWrapper.RepresentativeGuaranteeTaxIdentificationNumber);

		representative.Organisation.CustomsCodes.AddNew("EOR", "EOR CODE", "IT");

		AssertEquals(nameof(PrincipalTraderWrapper.RepresentativeGuaranteeTaxIdentificationNumber), "EOR CODE", PrincipalTraderWrapper.RepresentativeGuaranteeTaxIdentificationNumber);

		representative.Organisation.CustomsCodes.AddNew("IVA", "VAT (IVA) CODE", "IT");
		AssertEquals(nameof(PrincipalTraderWrapper.RepresentativeGuaranteeTaxIdentificationNumber), "VAT (IVA) CODE", PrincipalTraderWrapper.RepresentativeGuaranteeTaxIdentificationNumber);

		representative.Organisation.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		AssertEquals(nameof(PrincipalTraderWrapper.RepresentativeGuaranteeTaxIdentificationNumber), "EOR CODE", PrincipalTraderWrapper.RepresentativeGuaranteeTaxIdentificationNumber);

		representative.Organisation.CustomsCodes.AddNew("COD", "FISCAL CODE", "IT");
		AssertEquals(nameof(PrincipalTraderWrapper.RepresentativeGuaranteeTaxIdentificationNumber), "FISCAL CODE", PrincipalTraderWrapper.RepresentativeGuaranteeTaxIdentificationNumber);

		representative = null;
		traderWrapper = GetNewSADTraderWrapper();
		AssertEquals("Representative Name should be empty when Representative is not defined", ZString.Empty, PrincipalTraderWrapper.RepresentativeName);
	}

	public override void TestRepresentativeName()
	{
		representative.Organisation.OH_FullName = "Representative full name";
		AssertEquals(nameof(PrincipalTraderWrapper.RepresentativeName), "Representative full name", PrincipalTraderWrapper.RepresentativeName);

		representative = null;
		traderWrapper = GetNewSADTraderWrapper();
		AssertEquals("Representative Name should be empty when Representative is not defined", ZString.Empty, PrincipalTraderWrapper.RepresentativeName);
	}

	protected override void SetUp()
	{
		orgHeader = Factory.New<OrgHeader>();

		nctsHeader = Factory.NewDepartureNctsHeader();
		var representativeAddress = orgHeader.Addresses.AddNew();
		representative = nctsHeader.MovementHeader.Representative;
		representative.E2_OA_Address = representativeAddress.PK;

		orgAddress = orgHeader.Addresses.AddNew();
		orgCusCode = orgHeader.CustomsCodes.AddNew();
		jobDocAddress = Factory.New<JobDocAddress>();
		jobDocAddress.E2_OA_Address = orgAddress.PK;

		traderWrapper = GetNewSADTraderWrapper();
	}

	NctsHeader nctsHeader;
	JobDocAddress representative;

	protected override SADTraderWrapper GetNewSADTraderWrapper() => new TransitHeaderPrincipalTraderWrapper(jobDocAddress, representative);

	protected override NctsSADHeaderPrincipalTraderWrapper GetPrincipalTraderWrapper(JobDocAddress principalTraderAddress, JobDocAddress representativeAddress) => new TransitHeaderPrincipalTraderWrapper(principalTraderAddress, representativeAddress);
}
