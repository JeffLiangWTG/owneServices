using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class TransactionWithOverriddenBranchAndDepartment : NonPersistentBusinessObject, IObsoleteValidation
	{
		public TransactionWithOverriddenBranchAndDepartment(InvoicingBase invoice)
			: base(invoice.Factory)
		{
			Invoice = invoice;
			if (!Invoice.HasContext(BusinessContext.OverrideTransactionBranchAndDepartment))
			{
				originalAH_GB = AH_GB = ObjectFactory.Get<IAccountingDependencyFactory>().GetBranchLevelPostingHelper().GetTransactionHeaderBranchForOverrideBranchAndDepartment(Invoice);
			}
			else
			{
				originalAH_GB = AH_GB = (ZGuid)Invoice.AH_GBInfo.OriginalValue;
			}

			originalAH_GE = AH_GE = (ZGuid)Invoice.AH_GEInfo.OriginalValue;
		}

		readonly InvoicingBase Invoice;
		readonly ZGuid originalAH_GB;
		readonly ZGuid originalAH_GE;

		public AccTransactionHeaderLookups Lookups
		{
			get { return Invoice.Lookups; }
		}

		[RelatedBusinessObject("Header")]
		[List("Lookups.Headers")]
		public ZGuid AH_OH
		{
			get { return Invoice.AH_OH; }
		}

		public OrgHeader Header
		{
			get { return Factory.Load<OrgHeader>(AH_OH); }
		}

		public ZString HeaderFullName
		{
			get { return Invoice.HeaderFullName; }
		}

		public ZString AH_TransactionType
		{
			get { return Invoice.AH_TransactionType; }
		}

		public ZString AH_TransactionCategory
		{
			get { return Invoice.AH_TransactionCategory; }
		}

		public ZString AH_TransactionNum
		{
			get { return Invoice.AH_TransactionNum; }
		}

		[List("Lookups.Jobs")]
		public ZGuid AH_JH
		{
			get { return Invoice.AH_JH; }
		}

		[List("Lookups.TransactionCurrencies")]
		public ZString AH_RX_NKTransactionCurrency
		{
			get { return Invoice.AH_RX_NKTransactionCurrency; }
		}

		public ZDecimal AH_OSTotalAmount
		{
			get { return Invoice.AH_OSTotalAmount; }
		}

		public ZString AH_Desc
		{
			get { return Invoice.AH_Desc; }
		}

		public ZDecimal AH_OSTaxAmount
		{
			get { return Invoice.AH_OSTaxAmount; }
		}

		public ZDecimal AH_OSExTaxAmount
		{
			get { return Invoice.AH_OSExTaxAmount; }
		}

		[RelatedBusinessObject("Branch")]
		[List("Lookups.Branches")]
		public ZGuid AH_GB
		{
			get { return branchPK; }
			set
			{
				SetNonPersistentPropertyValue(AH_GBInfo, ref branchPK, value);
				ValidateAH_GB();
			}
		}
		ZGuid branchPK;

		public ZPropertyInfo AH_GBInfo
		{
			get { return GetZPropertyInfo(nameof(AH_GB)); }
		}

		public GlbBranch Branch
		{
			get { return Factory.Load<GlbBranch>(AH_GB); }
		}

		[RelatedBusinessObject("Department")]
		[List("Lookups.Departments")]
		public ZGuid AH_GE
		{
			get { return departmentPK; }
			set
			{
				SetNonPersistentPropertyValue(AH_GEInfo, ref departmentPK, value);
				ValidateAH_GE();
			}
		}
		ZGuid departmentPK;

		public ZPropertyInfo AH_GEInfo
		{
			get { return GetZPropertyInfo(nameof(AH_GE)); }
		}

		public GlbDepartment Department
		{
			get { return Factory.Load<GlbDepartment>(AH_GE); }
		}

		public void AppendChange()
		{
			Invoice.AH_GE = AH_GE;
			Invoice.AH_GB = AH_GB;
		}

		public void CancelChange()
		{
			AH_GE = originalAH_GE;
			AH_GB = originalAH_GB;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			//We need to clear any existing row error. If not, then OnFactorySaving will not be called.
			if (originalAH_GE != AH_GE)
			{
				Invoice.ClearRowNotificationsContaining(IntercompanyTransactionImportHelper.GetErrorMessageWhenTransactionLinesHaveInvalidDepartment());
			}
			ValidateAH_GB();
			ValidateAH_GE();
		}

		protected override void OnFactorySaving()
		{
			//Looks like there is no guarantee that RunPreSaveValidation will be called before coming here. Unit test "TestOverrdingBranchAndDepartmentSavingProcessRemovesInvalidDepartmentRowErrorMessageFirst" covers it.
			if (originalAH_GE != AH_GE)
			{
				Invoice.ClearRowNotificationsContaining(IntercompanyTransactionImportHelper.GetErrorMessageWhenTransactionLinesHaveInvalidDepartment());
			}

			CreateLogAfterOverridingBranchAndDepartment();
		}
		protected override void OnFactorySaved(bool saveSucceeded)
		{
			if (saveSucceeded && Invoice.HasContext(BusinessContext.OverrideTransactionBranchAndDepartment))
			{
				Invoice.RemoveContext(BusinessContext.OverrideTransactionBranchAndDepartment);
			}
		}

		void ValidateAH_GB()
		{
			AH_GBInfo.ClearAllNotifications();
			if (!IsValidationSuspended)
			{
				MandatoryValidation.CheckEntered(AH_GBInfo);
				var validBranchQuery = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
				validBranchQuery.AddToFilter(new ZQuery(GlbBranchSchema.GB_IsActive, SQLComparisonOperator.Equal, true));
				var validBranches = new GlbBranchCollection(Factory, validBranchQuery);

				if (Invoice.EnforceBranchLevelPostingRegistryItem != null && Invoice.EnforceBranchLevelPostingRegistryItem.Value.EnableBranchLevelPosting)
				{
					var branches = Invoice.Lines.Cast<DependentTransactionLine>().Select(x => x.AL_GB).ToHashSet();
					branches.Add(AH_GB);

					var isValid = BranchLevelPostingHelper.DoesAllBranchesBelongToSamePostingGroup(Invoice.EnforceBranchLevelPostingRegistryItem, branches);
					if (!isValid)
					{
						AH_GBInfo.AddError(Res.GetString("ad6af2ab-c47d-45b5-a363-1104dd564884", @"Branch is invalid as the branch set in the charges do not belong to the same Posting Group as that of the header."));
					}
				}

				ListValidation.ErrorIfInvalidPK(AH_GBInfo, validBranches);
			}
		}

		void ValidateAH_GE()
		{
			AH_GEInfo.ClearAllNotifications();
			if (!IsValidationSuspended)
			{
				if (GlbBranchCombinationValidation.ShouldValidateCombination(Branch))
				{
					var error = GlbBranchCombinationValidation.CheckBranchDepartmentCombination(Branch, Department);
					if (!String.IsNullOrEmpty(error))
					{
						AH_GEInfo.AddError(error);
					}
				}

				MandatoryValidation.CheckEntered(AH_GEInfo);

				var validDepartmentQuery = new ZQuery(GlbDepartmentSchema.GE_IsActive, SQLComparisonOperator.Equal, true);
				validDepartmentQuery.AddToFilter(new ZQuery(GlbDepartmentSchema.GE_IsActive, SQLComparisonOperator.Equal, true));
				var validDepartments = new GlbDepartmentCollection(Factory, validDepartmentQuery);
				ListValidation.ErrorIfInvalidPK(AH_GEInfo, validDepartments);
			}
		}

#if DEBUG
		public void ValidateAH_GE_ForTestOnly()
		{
			ValidateAH_GE();
		}

		public void CreateLogAfterOverridingBranchAndDepartment_ForTestOnly()
		{
			CreateLogAfterOverridingBranchAndDepartment();
		}
#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI008:LogReferenceValuesInEnglishOnly", Justification = "Baseline")]
		void CreateLogAfterOverridingBranchAndDepartment()
		{
			if (Invoice.HasContext(BusinessContext.OverrideTransactionBranchAndDepartment))
			{
				var getLogText = new Func<string, string, string, string>((type, oldValue, newValue) => Res.GetString("cd277c30-3297-492a-b6e8-ff4309a8c556", "Transaction {0} Edited: From '{1}' to '{2}'", type, oldValue, newValue));

				if (AH_GB != originalAH_GB)
				{
					var oldBranchCode = Factory.Load<GlbBranch>(originalAH_GB)?.GB_Code;
					var newBranchCode = Branch?.GB_Code;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Invoice.Logs.AddNew(Events.EditedARecord, getLogText(Res.GetString("b66d82be-5779-45c9-89e0-3a58195b6abf", "Branch"), oldBranchCode, newBranchCode));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}

				if (AH_GE != originalAH_GE)
				{
					var oldDeptCode = Factory.Load<GlbDepartment>(originalAH_GE)?.GE_Code;
					var newDeptCode = Department?.GE_Code;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Invoice.Logs.AddNew(Events.EditedARecord, getLogText(Res.GetString("cce65316-4904-41e9-8639-d7a818ffc8f7", "Department"), oldDeptCode, newDeptCode));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}
	}
}
