using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class InvoiceBaseValidation
	{
		protected virtual void CheckOriginalTransactionReference()
		{
			if (Parent.ShouldShowOriginalInvoiceReferenceFields)
			{
				if (Parent.IsCreditNoteComplianceDocumentConfigurationEnabled
					&& Parent.AH_Ledger == LedgerTypes.AccountsReceivable
					&& (Parent.OriginalReferenceTransaction?.AH_TransactionType ?? ZString.Empty) == TransactionTypes.Invoice
					&& Parent.OriginalReferenceTransaction.Lines.All(x => ((InvoicingLineBase)x).AL_AT.IsValid))
				{
					var allComplianceDocumentHeaders = Parent.OriginalReferenceTransaction.GetAllTransactionGeneratedComplianceDocument();
					if (allComplianceDocumentHeaders.Length == 0 || allComplianceDocumentHeaders.Any(currentComplianceDocumentHeader => currentComplianceDocumentHeader.ADH_DocumentNumber.IsEmpty))
					{
						InvoiceTransaction.OriginalTransactionReferenceInfo.AddError(Res.GetString("27F09189-A5D7-4473-B230-F1F0B5583974", "Please create compliance document record and allocate compliance document number for the selected invoice before proceeding to raise a credit note referencing it."));
					}

					if (allComplianceDocumentHeaders.Any(currentComplianceDocumentHeader => currentComplianceDocumentHeader.ADH_ComplianceSubType == TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE && (currentComplianceDocumentHeader.EInvoicingStatus.IsEmpty || currentComplianceDocumentHeader.EInvoicingStatus != Constants.EInvoicingPivotState.Succeed)))
					{
						InvoiceTransaction.OriginalTransactionReferenceInfo.AddError(Res.GetString("712D3346-4013-4A2E-B1A6-7E8CBD1258FA", "Credit Note can only be created after the Original TXE documents have been successfully uploaded (i.e., E-Reporting Status = SUC)."));
					}
				}

				if (!Parent.OriginalTransactionReferenceInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidPK(Parent.OriginalTransactionReferenceInfo);
				}

				CheckOriginalInvoiceDetails(Parent.OriginalTransactionReferenceInfo);
			}

			if (!Parent.OriginalTransactionReferenceInfo.HasErrors()
				&& Parent.ShouldShowOriginalInvoiceReferenceFields
				&& Parent.AreOriginalTransactionReferenceFieldsMandatory)
			{
				if (!Parent.OriginalTransactionReferenceInfo.HasErrors() &&
					Parent.OriginalTransactionReference.IsEmpty &&
					(!Parent.AH_OriginalTransactionNum.IsEmpty || !Parent.AH_OriginalInvoiceDate.IsEmpty))
				{
					Parent.OriginalTransactionReferenceInfo.AddError(Res.GetString("0fe5f4d4-dd49-4d8a-82c1-3c99ec4ef6e9", "Original Reference must be filled in if Original Invoice number or date have been filled in."));
				}

				if (!Parent.OriginalTransactionReferenceInfo.HasErrors())
				{
					var bothEmptyMessage = ValidateOriginalReferenceAndReferenceDateNotEmptyAtSameTime();
					if (!bothEmptyMessage.IsEmpty)
					{
						Parent.OriginalTransactionReferenceInfo.AddError(bothEmptyMessage);
					}
				}

				if (!Parent.OriginalTransactionReferenceInfo.HasErrors())
				{
					var neitherFilledMessage = ValidateOriginalReferenceAndReferenceDateNotFilledAtSameTime();
					if (!neitherFilledMessage.IsEmpty)
					{
						Parent.OriginalTransactionReferenceInfo.AddError(neitherFilledMessage);
					}
				}
			}
		}

		protected override void CheckAH_OriginalReferenceStartDate()
		{
			base.CheckAH_OriginalReferenceStartDate();

			if (Parent.ShouldShowOriginalInvoiceReferenceDatesFields && Parent.AreOriginalTransactionReferenceFieldsMandatory)
			{
				if (!Parent.AH_OriginalReferenceStartDateInfo.HasErrors() && !Parent.AH_OriginalReferenceEndDate.IsEmpty)
				{
					MandatoryValidation.CheckEntered(Parent.AH_OriginalReferenceStartDateInfo);
				}

				if (!Parent.AH_OriginalReferenceStartDateInfo.HasErrors())
				{
					var referenceDateNotInFutureMessage = ValidateReferenceTimeNotInFuture(Parent.AH_OriginalReferenceStartDate);
					if (!referenceDateNotInFutureMessage.IsEmpty)
					{
						Parent.AH_OriginalReferenceStartDateInfo.AddError(referenceDateNotInFutureMessage);
					}
				}

				if (!Parent.AH_OriginalReferenceStartDateInfo.HasErrors())
				{
					var referenceEndDateNotLessThanStartDateMessage = ValidateReferenceEndDateNotLessThanStartDate(Parent.AH_OriginalReferenceStartDate, Parent.AH_OriginalReferenceEndDate);
					if (!referenceEndDateNotLessThanStartDateMessage.IsEmpty)
					{
						Parent.AH_OriginalReferenceStartDateInfo.AddError(referenceEndDateNotLessThanStartDateMessage);
					}
				}

				if (!Parent.AH_OriginalReferenceStartDateInfo.HasErrors())
				{
					var bothEmptyMessage = ValidateOriginalReferenceAndReferenceDateNotEmptyAtSameTime();
					if (!bothEmptyMessage.IsEmpty)
					{
						Parent.AH_OriginalReferenceStartDateInfo.AddError(bothEmptyMessage);
					}
				}

				if (!Parent.AH_OriginalReferenceStartDateInfo.HasErrors())
				{
					var neitherFilledMessage = ValidateOriginalReferenceAndReferenceDateNotFilledAtSameTime();
					if (!neitherFilledMessage.IsEmpty)
					{
						Parent.AH_OriginalReferenceStartDateInfo.AddError(neitherFilledMessage);
					}
				}
			}
		}

		protected override void CheckAH_OriginalReferenceEndDate()
		{
			base.CheckAH_OriginalReferenceEndDate();

			if (Parent.ShouldShowOriginalInvoiceReferenceDatesFields && Parent.AreOriginalTransactionReferenceFieldsMandatory)
			{
				if (!Parent.AH_OriginalReferenceEndDateInfo.HasErrors() && !Parent.AH_OriginalReferenceStartDate.IsEmpty)
				{
					MandatoryValidation.CheckEntered(Parent.AH_OriginalReferenceEndDateInfo);
				}

				if (!Parent.AH_OriginalReferenceEndDateInfo.HasErrors())
				{
					var referenceDateNotInFutureMessage = ValidateReferenceTimeNotInFuture(Parent.AH_OriginalReferenceEndDate);
					if (!referenceDateNotInFutureMessage.IsEmpty)
					{
						Parent.AH_OriginalReferenceEndDateInfo.AddError(referenceDateNotInFutureMessage);
					}
				}

				if (!Parent.AH_OriginalReferenceEndDateInfo.HasErrors())
				{
					var referenceEndDateNotLessThanStartDateMessage = ValidateReferenceEndDateNotLessThanStartDate(Parent.AH_OriginalReferenceStartDate, Parent.AH_OriginalReferenceEndDate);
					if (!referenceEndDateNotLessThanStartDateMessage.IsEmpty)
					{
						Parent.AH_OriginalReferenceEndDateInfo.AddError(referenceEndDateNotLessThanStartDateMessage);
					}
				}

				if (!Parent.AH_OriginalReferenceEndDateInfo.HasErrors())
				{
					var bothEmptyMessage = ValidateOriginalReferenceAndReferenceDateNotEmptyAtSameTime();
					if (!bothEmptyMessage.IsEmpty)
					{
						Parent.AH_OriginalReferenceEndDateInfo.AddError(bothEmptyMessage);
					}
				}

				if (!Parent.AH_OriginalReferenceEndDateInfo.HasErrors())
				{
					var neitherFilledMessage = ValidateOriginalReferenceAndReferenceDateNotFilledAtSameTime();
					if (!neitherFilledMessage.IsEmpty)
					{
						Parent.AH_OriginalReferenceEndDateInfo.AddError(neitherFilledMessage);
					}
				}
			}
		}

		protected virtual void CheckReasonCode()
		{
			ListValidation.ErrorIfInvalidCode(Parent.ReasonCodeInfo, Parent.ReasonCodes);

			if (Parent.AreOriginalTransactionReferenceFieldsMandatory && Parent.OriginalTransactionIsSet)
			{
				MandatoryValidation.CheckEntered(Parent.ReasonCodeInfo);
			}
		}

		protected virtual void CheckReasonDescription()
		{
			if (Parent.AreOriginalTransactionReferenceFieldsMandatory && Parent.OriginalTransactionIsSet)
			{
				MandatoryValidation.CheckEntered(Parent.ReasonDescriptionInfo);
			}
			if (Parent.ReasonDescription.Contains(Parent.reasonCodeDescriptionSeparator))
			{
				Parent.ReasonDescriptionInfo.AddError(Res.GetString("807dd627-367f-46fa-8c13-bca001abe47f",
					"This field should never contain the '{0}' character.", Parent.reasonCodeDescriptionSeparator));
			}
		}

		ZString ValidateReferenceEndDateNotLessThanStartDate(ZDate referenceStartDate, ZDate referenceEndDate)
		{
			var result = ZString.Empty;
			if (!referenceStartDate.IsEmpty &&
				!referenceEndDate.IsEmpty &&
				referenceStartDate > referenceEndDate)
			{
				result = Res.GetString("d3e6a374-f165-4c66-8196-085c3ee407fd", "'Reference Date From' should be less than or equal to 'Reference Date To'.");
			}

			return result;
		}

		ZString ValidateReferenceTimeNotInFuture(ZDate referenceDate)
		{
			var result = ZString.Empty;
			if (referenceDate > ZDate.Today)
			{
				result = Res.GetString("f94fc2c6-c2f4-494e-ab0f-798e62325723", "'Reference Date From' and 'Reference Date To' must be filled with dates less than or equal to current date.");
			}

			return result;
		}

		ZString ValidateOriginalReferenceAndReferenceDateNotEmptyAtSameTime()
		{
			var result = ZString.Empty;
			if (!Parent.OriginalTransactionReference.IsEmpty &&
				!Parent.AH_OriginalReferenceStartDate.IsEmpty &&
				!Parent.AH_OriginalReferenceEndDate.IsEmpty)
			{
				result = Res.GetString("93d36287-7aa6-4deb-9734-2c448456be16", "Must identify only one of the following fields, original reference or date range.");
			}

			return result;
		}

		ZString ValidateOriginalReferenceAndReferenceDateNotFilledAtSameTime()
		{
			var result = ZString.Empty;
			if (Parent.ShouldShowOriginalInvoiceReferenceDatesFields &&
				Parent.OriginalTransactionReference.IsEmpty &&
				Parent.AH_OriginalReferenceStartDate.IsEmpty &&
				Parent.AH_OriginalReferenceEndDate.IsEmpty)
			{
				result = Res.GetString("4a82666b-ee73-42d5-a5d5-465761dca70e", "Must identify an original reference or date range in this transaction.");
			}

			return result;
		}

		protected override void CheckAH_OriginalInvoiceDate()
		{
			base.CheckAH_OriginalInvoiceDate();
			CheckOriginalInvoiceDetails(Parent.AH_OriginalInvoiceDateInfo);
		}

		protected override void CheckAH_OriginalTransactionNum()
		{
			base.CheckAH_OriginalTransactionNum();
			CheckOriginalInvoiceDetails(Parent.AH_OriginalTransactionNumInfo);
		}

		void CheckOriginalInvoiceDetails(ZPropertyInfo propertyInfo)
		{
			var shouldValidate = !propertyInfo.HasErrors();
			shouldValidate = shouldValidate && propertyInfo.Value.IsEmpty;
			shouldValidate = shouldValidate && Parent.ShouldShowOriginalInvoiceReferenceFields;
			shouldValidate = shouldValidate && (OriginalInvoiceDetailsMandatoryRegistryItem != null && OriginalInvoiceDetailsMandatoryRegistryItem.Value);
			shouldValidate = shouldValidate && Parent.OriginalTransactionReference.IsEmpty;
			shouldValidate = shouldValidate && (Parent.AH_OriginalTransactionNum.IsEmpty || Parent.AH_OriginalInvoiceDate.IsEmpty);

			if (shouldValidate)
			{
				propertyInfo.AddError(OriginalInvoiceDetailsMandatoryMessage);
			}
		}

		protected virtual BooleanRegistryItem OriginalInvoiceDetailsMandatoryRegistryItem => null;

		string OriginalInvoiceDetailsMandatoryMessage
		{
			get => Res.GetString("446491b4-6edd-4ae3-b877-2ffdd699b3fa", "Original Invoice Number and Date must be recorded. This is controlled by the registry {0}.", OriginalInvoiceDetailsMandatoryRegistryItem.Location());
		}
	}
}
