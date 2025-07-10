using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	partial class InvoicingBase
	{
		public TransactionPendingAllocationApprovalRequest AllocationApprovalRequest
		{
			get
			{
				if (!isAllocationApprovalRequestSet)
				{
					var query = new ZQuery(new TransactionPendingAllocationApprovalRequestCollection(Factory, new ZQuery(GenApprovalRequestSchema.XP_ParentID, PK)).CompleteFilter);
					query.OrderBy = GenApprovalRequestSchema.XP_SystemCreateTimeUtc.Name + " DESC";

					allocationApprovalRequest = Factory.LoadTop1<TransactionPendingAllocationApprovalRequest>(query);
					isAllocationApprovalRequestSet = true;

					SetOrganizationForImportedXMLMatchingRules();
				}

				return allocationApprovalRequest;
			}
		}
		TransactionPendingAllocationApprovalRequest allocationApprovalRequest;
		bool isAllocationApprovalRequestSet;

		public ZString ImportedCreditor
		{
			get
			{
				var result = ZString.Empty;
				var universalTransaction = GetUniversalTransaction();
				if (universalTransaction != null)
				{
					result = universalTransaction.Creditor;
				}

				return result;
			}
		}

		public ZString ImportedCreditorXmlCode
		{
			get
			{
				var result = ZString.Empty;
				var universalTransaction = GetUniversalTransaction();
				if (universalTransaction != null)
				{
					result = universalTransaction.CreditorSource;
				}

				return result;
			}
		}

		internal bool ShouldCreateOrganisationCodeMatchingRule => !ImportedCreditorXmlCode.IsEmpty && Header != null && Header.OH_Code != ImportedCreditor && AccountingConfigurationRegistry.Instance.EnableAutomaticOrganizationCodeMappingForUnallocatedInvoices.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

		public virtual bool IsImportedFromUniversalXML => (IsIncompleteInvoice || IsCompletingInvoice || IsAllocatingInvoice || IsPosted) && AllocationApprovalRequest != null && AllocationApprovalRequest.HasUniversalTransaction;

		bool IsImportedXMLMatchingRulesDataExists => SubmittedFromInvoicingForm && (IsIncompleteInvoice || IsCompletingInvoice || IsAllocatingInvoice) && AllocationApprovalRequest != null && AllocationApprovalRequest.HasUniversalTransaction;

		bool IsImportedXMLMatchingRulesCreationAllowed => AH_Ledger != LedgerTypes.IncompleteTransactions && IsImportedXMLMatchingRulesDataExists;

		internal UniversalTransactionWrapper GetUniversalTransaction()
		{
			UniversalTransactionWrapper result = null;
			if (IsImportedXMLMatchingRulesDataExists)
			{
				result = AllocationApprovalRequest.PostingDetails.UniversalTransaction;
				result.UpdateMappedCodes();
			}

			return result;
		}

		internal bool IsCrossLedgerImportFromXML => AllocationApprovalRequest != null && AllocationApprovalRequest.HasUniversalTransaction && AllocationApprovalRequest.PostingDetails.IsCrossLedgerImportFromXML;

		void SetOrganizationForImportedXMLMatchingRules(bool runOrgValidation = true)
		{
			if (!SetOrganizationForImportedXMLMatchingRulesSuspender.IsSuspended)
			{
				using (SetOrganizationForImportedXMLMatchingRulesSuspender.GetSuspender())
				{
					UniversalTransactionWrapper universalTransaction;
					if (isAllocationApprovalRequestSet && (universalTransaction = GetUniversalTransaction()) != null)
					{
						if (IsCrossLedgerImportFromXML)
						{
							if (universalTransaction.SetTransactionOrganisationAndUpdateMatchingForCrossLedgerOnly(Header))
							{
								if (runOrgValidation)
								{
									Validation.ValidateAH_OH();
								}
								Lines.Cast<InvoicingLineBase>().ToList().ForEach(line =>
								{
									(line.Validation as InvoicingLineBaseValidation)?.ValidateGenericCharge();
									line.ImportedChargeCodeInfo.RefreshBinding();
								});
							}
						}
						else if (runOrgValidation)
						{
							Validation.ValidateAH_OH();
						}
					}
				}
			}
		}

		void UpdateImportedXMLMatchingRules()
		{
			if (!IsImportedXMLMatchingRulesCreationAllowed)
			{
				return;
			}

			var universalTransaction = GetUniversalTransaction();
			if (ShouldCreateOrganisationCodeMatchingRule)
			{
				universalTransaction.UpdateCreditorCodeMapping(Header);
			}
			else
			{
				universalTransaction.DeleteTempOrgMappingRule();
			}

			var linesToUpdateChargeCodeMatchingRules = from line in Lines.Cast<InvoicingLineBase>()
													   where !line.ImportedChargeCodeXmlCode.IsEmpty && line.AL_AC.IsValid
													   group line by line.ImportedChargeCodeXmlCode into linesByXMLChargeCode
													   where linesByXMLChargeCode.Select(x => x.AL_AC).Distinct().Take(2).Count() == 1
													   let lineToProcess = linesByXMLChargeCode.First()
													   where lineToProcess.ShouldCreateChargeCodeMatchingRule
													   select lineToProcess;
			linesToUpdateChargeCodeMatchingRules.ToList().ForEach(line => universalTransaction.UpdateChargeCodeMapping(line.ChargeCode, line.ImportedChargeCodeXmlCode));
		}

		FunctionalitySuspender SetOrganizationForImportedXMLMatchingRulesSuspender => setOrganizationForImportedXMLMatchingRulesSuspender ??
			(setOrganizationForImportedXMLMatchingRulesSuspender = new FunctionalitySuspender());
		FunctionalitySuspender setOrganizationForImportedXMLMatchingRulesSuspender;
	}
}
