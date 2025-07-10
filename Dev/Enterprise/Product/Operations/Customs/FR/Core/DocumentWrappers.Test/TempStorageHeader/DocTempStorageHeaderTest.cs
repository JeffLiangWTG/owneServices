using CargoWise.Types;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DocumentWrappers.TempStorageHeader.Testing;

sealed class DocTempStorageHeaderTest : DocBaseWrapperTest
{
	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		return DocTempStorageHeader.New(header, Factory);
	}

	new DocTempStorageHeader Wrapper => (DocTempStorageHeader)base.Wrapper;

	#region Related Business Objects

	public void TestLines()
	{
		header.CusTempStorageDec.CusTempStorageLines.AddNew();
		header.CusTempStorageDec.CusTempStorageLines.AddNew();
		header.CusTempStorageDec.CusTempStorageLines.AddNew();
		AssertEquals(3, Wrapper.Lines.Count);
	}

	#endregion

	#region Properties

	public void TestInternalReference()
	{
		header.SJH_JobReference = "FRJ00020";
		AssertEquals("FRJ00020", Wrapper.InternalReference);
	}

	public void TestCustomerReference()
	{
		header.DDTNumber = "DDT123456";
		header.SJH_ReferenceNumber = "IST_200058";
		AssertEquals("DDT123456 / IST_200058", Wrapper.CustomerReference);
	}

	[TestDate(2020, 06, 30)]
	public void TestReportDate()
	{
		header.SJH_PresentationDate = ZDate.Today.AddDays(1);
		AssertEquals("01/07/2020", Wrapper.ReportDate);
	}

	public void TestCustomerAddress()
	{
		header.Customer.OH_Code = "EASYLOG";
		header.Customer.OH_FullName = "EASYLOG";
		header.Customer.MainAddress.OA_Address1 = "27 RUE D ALSACE";
		header.Customer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
		header.Customer.MainAddress.OA_City = "FRANCONVILLE";
		header.Customer.MainAddress.OA_PostCode = "95130";
		AssertEquals("EASYLOG\n27 RUE D ALSACE\n95130 FRANCONVILLE\nFRANCE", Wrapper.CustomerAddress);
	}

	public void TestCustomerTelephone()
	{
		header.Customer.MainAddress.OA_Phone = "+33100200300";
		AssertEquals("+33100200300", Wrapper.CustomerTelephone);
	}

	public void TestCustomerAgreement()
	{
		header.SJH_CustomsProfile = "Test CustomsProfile";
		AssertEquals("Test CustomsProfile", Wrapper.CustomerAgreement);
	}

	public void TestTaxID()
	{
		header.Customer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "FR32582075100080", Core.Constants.CountryCodes.France);
		AssertEquals("FR32582075100080", Wrapper.TaxID);
	}

	public void TestOfficeCode()
	{
		header.SJH_CustomsOffice = "Off1";
		AssertEquals("Off1", Wrapper.OfficeCode);
	}

	public void TestOfficeName()
	{
		var startDate = ZDateTime.Today.AddDays(-2);
		var endDate = ZDateTime.Today.AddDays(2);
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Test");
		helper.CreateNewOrGetExistingCusCodeList(GlbBranch.CurrentBranch.Company.Country.Code, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Off1", "TestDescription", startDate, endDate);
		Factory.Save();

		var customer = Factory.NewWithValidTestData<OrgHeader>();
		var presenter = Factory.NewWithValidTestData<OrgAddress>();
		var representative = Factory.NewWithValidTestData<OrgAddress>();

		header.SJH_GB = GlbBranch.CurrentBranch.PK;
		header.SJH_JobReference = "From1";
		header.SJH_OH_Customer = customer.PK;
		header.SJH_OA_Presenter = presenter.PK;
		header.SJH_OA_Representative = representative.PK;
		header.SJH_CustomsOffice = "Off1";
		AssertEquals("TestDescription", Wrapper.OfficeName);
	}

	public void TestPreviousDocument()
	{
		header.SJH_PreviousReferenceType = "PRE";
		header.SJH_PreviousReferenceNumber = "20FR987987897";
		AssertEquals("PRE Z 20FR987987897", Wrapper.PreviousDocument);
	}

	public void TestCountryOfOrigin()
	{
		header.SJH_RL_NKLoading = "USATL";
		AssertEquals("US", Wrapper.CountryOfOrigin);
	}

	public void TestContainerNumber()
	{
		AssertEquals(ZString.Empty, Wrapper.ContainerNumber);

		var cusTempStorageContainers1 = storageDec.CusTempStorageContainers.AddNew();
		cusTempStorageContainers1.CY_Data = "cyDate1";
		cusTempStorageContainers1.CY_Code = "A1";

		var cusTempStorageContainers2 = storageDec.CusTempStorageContainers.AddNew();
		cusTempStorageContainers2.CY_Data = "cyDate2";
		cusTempStorageContainers2.CY_Code = "A2";

		AssertEquals("cyDate1; cyDate2", Wrapper.ContainerNumber);
	}

	public void TestTotalGrossWeight()
	{
		header.CusTempStorageDec.CusTempStorageLines.AddNew().TSL_GrossWeight = 1m;
		header.CusTempStorageDec.CusTempStorageLines.AddNew().TSL_GrossWeight = 10m;
		header.CusTempStorageDec.CusTempStorageLines.AddNew().TSL_GrossWeight = 100m;
		AssertEquals(111m, Wrapper.TotalGrossWeight);
	}

	#endregion
	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<CusTempStorageJobHeader>();
		storageDec = ISTCusTempStorageDec.New(header);

		var customer = Factory.New<OrgHeader>();
		customer.OH_Code = "Test Code";
		header.SJH_OH_Customer = customer.PK;
	}

	CusTempStorageJobHeader header;
	CusTempStorageDec storageDec;
}
