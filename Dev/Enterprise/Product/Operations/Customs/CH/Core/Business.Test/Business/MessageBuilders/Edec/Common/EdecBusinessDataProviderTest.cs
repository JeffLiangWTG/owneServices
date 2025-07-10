using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

abstract class EdecBusinessDataProviderTest : TestCaseWithFactory
{
	protected abstract string MessageType { get; }

	protected abstract EdecBusinessDataProvider CreateEdecBusinessDataProvider(CusEntryHeader entryHeader);

	protected abstract void SetOrganisationsVATNumber(string vATNumber);

	public void TestConstructorNullArgument()
	{
		CombineAssertions(() =>
		{
			AssertNull("Argument == null", CreateEdecBusinessDataProvider(null));
			AssertNotNull("Argument != null", CreateEdecBusinessDataProvider(entryHeader));
		});
	}

	public virtual void TestProvider_FreeCarrierSeller()
	{
		declaration.JE_PaymentMethod = DeclarationPayerList.Codes.Consignor;
		declaration.JE_VATPaidBy = DeclarationPayerList.Codes.Importer;
		invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;

		declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeCarrierSeller;
		CombineAssertions(() =>
		{
			AssertEquals(nameof(business.Incoterms), "FCA", business.Incoterms);
			AssertEquals(nameof(business.InvoiceCurrencyType), "2", business.InvoiceCurrencyType);
		});
	}

	public virtual void TestProvider_DeliveredDutyPaid()
	{
		declaration.JE_PaymentMethod = DeclarationPayerList.Codes.Consignor;
		declaration.JE_VATPaidBy = DeclarationPayerList.Codes.Importer;
		invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;

		declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
		CombineAssertions(() =>
		{
			AssertEquals(nameof(business.Incoterms), "DDP", business.Incoterms);
			AssertEquals(nameof(business.InvoiceCurrencyType), "2", business.InvoiceCurrencyType);
		});
	}

	public void TestIncoterms()
	{
		CombineAssertions(() =>
		{
			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeCarrierSeller;
			AssertEquals($"JE_ShipmentIncoTerm={declaration.JE_ShipmentIncoTerm}", Core.Constants.IncoTerms.FreeCarrier, business.Incoterms);
			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeCarrierBuyer;
			AssertEquals($"JE_ShipmentIncoTerm={declaration.JE_ShipmentIncoTerm}", Core.Constants.IncoTerms.FreeCarrier, business.Incoterms);
			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			AssertEquals($"JE_ShipmentIncoTerm={declaration.JE_ShipmentIncoTerm}", Core.Constants.IncoTerms.DeliveredDutyPaid, business.Incoterms);
		});
	}

	public void TestVATNumber()
	{
		declaration.JE_OH_Importer = CreateOrganisation(countryCode: Core.Constants.CountryCodes.Switzerland).PK;
		declaration.JE_OH_Supplier = CreateOrganisation(countryCode: Core.Constants.CountryCodes.Switzerland).PK;

		CombineAssertions(() =>
		{
			AssertNull("No VAT-Number", business.VATNumber);

			SetOrganisationsVATNumber(ValidVATNumberE);
			AssertEquals("VAT-Number starting with E", ValidVATNumberCHE, business.VATNumber);
		});
	}

	public void TestVATSuffix()
	{
		declaration.JE_OH_Importer = CreateOrganisation(countryCode: Core.Constants.CountryCodes.Switzerland).PK;
		declaration.JE_OH_Supplier = CreateOrganisation(countryCode: Core.Constants.CountryCodes.Switzerland).PK;

		CombineAssertions(() =>
		{
			AssertEquals("No VAT-Number", null, business.VATSuffix);

			SetOrganisationsVATNumber("XYZ");
			AssertEquals("Invalid VAT-Number", false, business.VATSuffix);

			SetOrganisationsVATNumber(ValidVATNumberCHE);
			AssertEquals("Valid VAT-Number (CHE...)", true, business.VATSuffix);

			SetOrganisationsVATNumber(ValidVATNumberE);
			AssertEquals("Valid VAT-Number (E...)", true, business.VATSuffix);
		});
	}

	public void TestInvoiceCurrencyType()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No invoice", "0", business.InvoiceCurrencyType);

			var specifiedCurrencies = new HashSet<ZString>();

			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Switzerland;
			specifiedCurrencies.Add(invoiceHeader.JZ_RX_NKInvoice_Currency);
			AssertEquals(invoiceHeader.JZ_RX_NKInvoice_Currency, "1", business.InvoiceCurrencyType);

			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			specifiedCurrencies.Add(invoiceHeader.JZ_RX_NKInvoice_Currency);
			AssertEquals(invoiceHeader.JZ_RX_NKInvoice_Currency, "2", business.InvoiceCurrencyType);

