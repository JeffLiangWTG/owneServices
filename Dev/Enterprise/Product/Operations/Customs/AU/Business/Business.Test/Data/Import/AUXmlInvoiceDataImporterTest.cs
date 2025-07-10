using System.Xml;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUXmlInvoiceDataImporterTest : XmlInvoiceDataImporterTest
	{
		public void TestOriginIsDefaultedFromProductIfNoneSpecified()
		{
			var org = GetNewSupplierOrg();
			GetNewPart("PART", org);
			Factory.Save();

			var jobDec = Factory.New<JobDeclaration>();
			jobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			jobDec.JE_OH_Supplier = org.PK;

			string xmlForImport =
				"<Invoices>" +
				"<InvoiceHeader>" +
				"<InvoiceNumber>5008446</InvoiceNumber>" +
				"<InvoiceLines>" +
				"<InvoiceLine>" +
				"<ProductNumber>PART</ProductNumber>" +
				"</InvoiceLine>" +
				"</InvoiceLines>" +
				"</InvoiceHeader>" +
				"</Invoices>";

			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xmlForImport);
			var importer = new XmlInvoiceDataImporter(xmlDoc, jobDec);
			importer.Import();
			var invLine = jobDec.Invoices[0].JobComInvoiceLines[0];
			AssertEquals("PART", invLine.JI_PartNo);
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, invLine.JI_CountryOfOrigin);
		}

		OrgHeader GetNewSupplierOrg()
		{
			var result = OrgHeader.New(Factory);
			result.OH_Code = "TESTORG";
			result.OH_IsConsignor = true;
			return result;
		}

		AUOrgSupplierPart GetNewPart(string partNum, OrgHeader supplier)
		{
			var result = Factory.New<AUOrgSupplierPart>();
			result.OP_PartNum = partNum;

			var aUClass = Factory.New<Classification>();
			aUClass.CC_LookupCode = "TestLookup";
			aUClass.CC_TariffNum = "TEST";
			aUClass.CC_Description = "TEST";
			aUClass.AddInfo.ZA_ORG = Core.Constants.CountryCodes.SouthAfrica;
			aUClass.CC_ClassificationType = Common.ClassificationType.IMP;

			result.AddNewImportPivotWithClassification(aUClass.PK).AddInfo.ZA_ORG = Core.Constants.CountryCodes.SouthAfrica;
			result.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			return result;
		}
	}
}
