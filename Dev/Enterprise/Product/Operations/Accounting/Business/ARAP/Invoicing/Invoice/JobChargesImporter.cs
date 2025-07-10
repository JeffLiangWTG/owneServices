using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class JobChargesImporter : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string IncludeChargesForAllOtherCreditors = "IncludeChargesForAllOtherCreditors";
			public const string IncludeChargesForCreditorsWithTheSameAPSettlementGroup = "IncludeChargesForCreditorsWithTheSameAPSettlementGroup";
		}

		#endregion

		public JobChargesImporter(InvoicingLineBase sendingLine)
			: base(sendingLine.Factory)
		{
			this.SendingLine = sendingLine;

			IncludeChargesForAllOtherCreditors = AccountingConfigurationRegistry.Instance.IncludeChargesForAllOtherCreditors.Value;
			IncludeChargesForCreditorsWithTheSameAPSettlementGroup = AccountingConfigurationRegistry.Instance.IncludeChargesForCreditorsWithTheSameAPSettlementGroup.Value;
		}

		readonly InvoicingLineBase SendingLine;

		public IndependentChargeCollectionForBinding JobChargesCollection
		{
			get
			{
				if (JobChargesCollection_innerValue == null)
				{
					JobChargesCollection_innerValue = new IndependentChargeCollectionForBinding(Factory);
					if (!LoadingJobCharge)
					{
						ReloadJobCharges();
					}
				}
				return JobChargesCollection_innerValue;
			}
		}
		IndependentChargeCollectionForBinding JobChargesCollection_innerValue;

		#region IncludeChargesForAllOtherCreditors

		public ZBool IncludeChargesForAllOtherCreditors
		{
			get { return IncludeChargesForAllOtherCreditors_innerValue; }
			set
			{
				if (SetNonPersistentPropertyValue(IncludeChargesForAllOtherCreditorsInfo, ref IncludeChargesForAllOtherCreditors_innerValue, value))
				{
					IncludeChargesForCreditorsWithTheSameAPSettlementGroup_innerValue = false;
					IncludeChargesForCreditorsWithTheSameAPSettlementGroupInfo.RefreshBinding();
					ReloadJobCharges();
				}
			}
		}

		public ZPropertyInfo IncludeChargesForAllOtherCreditorsInfo
		{
			get { return GetZPropertyInfo(Schema.IncludeChargesForAllOtherCreditors); }
		}

		ZBool IncludeChargesForAllOtherCreditors_innerValue;

		#endregion

		#region IncludeChargesForCreditorsWithTheSameAPSettlementGroup

		public ZBool IncludeChargesForCreditorsWithTheSameAPSettlementGroup
		{
			get { return IncludeChargesForCreditorsWithTheSameAPSettlementGroup_innerValue; }
			set
			{
				if (SetNonPersistentPropertyValue(IncludeChargesForCreditorsWithTheSameAPSettlementGroupInfo, ref IncludeChargesForCreditorsWithTheSameAPSettlementGroup_innerValue, value))
				{
					IncludeChargesForAllOtherCreditors_innerValue = false;
					IncludeChargesForAllOtherCreditorsInfo.RefreshBinding();
					ReloadJobCharges();
				}
			}
		}

		public ZPropertyInfo IncludeChargesForCreditorsWithTheSameAPSettlementGroupInfo
		{
			get { return GetZPropertyInfo(Schema.IncludeChargesForCreditorsWithTheSameAPSettlementGroup); }
		}

		ZBool IncludeChargesForCreditorsWithTheSameAPSettlementGroup_innerValue;

		#endregion

		public void SetJobChargesToImport(IEnumerable<Charge> jobChargesToImport)
		{
			if (SendingLine != null && SendingLine.InvoiceBase != null && SendingLine.InvoiceBase is InvoicingBase)
			{
				if (jobChargesToImport != null && jobChargesToImport.Any())
				{
					//Cash Advance Job Charge Validation
					SendingLine.InvoiceBase.ImportJobChargesIntoInvoice(jobChargesToImport, SendingLine);
				}
			}
		}

		bool LoadingJobCharge;

		void ReloadJobCharges()
		{
			try
			{
				LoadingJobCharge = true;
				ZQuery jobChargesFilter = null;
				if (SendingLine != null && SendingLine.Job != null)
				{
					jobChargesFilter = new ZQuery(JobChargeSchema.JR_JH, SQLComparisonOperator.Equal, SendingLine.Job.PK);
					jobChargesFilter.AddToFilter(JoinCondition.And, JobChargeSchema.JR_E6, SQLComparisonOperator.Equal, null);
					jobChargesFilter.AddToFilter(JoinCondition.And, JobChargeSchema.JR_LocalCostAmt, AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.Value ?
						SQLComparisonOperator.NotEqual : SQLComparisonOperator.GreaterThan, 0);

					#region JobChargesCostAccountFilter

					ZQuery jobChargesCostAccountFilter = new ZQuery();
					if (!IncludeChargesForAllOtherCreditors && SendingLine.InvoiceBase != null && SendingLine.InvoiceBase.AH_OH.IsValid)
					{
						jobChargesCostAccountFilter.AddToFilter(new ZQuery(JobChargeSchema.JR_OH_CostAccount, SQLComparisonOperator.Equal, null));
						jobChargesCostAccountFilter.AddToFilter(JoinCondition.Or, JobChargeSchema.JR_OH_CostAccount, SQLComparisonOperator.Equal, SendingLine.InvoiceBase.AH_OH);
					}

					if (IncludeChargesForCreditorsWithTheSameAPSettlementGroup && SendingLine.InvoiceBase != null &&
						SendingLine.InvoiceBase.Header != null && SendingLine.InvoiceBase.Header.APSettlementGroup != null)
					{
						ZDBOnlySubQuery settlementGroupFilter = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
						ZDBOnlySubQuery orgRelatedPartyFilter = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_Parent);
						orgRelatedPartyFilter.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, SendingLine.InvoiceBase.Header.APSettlementGroup.PK);
						orgRelatedPartyFilter.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.APSettlementGroup);
						orgRelatedPartyFilter.AddToFilter(OrgRelatedPartySchema.PR_GC, GlbCompany.CurrentCompany.PK);

						settlementGroupFilter.AddSubQuery(orgRelatedPartyFilter, JoinCondition.And);

						ZDBOnlyQuery jobChargesAPSattlementGroupFilter = new ZDBOnlyQuery(typeof(JobCharge));
						jobChargesAPSattlementGroupFilter.AddSubQuery(JobChargeSchema.JR_OH_CostAccount, settlementGroupFilter, JoinCondition.And);

						jobChargesCostAccountFilter.AddToFilter(jobChargesAPSattlementGroupFilter, JoinCondition.Or);
					}
					jobChargesFilter.AddToFilter(jobChargesCostAccountFilter, JoinCondition.And);

					#endregion

					if (SendingLine.InvoiceBase != null)
					{
						var originalChargePKs = new List<ZGuid>();
						foreach (InvoicingLineBase line in SendingLine.InvoiceBase.Lines)
						{
							if (line.OriginalJobCharge != null)
							{
								originalChargePKs.Add(line.OriginalJobCharge.PK);
							}
						}
						if (originalChargePKs.Count > 0)
						{
							jobChargesFilter.AddToFilter(JoinCondition.And, JobChargeSchema.PK, SQLComparisonOperator.NotEqual, originalChargePKs.ToArray());
						}
					}
				}
				JobChargesCollection.RemoveAll();
				if (jobChargesFilter != null)
				{
					using (JobChargesCollection.SuspendListChanged())
					{
						JobChargesCollection.Load(jobChargesFilter);

						ExcludeChargesAlreadyImportedToIncompleteInvoices(jobChargesFilter);

						for (int count = JobChargesCollection.Count - 1; count >= 0; count--)
						{
							var jobCharge = JobChargesCollection[count];
							if (jobCharge.IsCostPosted)
							{
								JobChargesCollection.Remove(jobCharge);
							}
						}
						JobChargesCollection.SetReadOnlyIncludingChildren(true);
					}
					JobChargesCollection.RefreshBinding();
				}
			}
			finally
			{
				LoadingJobCharge = false;
			}

			void ExcludeChargesAlreadyImportedToIncompleteInvoices(ZQuery jobChargesFilter)
			{
				var alreadyImportedCharges = new ZQuery(jobChargesFilter);
				var chargeImportedInTheIncompleteInvoiceFilter = new ZDBOnlyQuery(typeof(JobCharge));
				var chargeAttribFilter = new ZDBOnlySubQuery(typeof(JobChargeAttrib), JobChargeAttribSchema.EC_JR);
				chargeAttribFilter.AddToFilter(JobChargeAttribSchema.EC_Name, JobChargeAttribTypeList.Codes.LinkedToIncompleteInvoice);
				chargeImportedInTheIncompleteInvoiceFilter.AddSubQuery(chargeAttribFilter, JoinCondition.And);
				alreadyImportedCharges.AddToFilter(chargeImportedInTheIncompleteInvoiceFilter);
				var importedCharges = Factory.Load<JobCharge>(alreadyImportedCharges);
				foreach (var jobCharge in importedCharges)
				{
					JobChargesCollection.Remove(jobCharge.PK);
				}
			}
		}
	}
}
