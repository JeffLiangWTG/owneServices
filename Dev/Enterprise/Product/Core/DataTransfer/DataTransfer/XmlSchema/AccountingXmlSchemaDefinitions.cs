using System;
using System.Xml.Schema;

namespace Enterprise.DataTransfer.Xml
{
	public class AccountingXmlSchemaDefinitions : XmlSchemaDefinitionsBase
	{
		#region Instance

		protected AccountingXmlSchemaDefinitions()
		{
		}

		public static AccountingXmlSchemaDefinitions Instance
		{
			get
			{
				AccountingXmlSchemaDefinitions result = (AccountingXmlSchemaDefinitions)WeakInstance.Target;
				if (result == null)
				{
					result = new AccountingXmlSchemaDefinitions();
					WeakInstance.Target = result;
				}
				return result;
			}
		}

		static WeakReference WeakInstance
		{
			get { return weakInstance ?? (weakInstance = new WeakReference(null)); }
		}
		[ThreadStatic] static WeakReference weakInstance;

		#endregion

		[ExpectXmlSchemaContainsRootElement("BankStatements")]
		public XmlSchema BankStatementsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "BankStatement.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("BankStatement")]
		public XmlSchema BankStatementSchema
		{
			get { return GetCompiledSchemaNestedElement(BankStatementsSchema, "BankStatements"); }
		}

		[ExpectXmlSchemaContainsRootElement("GLHeadersAndChargeCodes")]
		public XmlSchema GLHeadersAndChargeCodesCollectionSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "GLHeadersAndChargeCodes.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("SingleGLHeadersAndChargeCodesElement")]
		public XmlSchema GLHeadersAndChargeCodesSchema
		{
			get { return GetCompiledSchemaNestedElement(GLHeadersAndChargeCodesCollectionSchema, "GLHeadersAndChargeCodes"); }
		}

		[ExpectXmlSchemaContainsRootElement("ChargeCodes")]
		public XmlSchema ChargeCodesSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "ChargeCode.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("ChargeCode")]
		public XmlSchema ChargeCodeSchema
		{
			get { return GetCompiledSchemaNestedElement(ChargeCodesSchema, "ChargeCodes"); }
		}

		[ExpectXmlSchemaContainsRootElement("GLHeaders")]
		public XmlSchema GLHeadersSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "GLHeader.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("GLHeader")]
		public XmlSchema GLHeaderSchema
		{
			get { return GetCompiledSchemaNestedElement(GLHeadersSchema, "GLHeaders"); }
		}

		[ExpectXmlSchemaContainsRootElement("AlternateGLAccounts")]
		public XmlSchema AlternateGLAccountsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "AlternateGLAccount.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("AlternateGLAccount")]
		public XmlSchema AlternateGLAccountSchema
		{
			get { return GetCompiledSchemaNestedElement(AlternateGLAccountsSchema, "AlternateGLAccounts"); }
		}

		[ExpectXmlSchemaContainsRootElement("GLHeaderMultiLanguageMappings")]
		public XmlSchema GLHeaderMultiLanguageMappingsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "GLHeaderMultiLanguageMapping.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("GLHeaderMultiLanguageMapping")]
		public XmlSchema GLHeaderMultiLanguageMappingSchema
		{
			get { return GetCompiledSchemaNestedElement(GLHeaderMultiLanguageMappingsSchema, "GLHeaderMultiLanguageMappings"); }
		}

		[ExpectXmlSchemaContainsRootElement("GLHeaderMultiLanguageReportSetups")]
		public XmlSchema GLHeaderMultiLanguageReportSetupsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "GLHeaderMultiLanguageReportSetup.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("GLHeaderMultiLanguageReportSetup")]
		public XmlSchema GLHeaderMultiLanguageReportSetupSchema
		{
			get { return GetCompiledSchemaNestedElement(GLHeaderMultiLanguageReportSetupsSchema, "GLHeaderMultiLanguageReportSetups"); }
		}
		[ExpectXmlSchemaContainsRootElement("GLJournals")]
		public XmlSchema GLJournalsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "GLJournal.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("GLJournal")]
		public XmlSchema SingleGLJournalSchema
		{
			get { return GetCompiledSchemaNestedElement(GLJournalsSchema, "GLJournals"); }
		}

		[ExpectXmlSchemaContainsRootElement("FinancialInvoice")]
		public XmlSchema FinancialInvoiceSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "FinancialInvoice.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement(null)]
		public XmlSchema FinancialInvoicesSchema
		{
			get { return GetCompiledSchemaNestedElement(FinancialInvoiceSchema, "FinancialInvoice"); }
		}

		[ExpectXmlSchemaContainsRootElement(null)]
		public XmlSchema SingleFinancialInvoiceSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "FinancialInvoice.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("NettingClearingJournals")]
		public XmlSchema NettingClearingJournalsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "NettingClearingJournals.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("WipOrAccrual")]
		public XmlSchema FinancialWipOrAccrualSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "FinancialWipOrAccrual.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("FinancialTransactions")]
		public XmlSchema FinancialTransactionsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "FinancialTransactions.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("Balances")]
		public XmlSchema BalancesSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "Balance.xsd"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		[ExpectXmlSchemaContainsRootElement("Balance")]
		public XmlSchema SingleBalanceSchema
		{
			get { return GetCompiledSchemaNestedElement(BalancesSchema, "Balances"); }
		}

		[ExpectXmlSchemaContainsRootElement("DebtorBalances")]
		public XmlSchema DebtorBalancesSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "DebtorBalance.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("DebtorBalance")]
		public XmlSchema SingleDebtorBalanceSchema
		{
			get { return GetCompiledSchemaNestedElement(DebtorBalancesSchema, "DebtorBalances"); }
		}

		[ExpectXmlSchemaContainsRootElement(null)]
		public XmlSchema IncompleteTransaction
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "IncompleteTransaction.xsd"); }
		}

		const string DefaultBaseResourceName = "Enterprise.DataTransfer.DataFileDefinitions.Xml.Version1";
	}
}
