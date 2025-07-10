using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class H7PartyNameWrapperTest : DataProviderTestCase<H7PartyNameWrapper>
	{
		public void TestId_WhenEORIAndNIFIsAvailable_ShouldReturnEORI()
		{
			var eori = "ES000123";
			var orgAddress = CreateOrgAddress("My Company", "EOR", eori);
			var nifCusCode = orgAddress.CustomsCodes.AddNew();
			nifCusCode.OK_CodeType = "NIF";
			nifCusCode.OK_CustomsRegNo = "NIF0001";
			var wrapper = new H7PartyNameWrapper(orgAddress);

			AssertEquals(eori, wrapper.Id);
		}

		public void TestId_WhenNIFIsAvailable_ShouldReturnNIF()
		{
			var nif = "ESNIF0001";
			var orgAddress = CreateOrgAddress("My Company", "NIF", nif);
			var wrapper = new H7PartyNameWrapper(orgAddress);

			AssertEquals(nif, wrapper.Id);
		}

		public void TestId_WhenEORIOrNIFIsNotAvailable_ShouldReturnEmptyString()
		{
			var orgAddress = CreateOrgAddress("My Company", "PAS", "PAS0001");
			var wrapper = new H7PartyNameWrapper(orgAddress);

			AssertEquals(string.Empty, wrapper.Id);
		}

		public void TestId_WhenAddressIsNull_ShouldReturnEmptyString()
		{
			var wrapper = new H7PartyNameWrapper(null);
			AssertEquals(string.Empty, wrapper.Id);
		}

		public void TestName_WhenEORIIsAvailable_ShouldReturnFullName()
		{
			var orgAddress = CreateOrgAddress("My Company", "EOR", "ES000123");
			var wrapper = new H7PartyNameWrapper(orgAddress);

			AssertEquals("My Company", wrapper.Name);
		}

		public void TestName_WhenEORIIsNotAvailable_ShouldReturnFullName()
		{
			var orgAddress = CreateOrgAddress("My Company", "PAS", "PAS0001");
			var wrapper = new H7PartyNameWrapper(orgAddress);

			AssertEquals("My Company", wrapper.Name);
		}

		public void TestName_WhenHeaderIsNull_ShouldReturnEmptyString()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = new ZGuid();

			var wrapper = new H7PartyNameWrapper(orgAddress);

			AssertEquals(string.Empty, wrapper.Name);
		}

		public void TestName_WhenAddressIsNull_ShouldReturnEmptyString()
		{
			var wrapper = new H7PartyNameWrapper(null);
			AssertEquals(string.Empty, wrapper.Name);
		}

		OrgAddress CreateOrgAddress(string headerName, string cusCodeType, string cusCodeRegNo)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = headerName;

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;

			var cusCode = orgAddress.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = cusCodeRegNo;
			cusCode.OK_CodeType = cusCodeType;
			return orgAddress;
		}

		protected override H7PartyNameWrapper GetProvider()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			return new H7PartyNameWrapper(orgAddress);
		}
	}
}
