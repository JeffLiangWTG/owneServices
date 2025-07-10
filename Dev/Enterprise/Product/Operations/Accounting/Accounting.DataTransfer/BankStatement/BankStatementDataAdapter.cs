using System;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.BankStatement
{
	public class BankStatementDataAdapter : BaseAccountingDataAdapter<Business.Base.AccStatement.BankStatement, Xsd.BankStatement>
	{
		public static string MoreThanOneBankStatementErrorMsg
		{
			get { return Res.GetString("531d2e12-d360-4944-b57c-2ce9a0e21416", "There is more than one Bank Statement transaction in the XML file."); }
		}

		#region Data Adapter Overrides

		public override string RootCollectionElementName
		{
			get { return "BankStatements"; }
		}

		public override string RootElementName
		{
			get { return "BankStatement"; }
		}

		public override XmlSchema Schema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.BankStatementSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.BankStatementsSchema; }
		}

		protected override Business.Base.AccStatement.BankStatement FindBusinessObject(Xsd.BankStatement value, IValueObjectImportContext context)
		{
			if (!value.BankAccount.IsEmpty && !value.CompanyCode.IsEmpty)
			{
				var companyQuery = new ZQuery(GlbCompanySchema.GC_Code, value.CompanyCode);
				var companyPK = context.Factory.LoadTop1<GlbCompany>(companyQuery).PK;

				var bankAccQuery = new ZQuery(AccBankAccountSchema.AB_GC, companyPK);
				bankAccQuery.AddToFilter(AccBankAccountSchema.AB_Code, value.BankAccount);

				return context.Factory.LoadTop1<Business.Base.AccStatement.BankStatement>(bankAccQuery);
			}

			return null;
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(Business.Base.AccStatement.BankStatement bizObj, Xsd.BankStatement @object, IValueObjectExportContext context)
		{
			throw new NotSupportedException("Exporting BankStatement is currently not supported");
		}

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(Business.Base.AccStatement.BankStatement bizObj, Xsd.BankStatement value, IValueObjectImportContext context)
		{
			if (value != null)
			{
				ImportStatements(bizObj, value, context);
			}
		}

		void ImportStatements(Business.Base.AccStatement.BankStatement bizObj, Xsd.BankStatement value, IValueObjectImportContext context)
		{
			fNoOfStatementsImportedSuccessfully = 0;
			fNoOfDirectTransactionsCreated = 0;

			foreach (Xsd.BankStatementBankStatementLine line in value.BankStatementLines)
			{
				Statement statement = bizObj.AddNewStatement();
				statement.IsImportingData = true;
				try
				{
					statement.AS_DebitCredit = (line.TransactionAmount >= 0) ? Statement.DEBIT : Statement.CREDIT;
					if (!line.StatementDate.IsEmpty)
					{
						statement.AS_StatementDate = line.StatementDate;
					}
					context.SetPropertyInfoValue(statement.AS_ChequeOrReferenceInfo, line.ChequeOrReferenceNumber, line.ChequeOrReferenceNumberSpecified);
					statement.AS_Amount = Math.Abs(line.TransactionAmount);
					context.SetPropertyInfoValue(statement.AS_TypeInfo, line.TransactionType, line.TransactionTypeSpecified);

					DirectTransactionHeaderBase directTransaction = statement.DirectTransaction;
					if (directTransaction != null)
					{
						context.SetPropertyInfoValue(directTransaction.AH_DescInfo, line.TransactionDescription, line.TransactionDescriptionSpecified);
						context.SetPropertyInfoValue(directTransaction.AH_ChequeDrawerInfo, line.PayeeName, line.PayeeNameSpecified);
						if (!line.TransactionDate.IsEmpty)
						{
							directTransaction.AH_InvoiceDate = line.TransactionDate;
							directTransaction.AH_PostDate = line.TransactionDate;
						}
						directTransaction.AH_RX_NKTransactionCurrency = bizObj.AB_RX_NKAccountCurrency;

						fNoOfDirectTransactionsCreated++;
					}

					fNoOfStatementsImportedSuccessfully++;
				}
				finally
				{
					statement.IsImportingData = false;
				}
			}

			AddErrorsToNotifications(bizObj, value, context);
		}

		void AddErrorsToNotifications(Business.Base.AccStatement.BankStatement bizObj, Xsd.BankStatement value, IValueObjectImportContext context)
		{
			foreach (string errorString in bizObj.NotificationsIncludingChildren.GetErrors().GetUniqueMessageList())
			{
				context.Notify(new ErrorNotification(ErrorType.Error, errorString));
			}

			foreach (string warningString in bizObj.NotificationsIncludingChildren.GetWarnings().GetUniqueMessageList())
			{
				context.Notify(new ErrorNotification(ErrorType.Warning, warningString));
			}
		}

		#region NoOfStatementsImportedSuccessfully

		int fNoOfStatementsImportedSuccessfully;
		public int NoOfStatementsImportedSuccessfully
		{
			get { return fNoOfStatementsImportedSuccessfully; }
		}

		#endregion

		#region NoOfDirectTransactionsCreated

		int fNoOfDirectTransactionsCreated;
		public int NoOfDirectTransactionsCreated
		{
			get { return fNoOfDirectTransactionsCreated; }
		}

		#endregion

		#endregion

		#region Test metheds/property wrapper
		public void ImportFromValueObjectCore_ForTestOnly(Business.Base.AccStatement.BankStatement bizObj, Xsd.BankStatement value, IValueObjectImportContext context)
		{
			ImportFromValueObjectCore(bizObj, value, context);
		}
		#endregion
	}
}
