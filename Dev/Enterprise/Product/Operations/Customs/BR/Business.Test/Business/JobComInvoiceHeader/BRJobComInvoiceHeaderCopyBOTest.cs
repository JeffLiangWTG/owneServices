using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(BRJobComInvoiceHeaderCopyBO))]
	class BRJobComInvoiceHeaderCopyBOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLinkInvoiceLines()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoice1 = declaration1.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "Inv1";
			invoice1.JZ_InvoiceAmount = 1000;

			var line1 = invoice1.InvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			line1.JI_InvoiceQuantity = 10;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var copiedInvoice = declaration2.Invoices.AddNew();

			var copyBo = new BRJobComInvoiceHeaderCopyBO(invoice1, copiedInvoice, null);
			copyBo.CopyInvoice();

			var copiedInvoiceLine1 = copiedInvoice.InvoiceLines[0];

			AssertEquals("JI_ParentTableCode should be", JobComInvoiceLineSchema.Constants.Prefix, copiedInvoiceLine1.JI_ParentTableCode);
			AssertEquals("JI_ParentID should be", line1.PK, copiedInvoiceLine1.JI_ParentID);
		}

		public void TestGetInvoiceLinesToCopyOnlyLinesRequireLicense()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "Inv1";
			invoice.JZ_InvoiceAmount = 1000;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LineNo = 1;
			invoiceLine1.JI_InvoiceQuantity = 10;
			invoiceLine1.JI_RequiresImportLicense = true;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_InvoiceQuantity = 20;
			invoiceLine2.JI_RequiresImportLicense = false;

			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_LineNo = 3;
			invoiceLine3.JI_InvoiceQuantity = 30;
			invoiceLine3.JI_RequiresImportLicense = true;

			var cDeclaration = Factory.New<JobDeclaration>();
			cDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var cInvoice = cDeclaration.Invoices.AddNew();

			var option = new JobComInvoiceHeaderCopyOptions();
			option.AllLines = false;
			option.OnlyLinesRequireLicense = true;
			new BRJobComInvoiceHeaderCopyBO(invoice, cInvoice, option).CopyInvoice();

			AssertEquals("Should contain two InvoiceLine", 2, cInvoice.InvoiceLines.Count);

			var cInvoiceLine1 = cInvoice.InvoiceLines[0];
			var cInvoiceLine2 = cInvoice.InvoiceLines[1];

			CombineAssertions(() =>
			{
				AssertEquals("Copied InvoiceLines 1 JI_ParentTableCode should be", JobComInvoiceLineSchema.Constants.Prefix, cInvoiceLine1.JI_ParentTableCode);
				AssertEquals("Copied InvoiceLines 1 JI_ParentID should be", invoiceLine1.PK, cInvoiceLine1.JI_ParentID);
				AssertEquals("Copied InvoiceLines 2 JI_ParentTableCode should be", JobComInvoiceLineSchema.Constants.Prefix, cInvoiceLine2.JI_ParentTableCode);
				AssertEquals("Copied InvoiceLines 2 JI_ParentID should be", invoiceLine3.PK, cInvoiceLine2.JI_ParentID);
			});
		}

		public void TestGetInvoiceLinesToCopyAllLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "Inv1";
			invoice.JZ_InvoiceAmount = 1000;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LineNo = 1;
			invoiceLine1.JI_InvoiceQuantity = 10;
			invoiceLine1.JI_RequiresImportLicense = true;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_InvoiceQuantity = 20;
			invoiceLine2.JI_RequiresImportLicense = false;

			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_LineNo = 3;
			invoiceLine3.JI_InvoiceQuantity = 30;
			invoiceLine3.JI_RequiresImportLicense = true;

			var cDeclaration = Factory.New<JobDeclaration>();
			cDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var cInvoice = cDeclaration.Invoices.AddNew();

			var option = new JobComInvoiceHeaderCopyOptions();
			option.AllLines = true;
			option.OnlyLinesRequireLicense = false;
			new BRJobComInvoiceHeaderCopyBO(invoice, cInvoice, option).CopyInvoice();

			AssertEquals("Should contain two InvoiceLine", 3, cInvoice.InvoiceLines.Count);

			var cInvoiceLine1 = cInvoice.InvoiceLines[0];
			var cInvoiceLine2 = cInvoice.InvoiceLines[1];
			var cInvoiceLine3 = cInvoice.InvoiceLines[2];

			CombineAssertions(() =>
			{
				AssertEquals("Copied InvoiceLines 1 JI_ParentTableCode should be", JobComInvoiceLineSchema.Constants.Prefix, cInvoiceLine1.JI_ParentTableCode);
				AssertEquals("Copied InvoiceLines 1 JI_ParentID should be", invoiceLine1.PK, cInvoiceLine1.JI_ParentID);
				AssertEquals("Copied InvoiceLines 2 JI_ParentTableCode should be", JobComInvoiceLineSchema.Constants.Prefix, cInvoiceLine2.JI_ParentTableCode);
				AssertEquals("Copied InvoiceLines 2 JI_ParentID should be", invoiceLine2.PK, cInvoiceLine2.JI_ParentID);
				AssertEquals("Copied InvoiceLines 3 JI_ParentTableCode should be", JobComInvoiceLineSchema.Constants.Prefix, cInvoiceLine3.JI_ParentTableCode);
				AssertEquals("Copied InvoiceLines 3 JI_ParentID should be", invoiceLine3.PK, cInvoiceLine3.JI_ParentID);
			});
		}

		public void TestCopiedColumns()
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			var manufacturerAddress = manufacturer.Addresses.AddNew();
			manufacturerAddress.Address1 = "Manufacture Address";

			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "Inv1";
			invoice.JZ_InvoiceAmount = 1000;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_InvoiceQuantity = 10;
			invoiceLine.JI_Tariff = "11111111";
			invoiceLine.NaladiHs = "12345678";
			invoiceLine.JI_Weight = 15.123m;
			invoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
			invoiceLine.JI_OA_ManufacturerAddress = manufacturerAddress.PK;

			invoiceLine.DutyTaxRegime = "1";
			invoiceLine.DutyLegalBase = "4";
			var additionalTariffAgreement = invoiceLine.AdditionalTariffs.AddNew();
			additionalTariffAgreement.LegalActSubject = AdditionalTaxTypeList.Codes.TariffAgreement;
			additionalTariffAgreement.TariffType = "BO36";
			additionalTariffAgreement.TariffCode = "122";

			var additionalExDutyTariff = invoiceLine.AdditionalTariffs.AddNew();
			additionalExDutyTariff.LegalActSubject = AdditionalTaxTypeList.Codes.ExDutyTariff;
			additionalExDutyTariff.TariffType = ChildTariffTypeList.Codes.LEBIT;
			additionalExDutyTariff.TariffCode = "123";

			invoiceLine.TariffDetachs.AddNew("333");
			invoiceLine.TariffDetachs.AddNew("555");
			invoiceLine.NVECusCodeDataCollection[0].CY_Data = "0001";
			invoiceLine.NVECusCodeDataCollection[1].CY_Data = "0002";

			var cDeclaration = Factory.New<JobDeclaration>();
			cDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var cInvoice = cDeclaration.Invoices.AddNew();

			var option = new JobComInvoiceHeaderCopyOptions();
			option.AllLines = true;
			option.OnlyLinesRequireLicense = false;

			var copyBo = new BRJobComInvoiceHeaderCopyBO(invoice, cInvoice, option);
			copyBo.CopyInvoice();

			var cInvoiceLine = cInvoice.InvoiceLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("NaladiHs should be copied", "12345678", cInvoiceLine.NaladiHs);
				AssertEquals("JI_Weight should be copied", 15.123m, cInvoiceLine.JI_Weight);
				AssertEquals("JI_ManufacturerIndicator should be copied", ManufacturerIndicatorList.Codes._2, cInvoiceLine.JI_ManufacturerIndicator);
				AssertEquals("JI_OA_ManufacturerAddress should be copied", manufacturerAddress.PK, cInvoiceLine.ManufacturerDocAddressPK);
				AssertEquals("DutyTaxRegime should be copied", "1", cInvoiceLine.DutyTaxRegime);
				AssertEquals("DutyLegalBase should be copied", "4", cInvoiceLine.DutyLegalBase);
				AssertEquals("JI_SecondaryPreference should be", "BO36", cInvoiceLine.JI_SecondaryPreference);
				AssertEquals("TariffDetachCollection[0] should be copied", "333", cInvoiceLine.TariffDetachs[0].CY_Code);
				AssertEquals("TariffDetachCollection[1] should be copied", "555", cInvoiceLine.TariffDetachs[1].CY_Code);
				AssertEquals("NVECusCodeDataCollection[0] should be copied", "0001", cInvoiceLine.NVECusCodeDataCollection[0].CY_Data);
				AssertEquals("NVECusCodeDataCollection[1] should be copied", "0002", cInvoiceLine.NVECusCodeDataCollection[1].CY_Data);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var source = declaration1.Invoices.AddNew();

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var destination = declaration2.Invoices.AddNew();
			return new BRJobComInvoiceHeaderCopyBO(source, destination, null);
		}
	}
}
