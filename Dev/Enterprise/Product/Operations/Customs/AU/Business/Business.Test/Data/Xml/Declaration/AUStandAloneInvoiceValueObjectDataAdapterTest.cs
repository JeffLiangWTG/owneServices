using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUStandAloneInvoiceValueObjectDataAdapter))]
	class AUStandAloneInvoiceValueObjectDataAdapterTest : DataTransfer.Testing.StandAloneInvoiceValueObjectDataAdapterTest
	{
		public void TestSetInvoiceHeaderAdditionalInfo()
		{
			ToolTest.TestSetInvoiceHeaderAdditionalInfo();
		}

		public void TestSetInvoiceLinesAdditionalInfo()
		{
			ToolTest.TestSetInvoiceLinesAdditionalInfo();
		}

		public void TestSetXmlInvoiceHeaderAdditionalInfo()
		{
			ToolTest.TestSetXmlInvoiceHeaderAdditionalInfo();
		}

		public void TestSetXmlInvoiceLinesAdditionalInfo()
		{
			ToolTest.TestSetXmlInvoiceLinesAdditionalInfo();
		}

		public void TestAUSetInvoiceLineSummary()
		{
			ToolTest.TestAUSetInvoiceLineSummary();
		}

		public void TestAUSetInvoiceLineDetail()
		{
			ToolTest.TestAUSetInvoiceLineDetail();
		}

		public void TestAUSetInvoiceLineDetail_Bond()
		{
			ToolTest.TestAUSetInvoiceLineDetail_Bond();
		}

		public void TestNewBusinessObject()
		{
			ToolTest.TestNewBusinessObject();
		}

		public void TestSetXmlInvoiceLineDetails_ClassInfo()
		{
			ToolTest.TestSetXmlInvoiceLineDetails_ClassInfo();
		}

		public void TestSetXmlInvoiceLineDetails_Bond()
		{
			ToolTest.TestSetXmlInvoiceLineDetails_Bond();
		}

		protected override IValueObjectDataAdapter GetInvoiceValueObjectDataAdapter()
		{
			return new AUStandAloneInvoiceValueObjectDataAdapter();
		}

		AUStandAloneInvoiceValueObjectDataAdapter InvoiceDataAdapter
		{
			get
			{
				return (AUStandAloneInvoiceValueObjectDataAdapter)invoiceDataAdapter;
			}
		}

		protected override BaseJobComInvoiceHeader NewBusinessObject()
		{
			JobComInvoiceHeader result = Factory.New<JobComInvoiceHeader>();
			result.JZ_IncoTerm = "FOB";
			result.JZ_MessageType = JobMessageTypeList.Codes.Import;
			new FakeDeclarationCreatorForInvoice(result);
			return result;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			string outputFileFullPath = FileReader.ExtractEmbeddedResourceToFile(TestFilesPath, TempDir.DirectoryName, "AUEmptyStandAloneInvoice.xml");
			return new BusinessObjectAndExpectedOutputFileName(GetEmptyInvoiceHeader(), outputFileFullPath, ValidationKind.None, "Empty Invoice");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			string outputFileFullPath = FileReader.ExtractEmbeddedResourceToFile(TestFilesPath, TempDir.DirectoryName, "AUPopulatedStandAloneInvoice.xml");
			return new BusinessObjectAndExpectedOutputFileName(GetInvoiceHeaderWithTestData(), outputFileFullPath, ValidationKind.Xsd | ValidationKind.FactorySave, "Fully Populated Invoice");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return System.Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected new BaseJobComInvoiceHeader GetEmptyInvoiceHeader()
		{
			JobComInvoiceHeader invoiceHeader = (JobComInvoiceHeader)NewBusinessObject();
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2005, 3, 14);
			return invoiceHeader;
		}

		protected override void AssertRightNumberOfAddCustomsDetailsAreGeneratedFromAnEmptyInvoiceHeader(int count)
		{
			AssertEquals("AddCustomsDetails.Count", 17, count);
		}

		protected override BaseJobComInvoiceHeader GetInvoiceHeaderWithTestData()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Supplier";
			organisation.OH_IsConsignee = true;
			organisation.OH_IsConsignor = true;
			organisation.OH_RL_NKClosestPort = "AUMEL";
			OrgAddress address = organisation.Addresses.MainAddress;
			address.OA_Address1 = "7J1B7RFZ4WSH3LJTGUG7P55D1ABG3TMYMKFCPHQ2VSK4UDXQF0";
			organisation.OH_Code = "L1FZUT4BYOD8";
			JobComInvoiceHeader invoiceHeader = (JobComInvoiceHeader)base.GetInvoiceHeaderWithTestData();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Enterprise.Core.Constants.CurrencyCodes.Australia;
			invoiceHeader.JZ_OH_Supplier = organisation.PK;
			invoiceHeader.JZ_InvoiceNumber = "INVOICENUMBER";
			invoiceHeader.JZ_InvoiceAmount = 57;
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(1998, 3, 4);
			invoiceHeader.JZ_Volume = 159;
			invoiceHeader.JZ_VolumeUQ = "Vo";
			invoiceHeader.JZ_Weight = 168;
			invoiceHeader.JZ_WeightUQ = "We";
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2005, 3, 14);
			invoiceHeader.JZ_IncoTerm = "Inc";
			invoiceHeader.AddInfo.ZA_ORG = "AU";
			invoiceHeader.AddInfo.ZA_VALB_Hidden = "TV";
			invoiceHeader.AddInfo.ZA_HeaderREL_Hidden = "N";
			invoiceHeader.JZ_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = invoiceHeader.InvoiceLines.Cast<BaseJobComInvoiceLine>().First();
			invoiceLine.JI_CustomsUnitQty = "NR";
			return invoiceHeader;
		}

		protected override BaseJobComInvoiceHeader GetInvoiceHeader()
		{
			BaseJobComInvoiceHeader result = base.GetInvoiceHeader();
			result.JZ_MessageType = JobMessageTypeList.Codes.Import;
			return result;
		}

		protected override BaseCusClassification CreateTestCusClassification()
		{
			BaseCusClassification result = base.CreateTestCusClassification();
			result.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			result.CC_TariffNum = TariffNumber;
			return result;
		}

		protected override ZString TariffNumber
		{
			get
			{
				return "9999.99.99 99";
			}
		}

		AUInvoiceDataTransferToolTest ToolTest
		{
			get
			{
				return fToolTest ?? (fToolTest = new AUInvoiceDataTransferToolTest(InvoiceDataAdapter, GetInvoiceHeader, Factory));
			}
		}
		AUInvoiceDataTransferToolTest fToolTest;

		TestFileReader FileReader => fileReader ?? (fileReader = new TestFileReader(typeof(AUStandAloneInvoiceValueObjectDataAdapterTest)));
		TestFileReader fileReader;

		TempDirectory TempDir => tempDir ?? (tempDir = new TempDirectory());
		TempDirectory tempDir;

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "9999999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NR");
			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			tempDir?.Dispose();
		}

		const string TestFilesPath = "Enterprise.Customs.AU.Declaration.Business.Testing.Data.Xml.TestFiles";
	}
}
