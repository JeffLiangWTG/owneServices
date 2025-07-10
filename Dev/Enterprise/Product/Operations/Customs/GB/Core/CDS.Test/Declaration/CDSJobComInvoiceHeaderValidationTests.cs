using System;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.CDS.Declaration;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class CDSJobComInvoiceHeaderValidationTests : Business.Testing.JobComInvoiceHeaderValidationTEST
	{
		protected override Type GetTypeForTest()
		{
			return typeof(CDSJobComInvoiceHeaderValidation);
		}
		protected override Customs.Business.BaseJobComInvoiceHeader GetInvoiceHeader()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			return dec.Invoices.AddNew();
		}

		public void TestValidateINCOTermAndAgreedPlace()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var ceiA = declaration.CustomsEntryInstructions.AddNew();
			var ceiB = declaration.CustomsEntryInstructions.AddNew();
			var line11 = invoice1.InvoiceLines.AddNew();
			var line12 = invoice1.InvoiceLines.AddNew();
			var line21 = invoice2.InvoiceLines.AddNew();
			var line22 = invoice2.InvoiceLines.AddNew();

			// Both invoices invovled in both CEIs 
			line11.JI_CEI = ceiA.PK;
			line12.JI_CEI = ceiB.PK;
			line21.JI_CEI = ceiA.PK;
			line22.JI_CEI = ceiB.PK;
			invoice1.JZ_IncoTerm = "AAA";
			invoice1.JZ_IncoTermPlace = "PLACEA";
			invoice2.JZ_IncoTerm = "AAA";
			invoice2.JZ_IncoTermPlace = "PLACEB";
			AssertHasMessageErrorContaining(invoice1.JZ_IncoTermInfo, "same Incoterm and place");
			AssertHasMessageErrorContaining(invoice2.JZ_IncoTermInfo, "same Incoterm and place");

			// Each invoice on only one CEI - different INCO is OK
			line11.JI_CEI = ceiA.PK;
			line12.JI_CEI = ceiA.PK;
			line21.JI_CEI = ceiB.PK;
			line22.JI_CEI = ceiB.PK;
			invoice1.JZ_IncoTerm = "BBB";
			invoice1.JZ_IncoTermPlace = "PLACEA";
			invoice2.JZ_IncoTerm = "BBB";
			invoice2.JZ_IncoTermPlace = "PLACEB";
			AssertNoMessageErrorContaining(invoice1.JZ_IncoTermInfo, "same Incoterm and place");
			AssertNoMessageErrorContaining(invoice2.JZ_IncoTermInfo, "same Incoterm and place");

			// All on one CEI, no mixed terms allowed
			line11.JI_CEI = ceiA.PK;
			line12.JI_CEI = ceiA.PK;
			line21.JI_CEI = ceiA.PK;
			line22.JI_CEI = ceiA.PK;
			invoice1.JZ_IncoTerm = "CCC";
			invoice1.JZ_IncoTermPlace = "PLACEA";
			invoice2.JZ_IncoTerm = "CCC";
			invoice2.JZ_IncoTermPlace = "PLACEB";
			AssertHasMessageErrorContaining(invoice1.JZ_IncoTermInfo, "same Incoterm and place");
			AssertHasMessageErrorContaining(invoice2.JZ_IncoTermInfo, "same Incoterm and place");
		}

		public void TestValidateIncoTermPlaceLocation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTermPlace = "GBLondon";
			AssertNoMessageErrors(invoice.JZ_IncoTermPlaceInfo);
			invoice.JZ_IncoTermPlace = "GBLHR";
			AssertNoMessageErrors(invoice.JZ_IncoTermPlaceInfo);
			invoice.JZ_IncoTermPlace = "IESantry";
			AssertHasWarningContaining(invoice.JZ_IncoTermPlaceInfo, "When not using a UNLOCO, the location must be given with a country/region code");
			invoice.JZ_IncoTermPlace = "DWAnkhmorpork";
			AssertHasMessageErrorContaining(invoice.JZ_IncoTermPlaceInfo, "Use UNLOCO or country/region code prefix");
		}

		public void TestValidatePreCIFIncoTerm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTermPlace = "GBLHR";
			invoice.JZ_IncoTerm = "EXW";
			AssertHasMessageErrorContaining(invoice.JZ_IncoTermInfo, "Pre-CIF");
			invoice.JZ_IncoTerm = "CIF";
			AssertNoMessageErrorContaining(invoice.JZ_IncoTermInfo, "Pre-CIF");

			var chargeA = invoice.Charges.AddNew();
			chargeA.J7_ChargeType = "ONS";
			chargeA.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			chargeA.J7_Amount = 10m;
			invoice.JZ_IncoTerm = "FOB";
			AssertNoMessageErrorContaining(invoice.JZ_IncoTermInfo, "Pre-CIF");
			chargeA.Delete();

			var chargeB = invoice.GroupCharges.AddNew();
			chargeB.J7_ChargeType = "ONS";
			chargeB.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			chargeB.J7_Amount = 10m;
			AssertNoMessageErrorContaining(invoice.JZ_IncoTermInfo, "Pre-CIF");
		}

		public void TestValidateWarningSuppressionForCDSExports()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTermPlace = "GBLHR";
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			AssertNoMessageErrorContaining(invoice.JZ_IncoTermInfo, "Pre-CIF");
			invoice.Delete();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_IncoTermPlace = "GBLHR";
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			AssertHasMessageErrorContaining(invoice2.JZ_IncoTermInfo, "Pre-CIF");
		}

		public void TestValidateWarningNotSuppressedForCDSImports()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTermPlace = "GBLHR";
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			AssertHasMessageErrorContaining(invoice.JZ_IncoTermInfo, "Pre-CIF");
		}
	}
}
