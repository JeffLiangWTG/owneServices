using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	public class GuaranteeTestHelper
	{
		readonly BusinessObjectFactory factory;
		readonly decimal openingAmount;

		public GuaranteeTestHelper(BusinessObjectFactory factory, decimal openingAmount = 100m)
		{
			this.factory = factory;
			this.openingAmount = openingAmount;
		}

		public CusBondDetail CreateValidNotLinkedGuarantee(string reference = "Entry Reference", string appId = "Job Number", decimal amount = 3.14m)
		{
			var guarantee = CreateGuarantee();
			guarantee.Instruction.JobDeclaration.JE_DeclarationReference = appId;
			guarantee.PW_BondType = GuaranteeBondTypeList.Codes.Continuous;
			guarantee.PW_CPH_Guarantee = CusGuarantee.PK;
			guarantee.PW_BondAmount = amount;
			guarantee.PW_BondNumber2 = reference;
			guarantee.PW_BondEffectiveDate = ZDateTime.Today;
			return guarantee;
		}

		public CusBondDetail CreateInvalidNotLinkedGuarantee()
		{
			var guarantee = CreateGuarantee();
			guarantee.PW_BondType = GuaranteeBondTypeList.Codes.Continuous;
			guarantee.Instruction.EntryHeader.CH_BondAcquittedDate = ZDate.Today;
			guarantee.PW_CPH_Guarantee = ZGuid.NewZGuid();
			guarantee.PW_BondAmount = 3.14m;
			guarantee.PW_BondNumber2 = "Entry Reference";
			guarantee.PW_BondEffectiveDate = ZDateTime.Today;
			return guarantee;
		}

		public CusBondDetail CreateValidLinkedGuarantee()
		{
			var guarantee = CreateValidNotLinkedGuarantee();
			guarantee.LinkOrUnlink();
			return guarantee;
		}

		public CusGuaranteeHeader CusGuarantee => cusGuarantee ?? (cusGuarantee = CreateCusGuarantee());
		CusGuaranteeHeader cusGuarantee;

		public static void AdjustCusGuaranteeAmount(CusGuaranteeHeader cusGuarantee, ZDecimal amount, string reference = "Reference2")
		{
			var adjustTransaction = cusGuarantee.CusGuaranteeLineTransactions.AddNew();
			adjustTransaction.CPL_Reference = reference;
			adjustTransaction.CPL_TranValue = amount;
			adjustTransaction.CPL_IsAggregated = ZBool.False;
			adjustTransaction.CPL_Comment = "Comment2";
			adjustTransaction.CPL_Procedure = "AAA";
		}

		public static void AssertGuaranteeTransactions(string message, CusGuaranteeHeader guarantee, ZDecimal available, params (ZString Reference, ZDecimal Amout, ZString appId)[] transactions)
		{
			Assertion.AssertEquals(message + "->CPH_Calc_TotalBalanceIncludingPendingDecimal", available, guarantee.CPH_Calc_TotalBalanceIncludingPendingDecimal);
			Assertion.AssertEquals(message + "->Transaction Count", transactions.Length, guarantee.CusGuaranteeLineTransactions.Count);
			for (var i = 0; i < transactions.Length; i++)
			{
				var transaction = guarantee.CusGuaranteeLineTransactions[i];
				if (transaction.CPL_TransactionType == PermitTransactionTypeList.Codes.ADJ)
				{
					Assertion.AssertEquals(message + $"-> ADJ Transaction {i + 1}->CPL_TranValue", transactions[i].Amout, transaction.CPL_TranValue);
				}
				else
				{
					AssertGuaranteeTransaction(message + $"->Transaction {i + 1}", transaction, i == 0 ? PermitTransactionTypeList.Codes.OBL : PermitTransactionTypeList.Codes.CUS, transactions[i].Reference, transactions[i].Amout, transactions[i].appId);
				}
			}
		}

		static void AssertGuaranteeTransaction(string message, SharedCusPermitLineTransaction transaction, ZString transactionType, ZString reference, ZDecimal tranValue, ZString appId)
		{
			Assertion.AssertEquals(message + "->CPL_TranValue", tranValue, transaction.CPL_TranValue);
			Assertion.AssertEquals(message + "->CPL_TransactionDate", ZDateTime.Now, transaction.CPL_TransactionDate);
			Assertion.AssertEquals(message + "->CPL_TransactionStatus", PermitTransactionStatusList.Codes.Confirmed, transaction.CPL_TransactionStatus);
			Assertion.AssertEquals(message + "->CPL_TransactionType", transactionType, transaction.CPL_TransactionType);
			Assertion.AssertEquals(message + "->CPL_TransactionCategory", PermitTransactionCategoryList.Codes.CUM, transaction.CPL_TransactionCategory);
			Assertion.AssertEquals(message + "->CPL_AppId", appId, transaction.CPL_AppId);
			Assertion.AssertEquals(message + "->CPL_Reference", reference, transaction.CPL_Reference);
			Assertion.AssertEquals(message + "->CPL_IsAggregated", ZBool.False, transaction.CPL_IsAggregated);
			Assertion.AssertEquals(message + "->CPL_Comment", "Instruction Desc.", transaction.CPL_Comment);
			Assertion.AssertEquals(message + "->CPL_Procedure", "AAA", transaction.CPL_Procedure);
		}

		CusBondDetail CreateGuarantee()
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Description = "Instruction Desc.";
			instruction.CEI_Style = "AAA";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			return instruction.Guarantee;
		}

		CusGuaranteeHeader CreateCusGuarantee()
		{
			var permitHolder = factory.New<OrgHeader>();
			permitHolder.OH_Code = "AB1";
			var guaranteeHeader = factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_OH_PermitHolder = permitHolder.PK;
			guaranteeHeader.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			guaranteeHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddDays(1);
			var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_Reference = "Opening";
			transaction.CPL_TranValue = openingAmount;
			transaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			transaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			transaction.CPL_AppId = "Job Number";
			transaction.CPL_IsAggregated = ZBool.False;
			transaction.CPL_Comment = "Instruction Desc.";
			transaction.CPL_Procedure = "AAA";
			return guaranteeHeader;
		}
	}
}
