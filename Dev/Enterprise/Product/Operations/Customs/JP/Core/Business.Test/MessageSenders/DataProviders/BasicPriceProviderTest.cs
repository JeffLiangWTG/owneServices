using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(BasicPriceProvider))]
	sealed class BasicPriceProviderTest : TestCaseWithFactory
	{
		public void TestBasicPrice()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = "TRF";
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_DateForDuty = new ZDateTime(2024, 8, 15);
			entryInstruction2.CEI_DateForDuty = new ZDateTime(2024, 8, 30);

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			var line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			var charge1 = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 1000m, invoiceHeader.JZ_RX_NKInvoice_Currency);
			charge1.J7_IsDutiable = true;
			var charge2 = line2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 1000m, invoiceHeader.JZ_RX_NKInvoice_Currency);
			charge2.J7_IsDutiable = true;

			line1.JI_LinePrice = 1m;
			line2.JI_LinePrice = 1m;
			line1.JI_CEI = entryInstruction1.PK;
			line2.JI_CEI = entryInstruction2.PK;

			using (var company = GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Japan))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				Factory.Save();
				var basicPriceProvider = new BasicPriceProvider(declaration.ActiveEntryHeaders[0].MergedLines[0]);
				AssertNull(basicPriceProvider.Coefficient);
				AssertEquals(1001m, basicPriceProvider.BasicPrice.Amount);
				AssertEquals(Core.Constants.CurrencyCodes.Australia, basicPriceProvider.BasicPrice.CurrencyCode);
			}
		}
	}
}
