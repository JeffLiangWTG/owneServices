using System.Linq;
using System.Reflection;
using CargoWise.Definitions;
using NUnit.Framework;

namespace Enterprise.Core.Testing
{
	sealed class ConstantsTest : TestCase
	{
		public void TestDocumentDeliveryDefaultLanguagesFallbackType()
		{
			AssertEquals("System", Constants.DocumentDeliveryDefaultLanguagesFallbackType.System);
			AssertEquals("Organization", Constants.DocumentDeliveryDefaultLanguagesFallbackType.Organization);
			AssertEquals("Contact", Constants.DocumentDeliveryDefaultLanguagesFallbackType.Contact);
			AssertEquals("Company", Constants.DocumentDeliveryDefaultLanguagesFallbackType.Company);
			AssertEquals("Branch", Constants.DocumentDeliveryDefaultLanguagesFallbackType.Branch);
			AssertEquals("Address", Constants.DocumentDeliveryDefaultLanguagesFallbackType.Address);
		}

		public void TestOrganisationCreateComplianceDocumentOnPostingTypes()
		{
			AssertEquals("NON", Constants.OrganisationCreateComplianceDocumentOnPostingTypes.NotApplicable);
			AssertEquals("PCD", Constants.OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber);
			AssertEquals("RCC", Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge);
			AssertEquals("NRC", Constants.OrganisationCreateComplianceDocumentOnPostingTypes.NotRollup);
		}

		public void TestDebtorTypes()
		{
			AssertEquals("GRP", Constants.DebtorTypes.Code.DebtorGroup);
			AssertEquals("ORG", Constants.DebtorTypes.Code.DebtorOrganisation);
			AssertEquals("Debtor Groups", Constants.DebtorTypes.Description.DebtorGroup);
			AssertEquals("Debtors", Constants.DebtorTypes.Description.DebtorOrganisation);
		}

