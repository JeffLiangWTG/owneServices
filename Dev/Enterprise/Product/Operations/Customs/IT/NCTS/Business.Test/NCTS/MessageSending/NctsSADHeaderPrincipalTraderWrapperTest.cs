using CargoWise.Types;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

abstract class NctsSADHeaderPrincipalTraderWrapperTest : SADTraderWrapperTest
{
	public void TestTraderGuaranteeTaxIdentificationNumber()
	{
		AssertEquals(nameof(PrincipalTraderWrapper.TraderGuaranteeTaxIdentificationNumber), ZString.Empty, PrincipalTraderWrapper.TraderGuaranteeTaxIdentificationNumber);
	}

	public abstract void TestTIRHolderIdentification();

	public virtual void TestRepresentativeGuaranteeTaxIdentificationNumber()
	{
		AssertEquals(nameof(PrincipalTraderWrapper.RepresentativeGuaranteeTaxIdentificationNumber), ZString.Empty, PrincipalTraderWrapper.RepresentativeGuaranteeTaxIdentificationNumber);
	}

	public virtual void TestRepresentativeName()
	{
		AssertEquals(nameof(PrincipalTraderWrapper.RepresentativeName), ZString.Empty, PrincipalTraderWrapper.RepresentativeName);
	}

	public void TestRepresentativeType()
	{
		AssertEquals(nameof(PrincipalTraderWrapper.RepresentativeType), ZString.Empty, PrincipalTraderWrapper.RepresentativeType);
	}

	public void TestIDAndIdCountryCode()
	{
		var emptyPrincipalTraderWrapper = GetPrincipalTraderWrapper(null, null);
		AssertIDAndCountryCodeAreEmpty(emptyPrincipalTraderWrapper);

		var principalTraderAddress = Factory.New<JobDocAddress>();
		var principalTraderWrapper = GetPrincipalTraderWrapper(principalTraderAddress, null);
		AssertIDAndCountryCodeAreEmpty(principalTraderWrapper);

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Category = "BUS";
		principalTraderAddress.OrganisationPK = orgHeader.PK;

		principalTraderWrapper = GetPrincipalTraderWrapper(principalTraderAddress, null);
		AssertIDAndCountryCodeAreEmpty(principalTraderWrapper);

		var vatCusCode = orgHeader.CustomsCodes.AddNew();
		vatCusCode.OK_CodeType = "IVA";
		vatCusCode.OK_RN_NKCodeCountry = "IT";
		vatCusCode.OK_CustomsRegNo = "123456";

		principalTraderWrapper = GetPrincipalTraderWrapper(principalTraderAddress, null);
		AssertTraderWrapperIdCountryCodeAndId(principalTraderWrapper, "IT", "123456");

		var eorCusCode = orgHeader.CustomsCodes.AddNew("EOR", "654321", "DE");
		eorCusCode.OK_CodeType = "EOR";
		eorCusCode.OK_RN_NKCodeCountry = "DE";
		eorCusCode.OK_CustomsRegNo = "654321";

		principalTraderWrapper = GetPrincipalTraderWrapper(principalTraderAddress, null);
		AssertTraderWrapperIdCountryCodeAndId(principalTraderWrapper, "DE", "654321");

		void AssertIDAndCountryCodeAreEmpty(NctsSADHeaderPrincipalTraderWrapper traderWrapper)
		{
			CombineAssertions("Check ID and IdCountry code are empty", () =>
			{
				AssertEquals("IdCountryCode", "", traderWrapper.IdCountryCode);
				AssertEquals("ID", "", traderWrapper.ID);
			});
		}
	}

	protected NctsSADHeaderPrincipalTraderWrapper PrincipalTraderWrapper => (NctsSADHeaderPrincipalTraderWrapper)traderWrapper;

	protected abstract NctsSADHeaderPrincipalTraderWrapper GetPrincipalTraderWrapper(JobDocAddress principalTraderAddress, JobDocAddress representativeAddress);
	protected sealed override ZBool ShouldAssertZeroCustomsCode => false;
}
