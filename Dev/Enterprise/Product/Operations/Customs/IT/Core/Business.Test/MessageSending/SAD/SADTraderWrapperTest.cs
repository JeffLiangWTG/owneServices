using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class SADTraderWrapperTest : TestCaseWithFactory
{
	public void TestIdCountryCodeAndIdEuropeanCountryAndNaturalPerson()
	{
		orgAddress.OA_RN_NKCountryCode = "DE";
		orgHeader.OH_Category = "NAT";
		orgCusCode.OK_CodeType = "EOR";
		orgCusCode.OK_RN_NKCodeCountry = "DE";
		orgCusCode.OK_CustomsRegNo = "385040449";
		var codCusCode = orgHeader.CustomsCodes.AddNew();
		codCusCode.OK_CodeType = "COD";
		codCusCode.OK_RN_NKCodeCountry = "IT";
		codCusCode.OK_CustomsRegNo = "123456789";

		traderWrapper = GetNewSADTraderWrapper();
		AssertTraderWrapperIdCountryCodeAndId(traderWrapper, "DE", "385040449");

		orgHeader.CustomsCodes.RemoveAndDelete(orgCusCode);
		traderWrapper = GetNewSADTraderWrapper();
		AssertTraderWrapperIdCountryCodeAndId(traderWrapper, "IT", "123456789");

		orgHeader.CustomsCodes.RemoveAndDelete(codCusCode);
		traderWrapper = GetNewSADTraderWrapper();
		AssertEmptyOrZeroIdCountrCodeAndId("DE");
	}

	public void TestIdCountryCodeAndIdEuropeanCountryAndNotNaturalPerson()
	{
		orgAddress.OA_RN_NKCountryCode = "DE";
		orgHeader.OH_Category = "BUS";
		orgCusCode.OK_CodeType = "EOR";
		orgCusCode.OK_RN_NKCodeCountry = "DE";
		orgCusCode.OK_CustomsRegNo = "385040449";
		var ivaCusCode = orgHeader.CustomsCodes.AddNew();
		ivaCusCode.OK_CodeType = "IVA";
		ivaCusCode.OK_RN_NKCodeCountry = "IT";
		ivaCusCode.OK_CustomsRegNo = "123456789";

		traderWrapper = GetNewSADTraderWrapper();
		AssertTraderWrapperIdCountryCodeAndId(traderWrapper, "DE", "385040449");

		orgHeader.CustomsCodes.RemoveAndDelete(orgCusCode);
		traderWrapper = GetNewSADTraderWrapper();
		AssertTraderWrapperIdCountryCodeAndId(traderWrapper, "IT", "123456789");

		orgHeader.CustomsCodes.RemoveAndDelete(ivaCusCode);
		traderWrapper = GetNewSADTraderWrapper();
		AssertEmptyOrZeroIdCountrCodeAndId("DE");
	}

	public void TestIdCountryCodeAndIdNotEuropeanCountryWithEoriCode()
	{
		orgAddress.OA_RN_NKCountryCode = "US";
		orgHeader.OH_Category = "BUS";
		orgCusCode.OK_CodeType = "EOR";
		orgCusCode.OK_RN_NKCodeCountry = "US";
		orgCusCode.OK_CustomsRegNo = "385040449";
		AssertTraderWrapperIdCountryCodeAndId(traderWrapper, "US", "385040449");
	}

	public void TestIdCountryCodeAndIdNotEuropeanCountryWithoutEoriCode()
	{
		orgAddress.OA_RN_NKCountryCode = "US";
		orgHeader.OH_Category = "BUS";
		orgCusCode.OK_CodeType = "COD";
		orgCusCode.OK_CustomsRegNo = "385040449";
		traderWrapper = GetNewSADTraderWrapper();

		AssertEmptyOrZeroIdCountrCodeAndId("US");
	}

	public virtual void TestName()
	{
		orgHeader.OH_FullName = "IKEA";
		AssertEquals("IKEA", traderWrapper.Name);

		orgHeader.OH_FullName = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		AssertEquals("ABCDEFGHIJKLMNOPQRSTUVWXYZ012345678", traderWrapper.Name);
	}

	public void TestAddress()
	{
		orgAddress.Address1 = "MAINADDRESS";
		orgAddress.Address2 = "ABCEXPMEL";
		AssertEquals("MAINADDRESS ABCEXPMEL", traderWrapper.Address);
		orgAddress.Address2 = ZString.Empty;
		AssertEquals("MAINADDRESS", traderWrapper.Address);

		orgAddress.Address1 = "MAINADDRESS";
		orgAddress.Address2 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ012345678";
		AssertEquals("MAINADDRESS ABCDEFGHIJKLMNOPQRSTUVW", traderWrapper.Address);
	}

	public void TestPostCode()
	{
		orgAddress.Postcode = "4000";
		AssertEquals("4000", traderWrapper.Postcode);

		orgAddress.Postcode = "0123456789";
		AssertEquals("012345678", traderWrapper.Postcode);
	}

	public void TestCity()
	{
		orgAddress.City = "ABCEXPMEL";
		AssertEquals("ABCEXPMEL", traderWrapper.City);

		orgAddress.City = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		AssertEquals("ABCDEFGHIJKLMNOPQRSTUVWXYZ012345678", traderWrapper.City);
	}

	public void TestCountryCode()
	{
		orgAddress.OA_RN_NKCountryCode = "ZA";
		traderWrapper = GetNewSADTraderWrapper();
		AssertEquals("ZA", traderWrapper.CountryCode);
	}

	protected virtual SADTraderWrapper GetNewSADTraderWrapper() => new SADTraderWrapper(jobDocAddress);
	protected virtual ZBool ShouldAssertZeroCustomsCode => true;

	protected override void SetUp()
	{
		base.SetUp();

		orgHeader = Factory.New<OrgHeader>();
		orgAddress = orgHeader.Addresses.AddNew();
		orgCusCode = orgHeader.CustomsCodes.AddNew();
		jobDocAddress = Factory.New<JobDocAddress>();
		jobDocAddress.E2_OA_Address = orgAddress.PK;

		traderWrapper = GetNewSADTraderWrapper();
	}
	protected OrgHeader orgHeader;
	protected OrgAddress orgAddress;
	protected OrgCusCode orgCusCode;
	protected JobDocAddress jobDocAddress;
	protected SADTraderWrapper traderWrapper;

	protected void AssertTraderWrapperIdCountryCodeAndId(SADTraderWrapper traderWrapper, string idCountryCode, string id)
	{
		CombineAssertions(() =>
		{
			AssertEquals(idCountryCode, traderWrapper.IdCountryCode);
			AssertEquals(id, traderWrapper.ID);
		});
	}

	void AssertEmptyOrZeroIdCountrCodeAndId(string idCountryCode)
	{
		if (ShouldAssertZeroCustomsCode)
		{
			AssertTraderWrapperIdCountryCodeAndId(traderWrapper, idCountryCode, "0");
		}
		else
		{
			AssertTraderWrapperIdCountryCodeAndId(traderWrapper, "", "");
		}
	}
}

sealed class SADTraderWrapperCreatedByOrgAddressTest : SADTraderWrapperTest
{
	protected override SADTraderWrapper GetNewSADTraderWrapper() => new SADTraderWrapper(orgAddress);
}

sealed class SADTraderWrapperDoNotInheritTest : SADTraderWrapperTest
{
	public void TestShouldUseZeroCustomsCodePlaceholderIfNoneFound()
	{
		var sadTraderWrapperForTest = new SADTraderWrapperForTest(jobDocAddress);

		AssertEquals("ShouldUseZeroCustomsCodePlaceholderIfNoneFound", true, sadTraderWrapperForTest.ShouldUseZeroCustomsCodeExposed);
	}

	class SADTraderWrapperForTest : SADTraderWrapper
	{
		public SADTraderWrapperForTest(JobDocAddress jobDocAddress) : base(jobDocAddress)
		{
		}

		public ZBool ShouldUseZeroCustomsCodeExposed => base.ShouldUseZeroCustomsCodePlaceholderIfNoneFound;
	}
}

