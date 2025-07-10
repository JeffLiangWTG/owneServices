using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage.Testing;

[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Latvia)]
sealed class TemporaryStorageHeaderWrapperTest : DocBaseWrapperTest
{
	public void TestJobReferenceNumber()
	{
		header.AMA_JobReference = "TS00000001";
		AssertEquals("JobReferenceNumber", "TS00000001", Wrapper.JobReferenceNumber);
	}

	public void TestAuthorisationNumber()
	{
		header.GoodsLocation.Address.AuthorisationNumber = "9999000002";
		AssertEquals("AuthorisationNumber", "9999000002", Wrapper.AuthorisationNumber);
	}

	public void TestLocalClient()
	{
		const string expectedWrapperValue =
			"Company Name\r\n" +
			"Company Address1\r\n" +
			"Company Address2\r\n" +
			"12345\r\n" +
			"Company City\r\n" +
			"IT";

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "Company Name";
		orgHeader.MainAddress.OA_Address1 = "Company Address1";
		orgHeader.MainAddress.OA_Address2 = "Company Address2";
		orgHeader.MainAddress.OA_PostCode = "12345";
		orgHeader.MainAddress.OA_City = "Company City";
		orgHeader.MainAddress.OA_RN_NKCountryCode = "IT";

		jobHeader.JH_OA_LocalChargesAddr = orgHeader.MainAddress.PK;
		AssertMultilineASCIIEquals("LocalClient", expectedWrapperValue, Wrapper.LocalClient);
	}

	public void TestBills()
	{
		AssertType<TemporaryStorageBillWrapperCollection>(Wrapper.Bills);
	}

	public void TestDeclarant()
	{
		const string expectedWrapperValue =
			"Company Name\r\n" +
			"Company Address1\r\n" +
			"Company Address2\r\n" +
			"12345\r\n" +
			"Company City\r\n" +
			"ES";

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "Company Name";
		orgHeader.MainAddress.OA_Address1 = "Company Address1";
		orgHeader.MainAddress.OA_Address2 = "Company Address2";
		orgHeader.MainAddress.OA_PostCode = "12345";
		orgHeader.MainAddress.OA_City = "Company City";
		orgHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;

		header.AMA_OA_Declarant = orgHeader.MainAddress.PK;
		AssertMultilineASCIIEquals("Declarant", expectedWrapperValue, Wrapper.Declarant);
	}

	new TemporaryStorageHeaderWrapper Wrapper => (TemporaryStorageHeaderWrapper)base.Wrapper;

	protected override DocBaseWrapper GetNewDocumentWrapper() => TemporaryStorageHeaderWrapper.New(header, Factory);

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<TemporaryStorageHeader>();
		jobHeader = new JobHeader.Loader(header).TryLoadOrCreate();
	}

	TemporaryStorageHeader header;
	JobHeader jobHeader;
}
