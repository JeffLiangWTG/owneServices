using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportLicenseLoadingObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInvoiceHeaderList()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.PrimaryRegistrationNumber.Number = "08264406000193";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.FOB;

			var messageBuilder = new ZStringBuilder();
			var importLicenseLoading = new ImportLicenseLoadingObjectParent(declaration);
			importLicenseLoading.AddLog = (completedCount, totalCount, messageText) => messageBuilder.Append(messageText);
			using (var stream = GenerateStreamFromString(ImportLicenseLoadingObjectParentTest.RightXML))
			{
				importLicenseLoading.LoadAndValidateXML("RightXML.xml", stream);

				var importLicenseLoadingObject = importLicenseLoading.Collection.FirstOrDefault() as ImportLicenseLoadingObject;
				var lookups = new ImportLicenseLoadingObjectLookups(importLicenseLoadingObject);
				AssertEquals(lookups.InvoiceHeaderList, importLicenseLoadingObject.Declaration.Invoices);
			}
		}

		public void TestChargeTypeList()
		{
			ReferenceTestDataHelper.CreateReferenceDataForILFFeeTypeList(Factory);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var importLicenseObject = new ImportLicenseLoadingObject(new ImportLicenseLoadingObjectParent(declaration));
			var importLicenseObjectLookups = new ImportLicenseLoadingObjectLookups(importLicenseObject);
			AssertContainsExactElementsInAnyOrder(new string[] { "1", "2" }, importLicenseObjectLookups.ImportLicenseTypeList.GetAllCodes());
			AssertContainsExactElementsInAnyOrder(new string[] { "F1ND", "F1D5" }, importLicenseObjectLookups.ImportLicenseFeeTypeList.GetAllCodes());
		}

		Stream GenerateStreamFromString(string s)
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(s);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}
	}
}
