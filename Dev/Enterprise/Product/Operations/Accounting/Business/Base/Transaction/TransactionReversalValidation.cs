using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionReversalValidation : TransactionHeaderValidation
	{
		public TransactionReversalValidation(TransactionHeader parent, IDataRefreshBusUpdateActionDecider dataRefreshBusUpdateActionDecider)
			: base(parent)
		{
			DataRefreshBusUpdateActionDecider = Argument.NotNull(dataRefreshBusUpdateActionDecider, nameof(dataRefreshBusUpdateActionDecider));
		}

		readonly IDataRefreshBusUpdateActionDecider DataRefreshBusUpdateActionDecider;

		public override void ValidateAll()
		{
			ValidateAH_PostDate();
			ValidateAH_InvoiceDate();
			ValidateAH_TransactionNum();
			ValidateMultipleReversing();
			ValidateAlreadyReversed();
			ValidateAlreadyGeneratedComplianceDocument();
			ValidateAH_GE();
			ValidateDataRefreshBusUpdate();
		}

		void ValidateAlreadyGeneratedComplianceDocument()
		{
			if (TransactionHeaderHelper.CheckHasGeneratedComplianceDocumentBeforeReverse(Parent.OriginalTransaction))
			{
				Parent.AddRowError(TransactionHeaderHelper.HasGeneratedComplianceDocumentErrorMessage);
			}
		}

		void ValidateAlreadyReversed()
		{
			if (Parent.OriginalTransaction != null && Parent.OriginalTransaction.IsAlreadyReversedByOtherUser())
			{
				Parent.AddRowError(Res.GetString("a0447c37-684b-4ec2-9d0c-831d2f80cb16", "This transaction was already reversed by another user."));
			}
		}

		void ValidateDataRefreshBusUpdate()
		{
			if (Parent.OriginalTransaction != null && DataRefreshBusUpdateActionDecider.HasSkippedDataRefreshBusUpdate(Parent.OriginalTransaction))
			{
				Parent.AddRowError(Res.GetString("e995e23b-3edb-4528-a8fd-4585cd0e69ca", "Original Transaction was modified by this user during another operation. Please cancel your changes and reload the form."));
			}
		}

		protected override void CheckAH_TransactionNum()
		{
			base.CheckAH_TransactionNum();
			if (Parent.AH_Ledger == LedgerTypes.AccountsPayable &&
				(Parent.AH_TransactionType == TransactionTypes.Invoice ||
				Parent.AH_TransactionType == TransactionTypes.CreditNote ||
				Parent.AH_TransactionType == TransactionTypes.AdjustmentNote))
			{
				if (!Parent.IsSelfBillingInvoice)
				{
					MandatoryValidation.CheckEntered(Parent.AH_TransactionNumInfo);
					if (!Parent.AH_OH.IsEmpty && !Parent.IsInDatabase)
					{
						if ((AccountingUtils.APTransactionNumberExists(Parent.AH_TransactionType, Parent.AH_TransactionNum, Parent.AH_OH, Parent.AH_InvoiceDate)).HasNotification)
						{
							Parent.AH_TransactionNumInfo.AddError(Res.GetString("c48a6005-d564-496b-8d3b-390cf1de40f0", "The transaction number is already in use. Please select another one."));
						}
						else if ((AccountingUtils.UATransactionNumberExists(Parent.AH_TransactionType, Parent.AH_TransactionNum, Parent.AH_OH, Parent.PK, Parent.AH_InvoiceDate)).HasNotification)
						{
							Parent.AH_TransactionNumInfo.AddError(Res.GetString("87b5840b-dc3f-4e54-97f4-37bd6252e0c5", "The transaction number is already in use by Unapproved Invoice. Please select another one."));
						}
						else if (IsTransactionNumUsedInJobInvoicing)
						{
							Parent.AH_TransactionNumInfo.AddError(Res.GetString("a9acbc38-6459-4d35-b3db-cdefc22b20b3", "The transaction number is already in use on Job Invoicing. Please select another one."));
						}
						else if (!Parent.AH_TransactionNumInfo.HasErrors() && Parent.IsReverseTransaction && !Parent.IsInDatabase &&
							((IBusinessObjectInternals)Parent).ParentCollections.Length > 0)
						{
							ZQuery filter = new ZQuery(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
							filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, Parent.AH_TransactionNum);
							filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, Parent.AH_TransactionType);
							filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, Parent.AH_Ledger);
							filter.AddToFilter(AccTransactionHeaderSchema.AH_OH, Parent.AH_OH);
							filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, Parent.AH_GC);
							filter.FetchOnlyFromLocalCache = true;
							foreach (BusinessObjectCollection parentCollection in ((IBusinessObjectInternals)Parent).ParentCollections)
							{
								foreach (BusinessObject bizo in parentCollection)
								{
									TransactionHeader transaction = bizo as TransactionHeader;
									if (transaction != null && transaction.PK != Parent.PK && transaction.AH_TransactionNum == Parent.AH_TransactionNum &&
										transaction.AH_TransactionType == Parent.AH_TransactionType && transaction.AH_Ledger == Parent.AH_Ledger
										 && transaction.AH_OH == Parent.AH_OH && transaction.AH_GC == Parent.AH_GC)
									{
										Parent.AH_TransactionNumInfo.AddError(Res.GetString("8EDF0928-352E-4d45-AE5E-EE6497D3F5E0",
											"The transaction number '{0}' is already in use by another transaction in the collection.", Parent.AH_TransactionNum));
										break;
									}
								}
								if (Parent.AH_TransactionNumInfo.HasErrors())
								{
									break;
								}
							}
						}
					}
				}
			}
		}

		protected override void CheckAH_PostDate()
		{
			if (Parent is InvoicingBase invoicingBase)
			{
				var reversingCannotPost = invoicingBase.RelatedApportionmentReversings.FirstOrDefault(x => !x.CanReverseTransaction);
				if (reversingCannotPost != null)
				{
					Parent.AH_PostDateInfo.AddError(Res.GetString("17D6ED28-BF70-43CC-AF19-2D762DCD18AB", "Unable to post one or more journals for multi period apportionment.") + " " + reversingCannotPost.CantReverseErrorMessage);
				}
			}

			CheckAH_PostDateReversalAllowed();

			if (!Parent.AH_PostDateInfo.ReadOnly || Parent.Factory.HasContext(BusinessContext.ReverseDateForm))
			{
				base.CheckAH_PostDate();
			}
		}

		protected override void CheckAH_PostDateNotInPast()
		{
			if (!Parent.AH_PostDateInfo.HasErrors())
			{
				TransactionHeader originalTransaction = Parent.OriginalTransaction;
				if (originalTransaction != null)
				{
					if (originalTransaction != null && Parent.AH_PostDate.Date < originalTransaction.AH_PostDate.Date)
					{
						Parent.AH_PostDateInfo.AddError(Res.GetString("4ca86f91-299b-45be-9434-7ed3f1188809", "Reversing post date cannot be before the original post date of '{0}'.", originalTransaction.AH_PostDate.ToShortDateString()));
					}
				}

				base.CheckAH_PostDateNotInPast();
			}
		}

		protected virtual void CheckAH_PostDateReversalAllowed()
		{
			if (!Parent.AH_PostDateInfo.HasErrors())
			{
				if (Parent.OriginalTransaction is TransactionHeaderWithLines originalTransactionWithLines)
				{
					var postDateExceeds = IndiaGSTReversalHelper.CheckIsExceedIndiaFinancialYearEndDatePeriod(
							originalTransactionWithLines.AH_PostDate,
							Parent.AH_PostDate
						);
					var taxIDConstraintMatch = originalTransactionWithLines.Lines
						.Any(line => line is InvoicingLineBase invoicingLine
							&& invoicingLine.TaxRate != null
							&& IndiaGSTReversalHelper.CheckIsConstraintTaxID(invoicingLine.TaxRate)
						);
					if (postDateExceeds && taxIDConstraintMatch)
					{
						Parent.AH_PostDateInfo.AddError(IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_ReverseINV);
					}
				}
			}
		}

		protected override void CheckAH_ExchangeRate()
		{
			// Do Nothing
		}

		protected override void CheckAH_OH()
		{
			// Do Nothing
		}

		protected override bool ShouldValidateBranchDepartmentCombinationForParentInDatabase
		{
			get
			{
				return true;
			}
		}

		bool IsTransactionNumUsedInJobInvoicing
		{
			get
			{
				bool result = false;
				if (!Parent.AH_TransactionNum.IsEmpty)
				{
					ZQuery filter = new ZQuery(JobChargeSchema.JR_APInvoiceNum, Parent.AH_TransactionNum);
					filter.AddToFilter(JobChargeSchema.JR_OH_CostAccount, Parent.AH_OH);
					filter.AddToFilter(JobChargeSchema.JR_LocalCostAmt, SQLComparisonOperator.LessThan, 0);
					result = Parent.Factory.GetCachedReadOnlyFactory().LoadTop1<JobCharge>(filter) != null;
				}
				return result;
			}
		}

		protected new TransactionHeader Parent
		{
			get { return base.Parent; }
		}

		protected override INotificationType NotificationTypeForBranchDepartmentCombination
		{
			get
			{
				return CargoWise.EntityFramework.NotificationType.Warning;
			}
		}
	}
}
