using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(NFEImportObject))]
	class NFEImportObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultExchangeRate()
		{
			var currency = Factory.New<RefCurrency>();
			currency.RX_Code = "XYZ";
			RefExchangeRate buyRate = currency.ExchangeRates.AddNew();
			buyRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;
			buyRate.RE_StartDate = new ZDateTime(2021, 05, 10, 00, 00, 00);
			buyRate.RE_ExpiryDate = new ZDateTime(2021, 05, 10, 23, 59, 59);
			buyRate.RE_SellRate = 0.6m;

			RefExchangeRate sellRate = currency.ExchangeRates.AddNew();
			sellRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			sellRate.RE_StartDate = new ZDateTime(2021, 05, 10, 00, 00, 00);
			sellRate.RE_ExpiryDate = new ZDateTime(2021, 05, 10, 23, 59, 59);
			sellRate.RE_SellRate = 0.8m;

			Factory.Save();

			var parent = new NFEImportObjectParent(Factory.New<JobDeclaration>());

			var nfeImportObject = parent.NFEImportObjectCollection.AddNew();
			nfeImportObject.NfeDate = new ZDateTime(2021, 5, 20);
			AssertEquals("Currency Code is empty", ZString.Empty, nfeImportObject.CurrencyCode);

			nfeImportObject.CurrencyCode = "XXX";
			AssertEquals("Currency Code is XXX", "XXX", nfeImportObject.CurrencyCode);
			AssertEquals("Default Exchange Rate not found", 0m, nfeImportObject.ExchangeRate);
			AssertEquals("Default Exchange Rate Date", ZDateTime.Empty, nfeImportObject.ExchangeRateDate);
			AssertEquals("Default Exchange Rate Buy not found", 0m, nfeImportObject.ExchangeRateBuy);
			AssertEquals("Default Exchange Rate Sell", 0m, nfeImportObject.ExchangeRateSell);

			nfeImportObject.CurrencyCode = Core.Constants.CurrencyCodes.Brazil;
			AssertEquals("Currency Code is Brazil", Core.Constants.CurrencyCodes.Brazil, nfeImportObject.CurrencyCode);
			AssertEquals("Default Exchange Rate found", 1m, nfeImportObject.ExchangeRate);
			AssertEquals("Default Exchange Rate Date found", new ZDateTime(2021, 5, 20), nfeImportObject.ExchangeRateDate);
			AssertEquals("Default Exchange Rate Buy found", 1m, nfeImportObject.ExchangeRateBuy);
			AssertEquals("Default Exchange Rate Sell found", 1m, nfeImportObject.ExchangeRateSell);

			nfeImportObject.CurrencyCode = "XYZ";
			AssertEquals("Currency Code is Brazil", "XYZ", nfeImportObject.CurrencyCode);
			AssertEquals("Default Exchange Rate found", 0.6m, nfeImportObject.ExchangeRate);
			AssertEquals("Default Exchange Rate Date found", new ZDateTime(2021, 05, 10), nfeImportObject.ExchangeRateDate);
			AssertEquals("Default Exchange Rate Buy found", 0.6m, nfeImportObject.ExchangeRateBuy);
			AssertEquals("Default Exchange Rate Sell found", 0.8m, nfeImportObject.ExchangeRateSell);
		}

		public void TestDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.FOB;
			invoiceHeader.JZ_InvoiceNumber = "34234";

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;

			var nfeImportObject = new NFEImportObject(Factory);
			nfeImportObject.Declaration = declaration;

			AssertEquals("ExchangeRate", 0m, nfeImportObject.ExchangeRate);
			AssertEquals("Currency", Core.Constants.CurrencyCodes.Brazil, nfeImportObject.CurrencyCode);
			AssertEquals("Incoterm", BRIncoTermList.Codes.FOB, nfeImportObject.Incoterm);
			AssertEquals("EntryInstructionPK should be", entryInstruction.PK, nfeImportObject.EntryInstructionPK);
		}

		public void TestInvoiceHeaderPK_Readonly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var nfeImportObject = new NFEImportObject(Factory);
			nfeImportObject.Declaration = declaration;

			Assert("InvoiceHeaderPK editable for persistent declaration", !nfeImportObject.InvoiceHeaderPKInfo.ReadOnly);

			declaration.MakeNonPersistent();
			Assert("InvoiceHeaderPK readonly for non persistent declaration, not allow to create new invoice", nfeImportObject.InvoiceHeaderPKInfo.ReadOnly);
		}

		public void TestIncotermCurrencyReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var nfeImportObject = new NFEImportObject(Factory);

			declaration.Invoices.AddNew();
			nfeImportObject.Declaration = declaration;
			nfeImportObject.InvoiceHeaderPK = declaration.Invoices[0].PK;

			AssertEquals("Incoterm should be readonly", true, nfeImportObject.IncotermInfo.ReadOnly);
			AssertEquals("Currency should be readonly", true, nfeImportObject.CurrencyCodeInfo.ReadOnly);

			nfeImportObject.InvoiceHeaderPK = ZGuid.Empty;

			AssertEquals("Incoterm should be readonly", false, nfeImportObject.IncotermInfo.ReadOnly);
			AssertEquals("Currency should be readonly", false, nfeImportObject.CurrencyCodeInfo.ReadOnly);
		}

		public void TestInvoicePKChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			var nfeImportObject = new NFEImportObject(Factory);

			nfeImportObject.InvoiceHeaderPK = ZGuid.Empty;
			AssertCurrencyAndIncoterm(ZString.Empty, ZString.Empty);

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoiceHeader.JZ_IncoTerm = BRIncoTermList.Codes.FOB;

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader2.JZ_IncoTerm = BRIncoTermList.Codes.CIF;

			nfeImportObject.Declaration = declaration;
			nfeImportObject.InvoiceHeaderPK = invoiceHeader.PK;
			AssertCurrencyAndIncoterm(Core.Constants.CurrencyCodes.Brazil, BRIncoTermList.Codes.FOB);

			nfeImportObject.Declaration = declaration;
			nfeImportObject.InvoiceHeaderPK = invoiceHeader2.PK;
			AssertCurrencyAndIncoterm(Core.Constants.CurrencyCodes.UnitedStates, BRIncoTermList.Codes.CIF);

			nfeImportObject.InvoiceHeaderPK = ZGuid.Invalid;
			AssertCurrencyAndIncoterm(Core.Constants.CurrencyCodes.UnitedStates, BRIncoTermList.Codes.CIF);

			nfeImportObject.InvoiceHeaderPK = ZGuid.Empty;
			AssertCurrencyAndIncoterm(Core.Constants.CurrencyCodes.UnitedStates, BRIncoTermList.Codes.CIF);

			nfeImportObject.Declaration = declaration;
			nfeImportObject.InvoiceHeaderPK = invoiceHeader.PK;
			AssertCurrencyAndIncoterm(Core.Constants.CurrencyCodes.Brazil, BRIncoTermList.Codes.FOB);

			nfeImportObject.InvoiceHeaderPK = ZGuid.Empty;
			AssertCurrencyAndIncoterm(ZString.Empty, ZString.Empty);

			void AssertCurrencyAndIncoterm(string expectedCurrency, string expectedIncoterm)
			{
				CombineAssertions(() =>
				{
					AssertEquals("Currency", expectedCurrency, nfeImportObject.CurrencyCode);
					AssertEquals("Incoterm", expectedIncoterm, nfeImportObject.Incoterm);
				});
			}
		}

		public void TestEntryInstructionPK()
		{
			var declaration = Factory.New<JobDeclaration>();

			var nfeImportObject1 = new NFEImportObject(Factory);
			nfeImportObject1.Declaration = declaration;

			AssertEquals("EntryInstructionPK should be", ZGuid.Empty, nfeImportObject1.EntryInstructionPK);

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;

			var nfeImportObject2 = new NFEImportObject(Factory);
			nfeImportObject2.Declaration = declaration;

			AssertEquals("EntryInstructionPK should be", ZGuid.Empty, nfeImportObject2.EntryInstructionPK);

			entryInstruction2.CEI_LegalDocument = LegalDocumentList.Codes.NoInvoice;

			var nfeImportObject3 = new NFEImportObject(Factory);
			nfeImportObject3.Declaration = declaration;

			AssertEquals("EntryInstructionPK should be", entryInstruction1.PK, nfeImportObject3.EntryInstructionPK);
		}

		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NFEImportObject(Factory);
		}

		#endregion
	}
}
