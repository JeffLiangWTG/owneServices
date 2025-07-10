using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusEntryLineIEDIInvoiceLineOGDTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			//Min
			AssertEquals("TariffNumber", invoiceLine.JI_Tariff, entryLine.TariffNumber);
			AssertEquals("Quantity", invoiceLine.JI_InvoiceQuantity, entryLine.Quantity);
			AssertEquals("QuantityUnits", invoiceLine.JI_InvoiceUQ, entryLine.QuantityUnits);
			AssertEquals("CountryOfOrigin", invoiceLine.JI_CountryOfOrigin, entryLine.CountryOfOrigin);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_StateOrRegionOfOrigin = invoiceLine.AddInfoLookups.StatesOfOrigin[0].Code;
			AssertEquals("CountryOfOrigin is US", (ZString)("U" + invoiceLine.JI_StateOrRegionOfOrigin), entryLine.CountryOfOrigin);
			AssertEquals("ItemDescription", invoiceLine.JI_Description, entryLine.ItemDescription);
			//AQ
			AssertEquals("PageNumber", 1, entryLine.PageNumber);
			AssertEquals("LineNumber", 1, entryLine.LineNumber);
			AssertEquals("UnitPrice", (ZDecimal)0, entryLine.UnitPrice);
			AssertEquals("LinePrice", invoiceLine.JI_LinePrice, entryLine.LinePrice);
			AssertEquals("LinePriceCurrency", invoiceLine.LinePriceRefCurrency.RX_Code, entryLine.LinePriceCurrency);
			//OGD
			AssertEquals("ImportReasonCode", invoiceLine.CA_ImportReasonCode, entryLine.ImportReasonCode);
			AssertEquals("RegistrationNumbers count", 2, entryLine.RegistrationNumbers.Length);
			AssertEquals("CFIA Registration Number", invoiceLine.CFIARegistrationNumbers[0].CY_Data, entryLine.RegistrationNumbers[0]);
			AssertEquals("SITT Registration Number", invoiceLine.SITTCertificationNumbers[0].CY_Data, entryLine.RegistrationNumbers[1]);
			AssertEquals("RegistrationTypes count", 2, entryLine.RegistrationTypes.Length);
			AssertEquals("CFIA Registration Type", invoiceLine.CFIARegistrationNumbers[0].CY_Code, entryLine.RegistrationTypes[0]);
			AssertEquals("SITT Registration Type", invoiceLine.SITTCertificationNumbers[0].CY_Code, entryLine.RegistrationTypes[1]);
			AssertEquals("TIIN", invoiceLine.CA_TIIN, entryLine.TIIN);
			AssertEquals("CompliantCompletionIndicator", invoiceLine.CA_CompliantCompletion, entryLine.CompliantCompletionIndicator);
			AssertEquals("CompliantImportDateIndicator", invoiceLine.CA_CompliantImportDate, entryLine.CompliantImportDateIndicator);
			AssertEquals("Model", invoiceLine.CA_Model, entryLine.Model);
			AssertEquals("ModelNumber", invoiceLine.CA_ModelNumber, entryLine.ModelNumber);
			AssertEquals("BrandName", invoiceLine.JI_BrandName, entryLine.BrandName);
			AssertEquals("TypeSize", invoiceLine.CA_TypeSize, entryLine.TypeSize);
			AssertEquals("RequirementID", invoiceLine.CA_RequirementID, entryLine.RequirementID);
			AssertEquals("RequirementVersion", invoiceLine.CA_RequirementVer, entryLine.RequirementVersion);
			AssertEquals("AirsCode", invoiceLine.CA_AirsCode, entryLine.AirsCode);
			AssertEquals("DestinationProvince", invoiceLine.CA_DestinationProvince, entryLine.DestinationProvince);
			AssertEquals("EndUse", invoiceLine.CA_EndUse, entryLine.EndUse);
			AssertEquals("MiscID", invoiceLine.CA_MiscID, entryLine.MiscID);
			AssertEquals("CFIAOrigin is US", (ZString)("U" + invoiceLine.CA_CFIAUSStateOfOrigin), entryLine.CFIAOrigin);
			invoiceLine.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.Zimbabwe;
			AssertEquals("CFIAOrigin", invoiceLine.CA_RN_NKCFIAOrigin, entryLine.CFIAOrigin);
			AssertEquals("Make", string.Empty, entryLine.Make);
			AssertEquals("VehicleClass", string.Empty, entryLine.VehicleClass);
			AssertEquals("VIN", 0, entryLine.VIN.Length);
			AssertEquals("AssemblyMonth", 0, entryLine.AssemblyMonth.Length);
		}

		public void TestQuantityAndUnits()
		{
			invoiceLine.JI_InvoiceQuantity = 0;
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			declaration.DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].AllEntryLines[0];
			AssertEquals("Quantity", invoiceLine.JI_CustomsQuantity, entryLine.Quantity);
			AssertEquals("QuantityUnits", invoiceLine.JI_CustomsUnitQty, entryLine.QuantityUnits);

			invoiceLine.JI_InvoiceQuantity = 30;
			invoiceLine.JI_InvoiceUQ = "LB";
			declaration.DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].AllEntryLines[0];
			AssertEquals("Quantity", invoiceLine.JI_CustomsQuantity, entryLine.Quantity);
			AssertEquals("QuantityUnits", invoiceLine.JI_CustomsUnitQty, entryLine.QuantityUnits);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates).RX_Code;
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			//Min
			invoiceLine.JI_Tariff = "0301100000";
			invoiceLine.JI_CustomsQuantity = 20;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_InvoiceQuantity = 10;
			invoiceLine.JI_InvoiceUQ = "NMB";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine.JI_Description = "A LONG GOODS DESCRIPTION WHICH EXCEEDS 35 CHARACTERS";
			//AQ
			invoiceLine.JI_LineNo = 1;
			invoiceLine.CA_PageNumber = 1;
			invoiceLine.JI_LinePrice = 100m;
			//OGD
			invoiceLine.CA_ImportReasonCode = ImportReasonCodes.Codes.Export;
			invoiceLine.SITTCertificationNumbers.AddNew("SITT12345");
			invoiceLine.CFIARegistrationNumbers.AddNew("COD", "CFIA12345");
			invoiceLine.CA_TIIN = "TIN";
			invoiceLine.CA_CompliantCompletion = true;
			invoiceLine.CA_CompliantImportDate = true;
			invoiceLine.CA_Model = "DESIGN/STYLE/STRUCTURE";
			invoiceLine.CA_ModelNumber = "MODEL ID 12355";
			invoiceLine.JI_BrandName = "MODEL BRAND";
			invoiceLine.CA_TypeSize = "HP/RPM";
			invoiceLine.CA_RequirementID = "AIRS1234";
			invoiceLine.CA_RequirementVer = "2";
			invoiceLine.CA_AirsCode = "AIRS12";
			invoiceLine.CA_DestinationProvince = invoiceLine.AddInfoLookups.CanadianProvinces[0].Code;
			invoiceLine.CA_EndUse = "END";
			invoiceLine.CA_MiscID = "MID";
			invoiceLine.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.CA_CFIAUSStateOfOrigin = invoiceLine.AddInfoLookups.CFIAStatesOfOrigin[0].Code;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].AllEntryLines[0];
			((CusEntryLine)entryLine).LineNumber = 1;
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		IEDIInvoiceLineOGD entryLine;
	}
}
