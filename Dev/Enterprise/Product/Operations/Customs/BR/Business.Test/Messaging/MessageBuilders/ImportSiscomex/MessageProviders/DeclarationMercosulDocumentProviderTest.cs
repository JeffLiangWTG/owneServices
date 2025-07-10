using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationMercosulDocumentProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationMercosulDocumentProvider()
		{
			var invLine = Factory.New<JobComInvoiceLine>();
			var oMercosul = invLine.MercosulForeignDeclarations.AddNew();
			oMercosul.CSI_SubType = CertificateTypeList.Codes.CCPTC;
			oMercosul.CSI_Description = "000000000";
			oMercosul.CSI_ReferenceNumber = "1";
			oMercosul.CSI_ReferenceNumber2 = "100";
			oMercosul.CSI_RN_NKCountryCode = MercosulCountriesList.Codes.AR;
			oMercosul.CSI_Code = "02654";
			oMercosul.CSI_ItemNumber = 1;
			oMercosul.CSI_Quantity3 = 10m;

			var mercosulDocProvider = new DeclarationMercosulDocumentProvider(oMercosul);

			AssertEquals("ReferenceType", CertificateTypeList.Codes.CCPTC, mercosulDocProvider.ReferenceType);
			AssertEquals("ReferenceNumber", "000000000", mercosulDocProvider.ReferenceNumber);
			AssertEquals("QtyStart", "1", mercosulDocProvider.QtyStart);
			AssertEquals("QtyEnd", "100", mercosulDocProvider.QtyEnd);
			AssertEquals("CertificateNumber", "02654", mercosulDocProvider.CertificateNumber);
			AssertEquals("CountryCode", MercosulCountriesList.Codes.AR, mercosulDocProvider.CountryCode);
			AssertEquals("ItemNumber", (ZShort)1, mercosulDocProvider.ItemNumber);
			AssertEquals("QtyUnitStatistic", 10m, mercosulDocProvider.QtyUnitStatistic);
		}

		public void TestEquals()
		{
			var oDeclaration = Factory.New<JobDeclaration>();
			oDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			oDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var oInvoiceLine = oDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var mercosul1 = oInvoiceLine.MercosulForeignDeclarations.AddNew();
			mercosul1.CSI_SubType = CertificateTypeList.Codes.CCPTC;
			mercosul1.CSI_Description = "000000000";
			mercosul1.CSI_ReferenceNumber = "1";
			mercosul1.CSI_ReferenceNumber2 = "100";
			mercosul1.CSI_RN_NKCountryCode = MercosulCountriesList.Codes.AR;
			mercosul1.CSI_Code = "02654";
			mercosul1.CSI_ItemNumber = 1;
			mercosul1.CSI_Quantity3 = 10m;

			var mercosul2 = oInvoiceLine.MercosulForeignDeclarations.AddNew();
			var mercosulDoc1 = new DeclarationMercosulDocumentProvider(mercosul1);
			var mercosulDoc2 = new DeclarationMercosulDocumentProvider(mercosul2);

			Assert("Not Equals", !mercosulDoc1.Equals(null));
			Assert("Not Equals", !mercosulDoc1.Equals(mercosul1));
			Assert("Not Equals", !mercosulDoc1.Equals(mercosulDoc2));

			mercosul2.CSI_SubType = CertificateTypeList.Codes.CCPTC;
			Assert("Not Equals", !mercosulDoc1.Equals(mercosulDoc2));

			mercosul2.CSI_Description = "000000000";
			mercosul2.CSI_ReferenceNumber = "1";
			mercosul2.CSI_ReferenceNumber2 = "100";
			mercosul2.CSI_RN_NKCountryCode = MercosulCountriesList.Codes.AR;
			mercosul2.CSI_Code = "02654";
			mercosul2.CSI_ItemNumber = 1;
			mercosul2.CSI_Quantity3 = 10m;
			Assert("Equals", mercosulDoc1.Equals(mercosulDoc2));
		}
	}
}



