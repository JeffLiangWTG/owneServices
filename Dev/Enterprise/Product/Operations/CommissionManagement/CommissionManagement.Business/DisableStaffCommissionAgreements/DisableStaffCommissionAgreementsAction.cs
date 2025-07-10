using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class DisableStaffCommissionAgreementsAction : AutoDisableStaffCommissionAgreementsAction
	{
		#region New

		public static DisableStaffCommissionAgreementsAction New(GlbStaff staff)
		{
			var overridden = OverridableNewDelegate.Value;
			return overridden != null ? overridden(staff) : new DisableStaffCommissionAgreementsAction(staff);
		}

		protected delegate DisableStaffCommissionAgreementsAction NewDelegate(GlbStaff staff);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		protected DisableStaffCommissionAgreementsAction(GlbStaff staff)
			: base(staff.Factory)
		{
			Argument.NotNull(staff, "staff");

			this.staff = staff;

			using (SuspendSettingHasChanges())
			{
				this.Date = !staff.GS_DepartureDate.IsEmpty ? staff.GS_DepartureDate.Date : ZDateTime.Now.Date;
			}
		}

		readonly GlbStaff staff;

		#region Execute

		public void ExecuteButDontApprove()
		{
			var agreementsWithStaffAsRecipient = GetCommissionAgreementsWithStaffAsRecipient();
			foreach (var agreement in agreementsWithStaffAsRecipient)
			{
				if (!agreement.IsDraft)
				{
					if (!agreement.HasDraft)
					{
						var agreementDraft = agreement.CreateDraft();
						DisableStaffCommissionAgreementsIfRequired(agreementDraft, true);
					}
				}
				else
				{
					DisableStaffCommissionAgreementsIfRequired(agreement, true);
				}
			}

			Factory.Save();
		}

		public void ExecuteAndApprove(Progress progress)
		{
			using (var manager = ((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var agreementsToApprove = new List<OrgCommissionAgreement>();
				var agreementsWithStaffAsRecipient = GetCommissionAgreementsWithStaffAsRecipient();
				foreach (var agreement in agreementsWithStaffAsRecipient)
				{
					var disabledStaffCommissionAgreements = DisableStaffCommissionAgreementsIfRequired(agreement, !agreement.HasDraft);
					if (disabledStaffCommissionAgreements && !agreement.IsDraft)
					{
						agreementsToApprove.Add(agreement);
					}
				}

				Factory.Save();

				var context = CreateContext();

				context.AgreementsBeingApproved = new HashSet<OrgCommissionAgreement>(agreementsToApprove);

				var approver = GetCommissionAgreementApprover();
				approver.ApproveAndQueue(context, progress);

				manager.CommitTransaction();
			}
		}

		protected virtual CreateCommissionContext CreateContext()
		{
			var context = new CreateCommissionContext();
			context.OverwriteOldValues = true;
			return context;
		}

		protected virtual CommissionAgreementApprover GetCommissionAgreementApprover()
		{
			return CommissionAgreementApprover.New(Factory);
		}

		OrgCommissionAgreement[] GetCommissionAgreementsWithStaffAsRecipient()
		{
			var agreementsQuery = new ZDBOnlyQuery(typeof(OrgCommissionAgreement));
			var agreementHasStaffAsRecipientSubquery = new ZDBOnlySubQuery(typeof(OrgCommissionAgreementRecipient), OrgCommissionAgreementRecipientSchema.CAR_CA0);
			agreementHasStaffAsRecipientSubquery.AddToFilter(OrgCommissionAgreementRecipientSchema.CAR_GS_NKStaff, staff.GS_Code);
			if (!ShouldUpdateEarlierEndDates)
			{
				var recipientEndDateFilter = new ZQuery(OrgCommissionAgreementRecipientSchema.CAR_EndDate, null);
				recipientEndDateFilter.AddToFilter(JoinCondition.Or, OrgCommissionAgreementRecipientSchema.CAR_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, Date.Date.AddDays(1));
				agreementHasStaffAsRecipientSubquery.AddToFilter(recipientEndDateFilter);
			}
			agreementsQuery.AddSubQuery(agreementHasStaffAsRecipientSubquery, JoinCondition.And);

			return Factory.Load<OrgCommissionAgreement>(agreementsQuery);
		}

		bool DisableStaffCommissionAgreementsIfRequired(OrgCommissionAgreement agreement, bool shouldAddLog)
		{
			var staffRecipientsWithDifferentEndDate =
				from recipient in agreement.Recipients
				where
					recipient.CAR_GS_NKStaff == staff.GS_Code &&
					recipient.CAR_EndDate != Date.Date
				select recipient;

			bool hasUpdates = false;
			foreach (var staffRecipient in staffRecipientsWithDifferentEndDate)
			{
				if (ShouldUpdateEarlierEndDates || staffRecipient.CAR_EndDate.IsEmpty || staffRecipient.CAR_EndDate > Date.Date)
				{
					hasUpdates = true;
					staffRecipient.CAR_EndDate = Date.Date;

					if (shouldAddLog)
					{
						staffRecipient.GetMainVersion().Logs.AddNew(Events.StatusChange, string.Format("Staff Disable Date: {0}", Date.Date.ToShortDateString()));
					}
				}
			}

			return hasUpdates;
		}

		#endregion
	}
}
