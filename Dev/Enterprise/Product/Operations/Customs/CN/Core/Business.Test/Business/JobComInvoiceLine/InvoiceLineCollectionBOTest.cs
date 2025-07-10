using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(InvoiceLineViewCollection))]
	class InvoiceLineCollectionBOTest : Customs.Business.Testing.InvoiceLineCollectionBOTest<InvoiceLineViewCollection>
	{
		public void TestCopyLastLineDetailsToNewLines()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CNPTA", "CN Prefential Trade Agreement");
			var cnpta_01 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CNPTA", "XX", "CNPTA XX", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(cnpta_01.PK, Constants.UniversalReferenceConstants.CusCodeListAttributeName.ApplicableCountry, Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.FilteredInvoiceLines.CopyLastLineDetailsToNewLines = true;
			var header = declaration.Invoices.AddNew();
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.JI_DutyMode = "T";
			invoiceLine.JI_PartNo = "Part No";
			invoiceLine.JI_ProductVersion = "P version";
			invoiceLine.JI_CountryOfOrigin = "DE";
			invoiceLine.JI_RN_NKCountryOfExport = "GB";
			invoiceLine.JI_DestinationDistrict = "11111";
			invoiceLine.JI_OriginDistrict = "22222";
			invoiceLine.JI_DestinationRegion = "111111";
			invoiceLine.JI_OriginRegion = "222222";
			invoiceLine.JI_PrimaryPreference = "FTA";
			invoiceLine.JI_SecondaryPreference = "XX";
			invoiceLine.CertificateOfOrigin = "COO";
			invoiceLine.CertificateOfOriginCountry = "US";
			invoiceLine.CertificateOfOriginType = "C";
			invoiceLine.ItemNoOnCertOfOrigin = 1;
			var invoiceLine2 = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals("DutyMode should be defaulted", invoiceLine.JI_DutyMode, invoiceLine2.JI_DutyMode);
			AssertEquals("PartNo should be defaulted", invoiceLine.JI_PartNo, invoiceLine2.JI_PartNo);
			AssertEquals("ProductVersion should be defaulted", invoiceLine.JI_ProductVersion, invoiceLine2.JI_ProductVersion);
			AssertEquals("CountryOfOrigin should be defaulted", invoiceLine.JI_CountryOfOrigin, invoiceLine2.JI_CountryOfOrigin);
			AssertEquals("CountryOfExport should be defaulted", invoiceLine.JI_RN_NKCountryOfExport, invoiceLine2.JI_RN_NKCountryOfExport);
			AssertEquals("Destination District should be defaulted", invoiceLine.JI_DestinationDistrict, invoiceLine2.JI_DestinationDistrict);
			AssertEquals("Origin District should be defaulted", invoiceLine.JI_OriginDistrict, invoiceLine2.JI_OriginDistrict);
			AssertEquals("Destination Region should be defaulted", invoiceLine.JI_DestinationRegion, invoiceLine2.JI_DestinationRegion);
			AssertEquals("Origin Region should be defaulted", invoiceLine.JI_OriginRegion, invoiceLine2.JI_OriginRegion);

			AssertEquals("PrimaryPreferece", "FTA", invoiceLine2.JI_PrimaryPreference);
			AssertEquals("SecondaryPreference", "XX", invoiceLine2.JI_SecondaryPreference);
			AssertEquals("TradeAgreementCode", "XX", invoiceLine2.TradeAgreementCode);
			AssertEquals("COO", "COO", invoiceLine2.CertificateOfOrigin);
			AssertEquals("CertificateOfOriginCountry", "US", invoiceLine2.CertificateOfOriginCountry);
			AssertEquals("CertificateOfOriginType", "C", invoiceLine2.CertificateOfOriginType);
			AssertEquals("ItemNoOnCertOfOrigin", new ZShort(1), invoiceLine2.ItemNoOnCertOfOrigin);

			AssertEquals("documents", 1, invoiceLine2.CusSupportingDocuments.Count);
			var newDoc = invoiceLine2.CusSupportingDocuments.OfType<CusSupportingInfo>().FirstOrDefault();
			AssertEquals("documents CSI_Code", "1Y", newDoc.CSI_Code);
			AssertEquals("documents CSI_ReferenceNumber", "COO", newDoc.CSI_ReferenceNumber);
			AssertEquals("documents CSI_RN_NKCountryCode", "US", newDoc.CSI_RN_NKCountryCode);
			AssertEquals("documents CSI_SubType", "C", newDoc.CSI_SubType);
			AssertEquals("documents CSI_LineNo", new ZShort(1), newDoc.CSI_LineNo);
		}

		public void TestTypedIndexer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var collection = new InvoiceLineViewCollection(declaration);
			var invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
			AssertType<JobComInvoiceLine>(invoiceLine);
			AssertType<JobComInvoiceLine>(collection[0]);
		}

		protected override InvoiceLineViewCollection GetCollectionToTest()
		{
			return new InvoiceLineViewCollection(JobDeclaration);
		}

		protected new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		protected new JobComInvoiceHeader Invoice => (JobComInvoiceHeader)base.Invoice;
	}
}