		public void TestTransactionCategory()
		{
			AssertEquals("STD", Constants.TransactionCategory.Codes.Standard);
			AssertEquals("SBC", Constants.TransactionCategory.Codes.SelfBilling);
			AssertEquals("CLM", Constants.TransactionCategory.Codes.ClaimRelated);
			AssertEquals("WHT", Constants.TransactionCategory.Codes.WithholdingTax);
			AssertEquals("TNF", Constants.TransactionCategory.Codes.TransactionNotFound);
			AssertEquals("TAP", Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
			AssertEquals("CLR", Constants.TransactionCategory.Codes.Clearing);
			AssertEquals("AJJ", Constants.TransactionCategory.Codes.AutoJobRevenueJournal);
			AssertEquals("PBW", Constants.TransactionCategory.Codes.PaymentBasisWithholding);
			AssertEquals("CLJ", Constants.TransactionCategory.Codes.ClearingJournal);
			AssertEquals("INJ", Constants.TransactionCategory.Codes.InstalmentJournal);
			AssertEquals("API", Constants.TransactionCategory.Codes.CashAdvanceInvoice);
			AssertEquals("APR", Constants.TransactionCategory.Codes.CashAdvanceReceived);
			AssertEquals("APP", Constants.TransactionCategory.Codes.CashAdvancePaid);
			AssertEquals("UNR", Constants.TransactionCategory.Codes.UnrealizedExchangeGainLoss);

			AssertEquals("Standard", Constants.TransactionCategory.Descriptions.Standard);
			AssertEquals("Self Billing", Constants.TransactionCategory.Descriptions.SelfBilling);
			AssertEquals("Claim Related", Constants.TransactionCategory.Descriptions.ClaimRelated);
			AssertEquals("Withholding Tax", Constants.TransactionCategory.Descriptions.WithholdingTax);
			AssertEquals("Transaction Not Found", Constants.TransactionCategory.Descriptions.TransactionNotFound);
			AssertEquals("Transaction Already Paid", Constants.TransactionCategory.Descriptions.TransactionAlreadyPaid);
			AssertEquals("Clearing", Constants.TransactionCategory.Descriptions.Clearing);
			AssertEquals("Automatic Job Revenue Journal", Constants.TransactionCategory.Descriptions.AutoJobRevenueJournal);
			AssertEquals("Payment Basis Withholding", Constants.TransactionCategory.Descriptions.PaymentBasisWithholding);
			AssertEquals("Clearing Journal", Constants.TransactionCategory.Descriptions.ClearingJournal);
			AssertEquals("Installment Journal", Constants.TransactionCategory.Descriptions.InstalmentJournal);
			AssertEquals("Advance Payment Received", Constants.TransactionCategory.Descriptions.CashAdvanceReceived);
			AssertEquals("Advance Payment Paid", Constants.TransactionCategory.Descriptions.CashAdvancePaid);
			AssertEquals("Advance Payment Invoice", Constants.TransactionCategory.Descriptions.CashAdvanceInvoice);
			AssertEquals("Unrealized Exchange Gain/Loss", Constants.TransactionCategory.Descriptions.UnrealizedExchangeGainLoss);
		}

		public void TestTaxOverrideTransactionContext()
		{
			AssertEquals("ALL", Constants.TaxOverrideTransactionContext.Codes.All);
			AssertEquals("INT", Constants.TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport);
			AssertEquals("STD", Constants.TaxOverrideTransactionContext.Codes.Standard);
			AssertEquals("All Context", Constants.TaxOverrideTransactionContext.Descriptions.All);
			AssertEquals("Intercompany Invoice Import", Constants.TaxOverrideTransactionContext.Descriptions.IntercompanyInvoiceImport);
			AssertEquals("Standard Context", Constants.TaxOverrideTransactionContext.Descriptions.Standard);
		}

		public void TestTaxOverrideDefaultingRule()
		{
			AssertEquals("ART", Constants.TaxOverrideDefaultingRule.Codes.CopyARAmount);
			AssertEquals("NON", Constants.TaxOverrideDefaultingRule.Codes.NotApplicable);
			AssertEquals("SUM", Constants.TaxOverrideDefaultingRule.Codes.SumARAmount);
			AssertEquals("Copy AR Transaction's Tax ID, Ex-Tax Amount, Tax Amount as AP", Constants.TaxOverrideDefaultingRule.Descriptions.CopyARAmount);
			AssertEquals("Not Applicable", Constants.TaxOverrideDefaultingRule.Descriptions.NotApplicable);
			AssertEquals("Sum AR Transaction's Ex-Tax Amount and Tax Amount as AP Transaction's Ex-Tax Amount", Constants.TaxOverrideDefaultingRule.Descriptions.SumARAmount);
		}

		public void TestZeroTaxReferenceRateType()
		{
			AssertEquals("ZERO", Constants.ZeroTaxReferenceRateType.Zero);
			AssertEquals("EMPTY", Constants.ZeroTaxReferenceRateType.Empty);
		}

		public void TestWorkflowReservedTypes()
		{
			Assert(Constants.Workflow.IsReservedTaskType(Constants.Workflow.ExceptionType));
			Assert(Constants.Workflow.IsReservedTaskType(Constants.Workflow.MilestoneType));
			Assert(Constants.Workflow.IsReservedTaskType(Constants.Workflow.WorkflowTriggerType));
			AssertEquals(3, Constants.Workflow.ReservedTaskTypes.Count());

			for (int i = 0; i + 1 < Constants.Workflow.ReservedTaskTypes.Count(); i++)
			{
				var code1 = Constants.Workflow.ReservedTaskTypes.ElementAt(i);
				var code2 = Constants.Workflow.ReservedTaskTypes.ElementAt(i + 1);

				AssertLessThan($"Codes {code1} and {code2} should be in alphabetical order", string.Compare(code1, code2), 0);
			}
		}

		public void TestCustomsUniversalRefCusProcedure()
		{
			AssertEquals("71", Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration);
			AssertEquals("01", Constants.Customs.Universal.RefCusProcedure.Codes._01);
			AssertEquals("07", Constants.Customs.Universal.RefCusProcedure.Codes._07);
			AssertEquals("40", Constants.Customs.Universal.RefCusProcedure.Codes._40);
			AssertEquals("42", Constants.Customs.Universal.RefCusProcedure.Codes._42);
			AssertEquals("43", Constants.Customs.Universal.RefCusProcedure.Codes._43);
			AssertEquals("44", Constants.Customs.Universal.RefCusProcedure.Codes._44);
			AssertEquals("45", Constants.Customs.Universal.RefCusProcedure.Codes._45);
			AssertEquals("46", Constants.Customs.Universal.RefCusProcedure.Codes._46);
			AssertEquals("48", Constants.Customs.Universal.RefCusProcedure.Codes._48);
			AssertEquals("51", Constants.Customs.Universal.RefCusProcedure.Codes._51);
			AssertEquals("53", Constants.Customs.Universal.RefCusProcedure.Codes._53);
			AssertEquals("61", Constants.Customs.Universal.RefCusProcedure.Codes._61);
			AssertEquals("63", Constants.Customs.Universal.RefCusProcedure.Codes._63);
			AssertEquals("68", Constants.Customs.Universal.RefCusProcedure.Codes._68);
			AssertEquals("95", Constants.Customs.Universal.RefCusProcedure.Codes._95);
			AssertEquals("96", Constants.Customs.Universal.RefCusProcedure.Codes._96);
		}

		public void TestRefCusCodeListTypes()
		{
			AssertEquals("AI44T", Constants.Customs.Universal.RefCusCodeListTypes.Codes.AI44T);
			AssertEquals("AR44T", Constants.Customs.Universal.RefCusCodeListTypes.Codes.AR44T);
		}

		public void TestTransitWarehouseTransportUnitTypes()
		{
			AssertEquals("ULD", Constants.TransitWarehouseTransportUnitTypes.ULD);
			AssertEquals("CNT", Constants.TransitWarehouseTransportUnitTypes.Container);
			AssertEquals("VEH", Constants.TransitWarehouseTransportUnitTypes.Vehicle);
		}

		public void TestFreightServiceTypeCodes()
		{
			var sharedFreightServiceTypeCodes = typeof(FreightServiceTypes.Codes)
				.GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(x => x.FieldType == typeof(string))
				.Select(x => (string)x.GetValue(null));

			var additionalFreightServiceTypeCodes = new[] { "STD" };
			var expectedFreightServiceTypeCodes = additionalFreightServiceTypeCodes.Union(sharedFreightServiceTypeCodes);

			var freightServiceTypeCodes = typeof(Constants.FreightServiceType.Codes)
				.GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(x => x.FieldType == typeof(string))
				.Select(x => (string)x.GetValue(null));

			AssertContainsExactElementsInAnyOrder(expectedFreightServiceTypeCodes, freightServiceTypeCodes);
		}

		public void TestFreightServiceTypeDescriptions()
		{
			var expectedFreightServiceTypeDescriptions = typeof(FreightServiceTypes.Descriptions)
				.GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(x => x.FieldType == typeof(string))
				.Select(x => (string)x.GetValue(null));

			var freightServiceTypeDescriptions = typeof(Constants.FreightServiceType.Descriptions)
				.GetProperties()
				.Select(x => ((ZArchitecture.Core.ResourceString)x.GetValue(null)).EnglishText);

			AssertContainsExactElementsInAnyOrder(expectedFreightServiceTypeDescriptions, freightServiceTypeDescriptions);
		}
	}
}
