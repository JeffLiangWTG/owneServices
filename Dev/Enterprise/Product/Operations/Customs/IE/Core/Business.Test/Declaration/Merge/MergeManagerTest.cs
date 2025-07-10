using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class MergeManagerTest : TestCaseWithFactory
	{
		public void TestGetReasonCannotMerge_InvoiceLineWithoutEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "AB1";
			var invoice1 = declaration.Invoices.AddNew();
			var invoice1Line = invoice1.JobComInvoiceLines.AddNew();
			var mergeManager = new MergeManager(declaration);
			var notifier = new SendsMessagesToCustomsShutterUpperer(false);
			AssertEquals("Should show error for line without entry instruction selected", "Entry Instruction should be selected on all Invoice Lines", mergeManager.CheckAndGetPrerequisiteConditions(notifier));
			invoice1Line.JI_CEI = instruction1.PK;
			AssertEquals("All lines have entry instruction selected", string.Empty, mergeManager.CheckAndGetPrerequisiteConditions(notifier));
		}

		public void TestGetReasonCannotMerge_HasMultipleDeliveryTerms()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "AB1";
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = "AB2";
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_IncoTerm = "FoB";
			invoice1.JZ_IncoTermPlace = "BoB'S PLACE";
			invoice1.ZG_AgreedPlaceCode = "HeRE";
			invoice1.JZ_AdditionalTerms = "These Terms";
			var invoice1Line = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line.JI_CEI = instruction2.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = "FOB";
			invoice2.JZ_IncoTermPlace = "BOB'S PLACE";
			invoice2.ZG_AgreedPlaceCode = "THERE";
			invoice2.JZ_AdditionalTerms = "THOSE Terms";
			var invoice2Line = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line.JI_CEI = instruction2.PK;

			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_IncoTerm = "CAF";
			invoice3.JZ_IncoTermPlace = "JIM'S PLACE";
			invoice3.ZG_AgreedPlaceCode = "WHERE";
			invoice3.JZ_AdditionalTerms = "Other Terms";
			var invoice3Line = invoice3.JobComInvoiceLines.AddNew();
			invoice3Line.JI_CEI = instruction1.PK;
			var mergeManager = new MergeManager(declaration);
			var notifier = new SendsMessagesToCustomsShutterUpperer(false);
			CombineAssertions(() =>
			{
				AssertEquals("Pre - instruction1.HasMultipleDeliveryTerms", false, instruction1.HasMultipleDeliveryTerms);
				AssertEquals("Pre - instruction2.HasMultipleDeliveryTerms", true, instruction2.HasMultipleDeliveryTerms);
				var message = $"Cannot merge as Entry Instructions '{instruction2.HumanReadableName}' is linked to invoices with different delivery terms ({invoice1.JZ_IncoTermInfo.HumanReadableName}, {invoice1.JZ_IncoTermPlaceInfo.HumanReadableName}, {invoice1.ZG_AgreedPlaceCodeInfo.HumanReadableName}, {invoice1.JZ_AdditionalTermsInfo.HumanReadableName}).";
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("No error for Import", string.Empty, mergeManager.CheckAndGetPrerequisiteConditions(notifier));
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Export has error", message, mergeManager.CheckAndGetPrerequisiteConditions(notifier));

				invoice2.ZG_AgreedPlaceCode = "HERE";
				invoice2.JZ_AdditionalTerms = "These Terms";
				AssertEquals("instruction2.HasMultipleDeliveryTerms", false, instruction2.HasMultipleDeliveryTerms);
				AssertEquals("No error as instruction2.HasMultipleDeliveryTerms is false", string.Empty, mergeManager.CheckAndGetPrerequisiteConditions(notifier));
			});
		}

		public void TestGetReasonCannotMerge_HasMultipleCurrencies()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "AB1";
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = "AB2";
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoice1Line = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line.JI_CEI = instruction2.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var invoice2Line = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line.JI_CEI = instruction2.PK;

			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedKingdom;
			var invoice3Line = invoice3.JobComInvoiceLines.AddNew();
			invoice3Line.JI_CEI = instruction1.PK;
			var mergeManager = new MergeManager(declaration);
			var notifier = new SendsMessagesToCustomsShutterUpperer(false);
			CombineAssertions(() =>
			{
				AssertEquals("Pre - instruction1.HasMultipleCurrencies", false, instruction1.HasMultipleCurrencies);
				AssertEquals("Pre - instruction2.HasMultipleCurrencies", true, instruction2.HasMultipleCurrencies);

				var message = $"Cannot merge as Entry Instructions '{instruction2.HumanReadableName}' is linked to invoices with different currencies ({invoice1.JZ_RX_NKInvoice_CurrencyInfo.HumanReadableName}).";
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("No error for Import", string.Empty, mergeManager.CheckAndGetPrerequisiteConditions(notifier));

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Export has error", message, mergeManager.CheckAndGetPrerequisiteConditions(notifier));

				invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				AssertEquals("instruction2.HasMultipleCurrencies", false, instruction2.HasMultipleCurrencies);
				AssertEquals("No error as instruction2.HasMultipleCurrencies is false", string.Empty, mergeManager.CheckAndGetPrerequisiteConditions(notifier));
			});
		}

		public void TestGetReasonCannotMerge_HasMultipleNatureOfTransactions()
		{
			var declaration = Factory.New<JobDeclaration>();

			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "AB1";
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = "AB2";

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_ValuationCode = NatureOfTransactionList.Codes._11;
			var invoice1Line = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line.JI_CEI = instruction2.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_ValuationCode = NatureOfTransactionList.Codes._12;
			var invoice2Line = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line.JI_CEI = instruction2.PK;

			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_ValuationCode = NatureOfTransactionList.Codes._13;
			var invoice3Line = invoice3.JobComInvoiceLines.AddNew();
			invoice3Line.JI_CEI = instruction1.PK;
			var mergeManager = new MergeManager(declaration);
			var notifier = new SendsMessagesToCustomsShutterUpperer(false);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
			{
				AssertEquals("Pre - IsTransitionPeriodAES30 is true", true, declaration.IsTransitionPeriodAES30);

				AssertEquals("Pre - instruction1.HasMultipleNatureOfTransactions", false, instruction1.HasMultipleNatureOfTransactions);
				AssertEquals("Pre - instruction2.HasMultipleNatureOfTransactions", true, instruction2.HasMultipleNatureOfTransactions);

				var message = $"Cannot merge as Entry Instructions '{instruction2.HumanReadableName}' is linked to invoices with different Nature of Trans. ({invoice1.JZ_ValuationCodeInfo.HumanReadableName}).";
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("No error for Import", string.Empty, mergeManager.CheckAndGetPrerequisiteConditions(notifier));

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Export has error", message, mergeManager.CheckAndGetPrerequisiteConditions(notifier));

				invoice2.JZ_ValuationCode = NatureOfTransactionList.Codes._11;
				AssertEquals("instruction2.HasMultipleCurrencies", false, instruction2.HasMultipleNatureOfTransactions);
				AssertEquals("No error as instruction2.HasMultipleNatureOfTransactions is false", string.Empty, mergeManager.CheckAndGetPrerequisiteConditions(notifier));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, false))
			{
				AssertEquals("Pre - IsTransitionPeriodAES30 is false", false, declaration.IsTransitionPeriodAES30);

				AssertEquals("Pre - instruction1.HasMultipleNatureOfTransactions", false, instruction1.HasMultipleNatureOfTransactions);
				AssertEquals("Pre - instruction2.HasMultipleNatureOfTransactions", false, instruction2.HasMultipleNatureOfTransactions);

				var message = $"Cannot merge as Entry Instructions '{instruction2.HumanReadableName}' is linked to invoices with different Nature of Trans. ({invoice1.JZ_ValuationCodeInfo.HumanReadableName}).";
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("No error for Import", string.Empty, mergeManager.CheckAndGetPrerequisiteConditions(notifier));

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("No error for Export", string.Empty, mergeManager.CheckAndGetPrerequisiteConditions(notifier));

				invoice2.JZ_ValuationCode = NatureOfTransactionList.Codes._11;
				AssertEquals("instruction2.HasMultipleCurrencies", false, instruction2.HasMultipleNatureOfTransactions);
				AssertEquals("No error as instruction2.HasMultipleNatureOfTransactions is false", string.Empty, mergeManager.CheckAndGetPrerequisiteConditions(notifier));
			}
		}
	}
}
