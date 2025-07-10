using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class InsuranceProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(InsuranceProvider.New(null));
			AssertType<InsuranceProvider>(InsuranceProvider.New(Factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew()));
		}

		InsuranceProvider CreateInsuranceProvider(JobDeclaration declaration)
		{
			declaration.ResumeApportionment();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			return InsuranceProvider.New(declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().FirstOrDefault());
		}

		public void TestOverseasInsuranceCharge()
		{
			var (invoiceLine1, invoiceLine2) = PrepareInvoice();
			var declaration = invoiceLine1.Declaration;
			invoiceLine1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, Core.Constants.CurrencyCodes.UnitedStates);

			var dataProvider = CreateInsuranceProvider(declaration);
			CombineAssertions("ONS charges appoitioned", () =>
			{
				AssertEquals("Amount should be", 100d, dataProvider.Amount);
				AssertEquals("CurrencyCode should be","USD", dataProvider.CurrencyCode);
			});

			invoiceLine1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			dataProvider = CreateInsuranceProvider(declaration);

			CombineAssertions("All ONS charges have the same currency", () =>
			{
				AssertEquals("Amount should be", 300d, dataProvider.Amount);
				AssertEquals("CurrencyCode should be", "USD", dataProvider.CurrencyCode);
			});

			invoiceLine2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, Core.Constants.CurrencyCodes.EuropeanUnion);
			dataProvider = CreateInsuranceProvider(declaration);

			CombineAssertions("One ONS charges has different currency", () =>
			{
				AssertEquals("Amount should be", 766,67d, dataProvider.Amount);
				AssertEquals("CurrencyCode should be", "BRL", dataProvider.CurrencyCode);
			});
		}

		(JobComInvoiceLine, JobComInvoiceLine) PrepareInvoice()
		{
			var usdCurrency = RefCurrency.New(Factory);
			usdCurrency.RX_Code = Core.Constants.CurrencyCodes.UnitedStates;
			usdCurrency.SetCustomsRate(ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), 0.5m);

			var eurCurrency = RefCurrency.New(Factory);
			eurCurrency.RX_Code = Core.Constants.CurrencyCodes.EuropeanUnion;
			eurCurrency.SetCustomsRate(ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), 0.6m);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var entryInstructions = declaration.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 1800m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice.JZ_NetWeight = 60;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_NetWeight = 100;
			invoiceLine1.JI_NetWeightUQ = "KG";
			invoiceLine1.JI_LinePrice = 600m;
			invoiceLine1.JI_CEI = entryInstructions.PK;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2";
			invoiceLine2.JI_NetWeight = 100;
			invoiceLine2.JI_NetWeightUQ = "KG";
			invoiceLine2.JI_LinePrice = 600m;
			invoiceLine2.JI_CEI = entryInstructions.PK;

			return (invoiceLine1, invoiceLine2);
		}
	}
}
