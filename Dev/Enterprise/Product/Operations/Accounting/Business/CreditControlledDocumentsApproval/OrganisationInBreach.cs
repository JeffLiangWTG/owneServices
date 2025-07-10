using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.CreditStatus;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class OrganisationInBreach : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema
		public abstract class Schema
		{
			public const string OrgName = "OrgName";
			public const string OrgCode = "OrgCode";
			public const string OrgCreditLimit = "OrgCreditLimit";
			public const string IsUsingSettlementGroupCreditLimit = "IsUsingSettlementGroupCreditLimit";
			public const string IsCreditOnHold = "IsCreditOnHold";
			public const string OverCreditLimit = "OverCreditLimit";
			public const string StandardOverdueAmount = "StandardOverdueAmount";
			public const string DisbursementOverdueAmount = "DisbursementOverdueAmount";
			public const string StandardWIPsBilledToThisJob = "StandardWIPsBilledToThisJob";
			public const string DisbursementWIPsBilledToThisJob = "DisbursementWIPsBilledToThisJob";
			public const string BreachReasons = "BreachReasons";
		}

		#endregion

		public OrganisationInBreach()
		{
			dataAccessor = new ARAPDataAccessor();
		}

		OrgHeader organisation;
		readonly ARAPDataAccessor dataAccessor;
		BusinessObject parentBusinessObject;

		#region Properties

		public int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();
		public ZGuid OrganisationPK => organisation.PK;

		#region OrgName

		public ZString OrgName
		{
			get => orgName;
			private set => SetNonPersistentPropertyValue(OrgNameInfo, ref orgName, value);
		}
		ZString orgName;

		public ZPropertyInfo OrgNameInfo => GetZPropertyInfo(Schema.OrgName);

		#endregion

		#region OrgCode

		public ZString OrgCode
		{
			get => orgCode;
			private set => SetNonPersistentPropertyValue(OrgCodeInfo, ref orgCode, value);
		}
		ZString orgCode;

		public ZPropertyInfo OrgCodeInfo => GetZPropertyInfo(Schema.OrgCode);

		#endregion

		#region OrgCreditLimit

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal OrgCreditLimit
		{
			get => orgCreditLimit;
			private set => SetNonPersistentPropertyValue(OrgCreditLimitInfo, ref orgCreditLimit, value);
		}
		ZDecimal orgCreditLimit;

		public ZPropertyInfo OrgCreditLimitInfo => GetZPropertyInfo(Schema.OrgCreditLimit);

		#endregion

		#region IsUsingSettlementGroupCreditLimit
		public ZBool IsUsingSettlementGroupCreditLimit
		{
			get => isUsingSettlementGroupCreditLimit;
			private set => SetNonPersistentPropertyValue(IsUsingSettlementGroupCreditLimitInfo, ref isUsingSettlementGroupCreditLimit, value);
		}
		ZBool isUsingSettlementGroupCreditLimit;

		public ZPropertyInfo IsUsingSettlementGroupCreditLimitInfo => GetZPropertyInfo(Schema.IsUsingSettlementGroupCreditLimit);

		#endregion

		#region IsCreditOnHold

		public ZBool IsCreditOnHold
		{
			get => isCreditOnHold;
			private set => SetNonPersistentPropertyValue(IsCreditOnHoldInfo, ref isCreditOnHold, value);
		}
		ZBool isCreditOnHold;

		public ZPropertyInfo IsCreditOnHoldInfo => GetZPropertyInfo(Schema.IsCreditOnHold);

		#endregion

		#region OverCreditLimit

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal OverCreditLimit
		{
			get => overCreditLimit;
			private set => SetNonPersistentPropertyValue(OverCreditLimitInfo, ref overCreditLimit, value);
		}
		ZDecimal overCreditLimit;

		public ZPropertyInfo OverCreditLimitInfo => GetZPropertyInfo(Schema.OverCreditLimit);

		#endregion

		#region StandardOverdueAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal StandardOverdueAmount
		{
			get => standardOverdueAmount;
			private set => SetNonPersistentPropertyValue(StandardOverdueAmountInfo, ref standardOverdueAmount, value);
		}
		ZDecimal standardOverdueAmount;

		public ZPropertyInfo StandardOverdueAmountInfo => GetZPropertyInfo(Schema.StandardOverdueAmount);

		#endregion

		#region DisbursementOverdueAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal DisbursementOverdueAmount
		{
			get => disbursementOverdueAmount;
			private set => SetNonPersistentPropertyValue(DisbursementOverdueAmountInfo, ref disbursementOverdueAmount, value);
		}
		ZDecimal disbursementOverdueAmount;

		public ZPropertyInfo DisbursementOverdueAmountInfo => GetZPropertyInfo(Schema.DisbursementOverdueAmount);

		#endregion

		#region StandardWIPsBilledToThisJob

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal StandardWIPsBilledToThisJob
		{
			get => standardWIPsBilledToThisJob;
			private set => SetNonPersistentPropertyValue(StandardWIPsBilledToThisJobInfo, ref standardWIPsBilledToThisJob, value);
		}
		ZDecimal standardWIPsBilledToThisJob;

		public ZPropertyInfo StandardWIPsBilledToThisJobInfo => GetZPropertyInfo(Schema.StandardWIPsBilledToThisJob);

		#endregion

		#region DisbursementWIPsBilledToThisJob

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal DisbursementWIPsBilledToThisJob
		{
			get => disbursementWIPsBilledToThisJob;
			private set => SetNonPersistentPropertyValue(DisbursementWIPsBilledToThisJobInfo, ref disbursementWIPsBilledToThisJob, value);
		}
		ZDecimal disbursementWIPsBilledToThisJob;

		public ZPropertyInfo DisbursementWIPsBilledToThisJobInfo => GetZPropertyInfo(Schema.DisbursementWIPsBilledToThisJob);

		#endregion

		#region BreachReasons

		public ZString BreachReasons
		{
			get => breachReasons;
			private set => SetNonPersistentPropertyValue(BreachReasonsInfo, ref breachReasons, value);
		}
		ZString breachReasons;

		public ZPropertyInfo BreachReasonsInfo => GetZPropertyInfo(Schema.BreachReasons);

		#endregion

		#endregion

		internal void SetValues(OrgHeader org, string reasonsForCreditCheck, BusinessObject approvalRequestParentBizObj)
		{
			organisation = org;
			parentBusinessObject = approvalRequestParentBizObj;
			OrgName = org.OH_FullName;
			OrgCode = org.OH_Code;
			IsUsingSettlementGroupCreditLimit = org.CompanyData.OB_ARUseSettlementGroupCreditLimit;
			SetCreditDetails();
			IsCreditOnHold = org.CompanyData.OB_AROnCreditHold;
			StandardOverdueAmount = dataAccessor.GetStandardAROverdueAmount(org.PK.ToGuid());
			DisbursementOverdueAmount = dataAccessor.GetDisbursementAROverdueAmount(org.PK.ToGuid());
			SetWIPsBilledToThisJob();
			BreachReasons = reasonsForCreditCheck;
		}

		void SetCreditDetails()
		{
			var creditDetails = organisation.CreditChecker.GetCreditDetails(LedgerTypes.AccountsReceivable);
			OrgCreditLimit = creditDetails.CreditLimit;
			var creditAvailable = creditDetails.CreditLimit - creditDetails.TotalOutstandingAmount;
			OverCreditLimit = creditAvailable >= 0 ? 0m : -creditAvailable;
		}

		void SetWIPsBilledToThisJob()
		{
			var charges = GetChargesForThisJob();
			var unpostedChargesForThisOrg = charges.Where(x => !x.IsRevenuePosted && x.JR_OH_SellAccount == organisation.PK);
			foreach (var charge in unpostedChargesForThisOrg)
			{
				if (charge.IsDisbursementCharge)
				{
					DisbursementWIPsBilledToThisJob += charge.JR_LocalSellAmt;
				}
				else
				{
					StandardWIPsBilledToThisJob += charge.JR_LocalSellAmt;
				}
			}
		}

		List<Charge> GetChargesForThisJob()
		{
			var charges = new List<Charge>();
			var consol = parentBusinessObject as ForwardingConsol;
			if (consol != null && !consol.IsGateway())
			{
				consol.Shipments.ForEach(x => charges.AddRange(LoadJobAndCharges(x)));
			}
			else
			{
				charges.AddRange(LoadJobAndCharges(parentBusinessObject));
			}
			return charges;
		}

		List<Charge> LoadJobAndCharges(BusinessObject parent)
		{
			var charges = new List<Charge>();
			if (parent is IJobHeaderParent jobHeaderParent)
			{
				var job = new Job.Loader(jobHeaderParent).Load();
				if (job != null)
				{
					charges.AddRange(job.Charges.Cast<Charge>());
				}
			}
			return charges;
		}
	}
}