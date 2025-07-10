using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina
{
	public interface IArgentinaEInvoiceHelper
	{
		ZString GetReferenceNumberInfo(AccComplianceSequence complianceSequence, ZString transactionReference);
		ZString GetReferencePrefixInfo(AccComplianceSequence complianceSequence, ZString transactionReference);
		ZString GetComplianceSequencePrefixInfo(AccComplianceSequence complianceSequence);
		string GetOriginalTransactionComplianceSubType(OriginalReference transactionOriginalReference);
		ZString GetOriginalTransactionReference(OriginalReference transactionOriginalReference);
		ZString GetComplianceNumberPrefixFromTransaction(OriginalReference transactionOriginalReference);
		ZString GetComplianceNumberFromTransaction(OriginalReference transactionOriginalReference);
		bool IsOriginalReferenceCreditOrDebitNoteTransaction(TransactionInfo transactionInfo);
		bool IsMiPymeComplianceSubType(ZString complianceSubtype);
		bool IsMiPymeDebitOrCreditNoteComplianceSubType(ZString complianceSubtype);
		string GetBankAccountCBUNumber(TransactionInfo transaction, BusinessObjectFactory factory);
		(ZDecimal totalSubjectToVAT, ZDecimal totalNotSubjectToVAT, ZDecimal totalExemptVAT) GetTotalsTaxAmountFromTransactionInfo(TransactionInfo transaction, int multiplier);
		(ZGuid branchPK, ZGuid companyPK, ZGuid deparmentPK) GetBranchCompanyAndDepartamentPKFromTransactionInfo(TransactionInfo transactionInfo, BusinessObjectFactory factory);
		string GetMiPymeOriginalTransactionIsRejectedByBuyer(TransactionInfo transaction);
		short? GetCondicionIvaReceptor(TransactionInfo transaction);
	}

	class ArgentinaEInvoiceHelper : IArgentinaEInvoiceHelper
	{
		ZString IArgentinaEInvoiceHelper.GetReferenceNumberInfo(AccComplianceSequence complianceSequence, ZString transactionReference)
		{
			var complianceNumber = ZString.Empty;

			if (complianceSequence != null && !complianceSequence.XD_Prefix.IsEmpty && transactionReference.StartsWith(complianceSequence.XD_Prefix))
			{
				complianceNumber = transactionReference.SubstringSafe(complianceSequence.XD_Prefix.Length);
			}
			else
			{
				complianceNumber = transactionReference.RemoveNonNumericCharacters().SubstringSafe(5);
			}

			return complianceNumber;
		}

		ZString IArgentinaEInvoiceHelper.GetReferencePrefixInfo(AccComplianceSequence complianceSequence, ZString transactionReference)
		{
			return complianceSequence != null && !complianceSequence.XD_Prefix.IsEmpty ? complianceSequence.XD_Prefix.RemoveNonNumericCharacters() :
				transactionReference.RemoveNonNumericCharacters().SubstringSafe(0, 5);
		}

		ZString IArgentinaEInvoiceHelper.GetComplianceSequencePrefixInfo(AccComplianceSequence complianceSequence)
		{
			return complianceSequence != null && !complianceSequence.XD_Prefix.IsEmpty ? complianceSequence.XD_Prefix : ZString.Empty;
		}

		string IArgentinaEInvoiceHelper.GetOriginalTransactionComplianceSubType(OriginalReference transactionOriginalReference)
		{
			if (transactionOriginalReference != null)
			{
				return transactionOriginalReference.OriginalTransactionComplianceSubType.HasValue ?
							transactionOriginalReference.OriginalTransactionComplianceSubType.GetValueOrDefault(ZString.Empty) :
							transactionOriginalReference.OriginalTransactionNumber.GetValueOrDefault(ZString.Empty).SubstringSafe(0, 3);
			}

			return ZString.Empty;
		}

		ZString IArgentinaEInvoiceHelper.GetOriginalTransactionReference(OriginalReference transactionOriginalReference)
		{
			if (transactionOriginalReference != null)
			{
				return transactionOriginalReference.OriginalTransactionReference.HasValue ?
					transactionOriginalReference.OriginalTransactionReference.GetValueOrDefault(ZString.Empty) :
					!transactionOriginalReference.OriginalTransactionComplianceSubType.HasValue ?
					transactionOriginalReference.OriginalTransactionNumber.GetValueOrDefault(ZString.Empty).SubstringSafe(3) :
					ZString.Empty;
			}

			return ZString.Empty;
		}

		ZString IArgentinaEInvoiceHelper.GetComplianceNumberPrefixFromTransaction(OriginalReference transactionOriginalReference)
		{
			return ((IArgentinaEInvoiceHelper)this).GetOriginalTransactionReference(transactionOriginalReference).SubstringSafe(0, 5);
		}

		ZString IArgentinaEInvoiceHelper.GetComplianceNumberFromTransaction(OriginalReference transactionOriginalReference)
		{
			return ((IArgentinaEInvoiceHelper)this).GetOriginalTransactionReference(transactionOriginalReference).SubstringSafe(5);
		}

		bool IArgentinaEInvoiceHelper.IsOriginalReferenceCreditOrDebitNoteTransaction(TransactionInfo transactionInfo)
		{
			return (transactionInfo.OriginalReference != null &&
				ArgentinaConstants.DebitOrCreditNoteComplianceSubTypeList.Contains(transactionInfo.ComplianceSubType.GetValueOrDefault(ZString.Empty)));
		}

		bool IArgentinaEInvoiceHelper.IsMiPymeComplianceSubType(ZString complianceSubtype)
		{
			var result = false;

			if (!complianceSubtype.IsEmpty)
			{
				result = ArgentinaConstants.MiPymeComplianceSubTypeList.Contains(complianceSubtype);
			}

			return result;
		}

		bool IArgentinaEInvoiceHelper.IsMiPymeDebitOrCreditNoteComplianceSubType(ZString complianceSubtype)
		{
			var result = false;

			if (!complianceSubtype.IsEmpty)
			{
				result = ArgentinaConstants.MiPymeDebitOrCreditNoteComplianceSubTypeList.Contains(complianceSubtype);
			}

			return result;
		}

		string IArgentinaEInvoiceHelper.GetBankAccountCBUNumber(TransactionInfo transaction, BusinessObjectFactory factory)
		{
			var cbuNumber = ZString.Empty;

			if (transaction.OrganizationAddress != null && (transaction.OrganizationAddress?.OrganizationCode).HasValue && transaction.OrganizationAddress.OrganizationCode.Value != ZString.Empty)
			{
				OrgHeader organization = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, transaction.OrganizationAddress.OrganizationCode.Value);

				if (organization != null && transaction.Branch != null && !transaction.Branch.Code.GetValueOrDefault(ZString.Empty).IsEmpty)
				{
					GlbBranch branch = factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, transaction.Branch.Code.GetValueOrDefault(ZString.Empty));

					if (branch != null)
					{
						var bankAccount = AccBankAccount.GetDefaultReceiptBankAccountForDebtor(organization.PK, transaction.OSCurrency?.Code ?? ZString.Empty, branch, factory);

						cbuNumber = bankAccount != null ? bankAccount.AB_FullAccountNumber : cbuNumber;
					}
				}
			}

			return cbuNumber;
		}

		(ZDecimal totalSubjectToVAT, ZDecimal totalNotSubjectToVAT, ZDecimal totalExemptVAT) IArgentinaEInvoiceHelper.GetTotalsTaxAmountFromTransactionInfo(TransactionInfo transaction, int multiplier)
		{
			var importeGravado = ZDecimal.Zero;
			var importeNoGravado = ZDecimal.Zero;
			var importeExento = ZDecimal.Zero;

			if (transaction?.PostingJournalCollection != null && transaction.PostingJournalCollection.Any())
			{
				foreach (var line in transaction.PostingJournalCollection)
				{
					var taxTypeCode = line?.VATTaxID?.TaxType?.Code;

					switch (taxTypeCode)
					{
						case AccTaxRate.Types.Rated:
						case AccTaxRate.Types.CapitalRated:
							importeGravado += line.OSAmount.Value * multiplier;
							break;
						case AccTaxRate.Types.ExcludedFromTheTaxBase:
						case AccTaxRate.Types.NotReportable:
							importeNoGravado += line.OSAmount.Value * multiplier;
							break;
						case AccTaxRate.Types.Exempt:
							importeExento += line.OSAmount.Value * multiplier;
							break;
					}
				}
			}

			return (importeGravado, importeNoGravado, importeExento);
		}

		(ZGuid branchPK, ZGuid companyPK, ZGuid deparmentPK) IArgentinaEInvoiceHelper.GetBranchCompanyAndDepartamentPKFromTransactionInfo(TransactionInfo transactionInfo, BusinessObjectFactory factory)
		{
			ZGuid branchPK, companyPK, departmentPK;
			branchPK = companyPK = departmentPK = ZGuid.Empty;

			if (transactionInfo.Branch != null)
			{
				var branch = factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, transactionInfo.Branch.Code.GetValueOrDefault());
				if (branch != null)
				{
					branchPK = branch.PK;
					companyPK = branch.GB_GC;
				}
			}

			if (transactionInfo.Department != null)
			{
				var department = factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, transactionInfo.Department.Code.GetValueOrDefault());
				if (department != null)
				{
					departmentPK = department.PK;
				}
			}

			return (branchPK, companyPK, departmentPK);
		}

		string IArgentinaEInvoiceHelper.GetMiPymeOriginalTransactionIsRejectedByBuyer(TransactionInfo transaction)
		{
			var originalTransactionAmendingReversingReason = transaction.OriginalReference?.OriginalTransactionAmendingReversingReason?.Code ?? ZString.Empty;

			return originalTransactionAmendingReversingReason == "MIR" ? "S" : "N";
		}

		short? IArgentinaEInvoiceHelper.GetCondicionIvaReceptor(TransactionInfo transaction)
		{
			var registrationNumber = transaction.OrganizationAddress?.RegistrationNumberCollection?.FirstOrDefault(r =>
				(r.CountryOfIssue?.Code ?? ZString.Empty) == CountryCodes.Argentina &&
				ArgentinaConstants.ArcaIvaCategories.ContainsKey(r.Type?.Code ?? ZString.Empty))?.Type.Code;

			return registrationNumber.HasValue ? ArgentinaConstants.ArcaIvaCategories[registrationNumber] : null;
		}
	}
}