			foreach (var currency in new[] { Core.Constants.CurrencyCodes.BulgariaNew, Core.Constants.CurrencyCodes.Denmark, Core.Constants.CurrencyCodes.Latvia, Core.Constants.CurrencyCodes.Lithuania, Core.Constants.CurrencyCodes.Poland, Core.Constants.CurrencyCodes.RomaniaNew, Core.Constants.CurrencyCodes.Sweden, Core.Constants.CurrencyCodes.CzechRepublic, Core.Constants.CurrencyCodes.Hungary })
			{
				invoiceHeader.JZ_RX_NKInvoice_Currency = currency;
				specifiedCurrencies.Add(invoiceHeader.JZ_RX_NKInvoice_Currency);
				AssertEquals(invoiceHeader.JZ_RX_NKInvoice_Currency, "3", business.InvoiceCurrencyType);
			}

			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			specifiedCurrencies.Add(invoiceHeader.JZ_RX_NKInvoice_Currency);
			AssertEquals(invoiceHeader.JZ_RX_NKInvoice_Currency, "4", business.InvoiceCurrencyType);

			foreach (var currency in Core.Constants.CurrencyCodes.All)
			{
				if (!specifiedCurrencies.Contains(currency))
				{
					invoiceHeader.JZ_RX_NKInvoice_Currency = currency;
					AssertEquals(invoiceHeader.JZ_RX_NKInvoice_Currency, "5", business.InvoiceCurrencyType);
				}
			}

			invoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;
			specifiedCurrencies.Add(invoiceHeader.JZ_RX_NKInvoice_Currency);
			AssertEquals("empty", "0", business.InvoiceCurrencyType);
		});
	}

	public void TestInvoiceCurrencyType_FromInvoiceWithLargestAmount()
	{
		var invoiceHeader2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine2);

		(var invoiceHeader3, var entryHeader3, var entryLine3) = CreateDeclarationInvoice(declaration);

		void setInvoiceAmount(JobComInvoiceHeader invHeader, ZDecimal amount, ZDecimal exRate, string currency)
		{
			invHeader.JZ_InvoiceAmount = amount;
			invHeader.JZ_RX_NKInvoice_Currency = currency;
			invHeader.IsJZ_InvoiceCurrExRateUserEnterable = true;
			invHeader.JZ_InvoiceCurrExRate = exRate;
		}

		CombineAssertions(() =>
		{
			setInvoiceAmount(invoiceHeader, 100, 0.8, "EUR");
			setInvoiceAmount(invoiceHeader2, 200, 2.0, "USD");
			setInvoiceAmount(invoiceHeader3, 300, 0.8, "USD");
			AssertEquals("1st (EUR) is larger", "2", business.InvoiceCurrencyType);

			setInvoiceAmount(invoiceHeader, 200, 2.0, "EUR");
			setInvoiceAmount(invoiceHeader2, 100, 0.8, "USD");
			setInvoiceAmount(invoiceHeader3, 300, 0.8, "EUR");
			AssertEquals("2nd (USD) is larger", "4", business.InvoiceCurrencyType);
		});
	}

	protected override void SetUp()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageType;
		(invoiceHeader, entryHeader, entryLine) = CreateDeclarationInvoice(declaration);

		business = CreateEdecBusinessDataProvider(entryHeader);
	}
	protected JobDeclaration declaration;
	JobComInvoiceHeader invoiceHeader;
	CusEntryHeader entryHeader;
	CusEntryLine entryLine;
	protected IEdecBusiness business;

	private protected const string ValidVATNumberCHE = "CHE105908410";
	const string ValidVATNumberE = "E105908410";

	(JobComInvoiceHeader, CusEntryHeader, CusEntryLine) CreateDeclarationInvoice(JobDeclaration declaration)
	{
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		var entryLine = entryHeader.AllEntryLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine);
		return (invoiceHeader, entryHeader, entryLine);
	}

	private protected OrgHeader CreateOrganisation(string countryCode = "", string cadCode = null, string cavCode = null, string vatCode = null, string ctpCode = null)
	{
		var orgHeader = Factory.New<OrgHeader>();

		orgHeader.Addresses.AddNewMainAddress();
		orgHeader.MiscServ.OM_IMDefaultINCOTerm = ZString.Empty;
		orgHeader.MiscServ.OM_EXDefaultIncoTerm = ZString.Empty;

		if (cadCode != null)
		{
			orgHeader.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.CAD, cadCode);
		}

		if (cavCode != null)
		{
			orgHeader.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.CAV, cavCode);
		}

		if (vatCode != null)
		{
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, vatCode);
		}

		if (ctpCode != null)
		{
			orgHeader.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.CTP, ctpCode);
		}

		return orgHeader;
	}
}
