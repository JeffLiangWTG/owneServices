using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class SelectableCommissionLineGroupingValidation<TGrouping, TLine> : CommissionLineGroupingValidation<TGrouping, TLine>
		where TGrouping : SelectableCommissionLineGrouping<TGrouping, TLine>
		where TLine : BusinessObject, ISelectableViewCommissionLineProvider
	{
		public SelectableCommissionLineGroupingValidation(CommissionLineGrouping<TGrouping, TLine> parent) : base(parent)
		{
		}

		public void ValidateIsSelected()
		{
			ZValidationInternals.Validate(Parent.IsSelectedInfo, CheckIsSelected);
		}

		protected virtual void CheckIsSelected()
		{
			if (!Parent.IsSelected)
			{
				return;
			}

			var query = new ZDBOnlyQuery(typeof(OrgCommissionAgreement));
			var subQueryInQueue = new ZDBOnlySubQuery(typeof(OrgCommissionCalculationQueue), OrgCommissionCalculationQueueSchema.CAQ_CA0);
			var subQueryHasRecipient = new ZDBOnlySubQuery(typeof(OrgCommissionAgreementRecipient), OrgCommissionAgreementRecipientSchema.CAR_CA0);
			if (!Parent.StaffCode.IsEmpty)
			{
				subQueryHasRecipient.AddToFilter(OrgCommissionAgreementRecipientSchema.CAR_GS_NKStaff, Parent.StaffCode);
			}
			else
			{
				subQueryHasRecipient.AddToFilter(OrgCommissionAgreementRecipientSchema.CAR_OH_Party, Parent.PartyPk);
			}

			query.AddSubQuery(subQueryInQueue, JoinCondition.And);
			query.AddSubQuery(subQueryHasRecipient, JoinCondition.And);

			var hasInQueue = Parent.Factory.ExistsInDatabase(OrgCommissionAgreementSchema.Constants.TableName, query);

			if (hasInQueue)
			{
				var recipient = Parent.StaffCode.IsEmpty ? Parent.PartyName : Parent.StaffName;
				Parent.IsSelectedInfo.AddWarning(Res.GetString("B4AEA281-9046-44F9-BAF8-13DDD0783D11", "Some agreements for {0} are still in calculation queue.", recipient));
			}
		}

		public new SelectableCommissionLineGrouping<TGrouping, TLine> Parent
		{
			get { return base.Parent as SelectableCommissionLineGrouping<TGrouping, TLine>; }
		}

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidateIsSelected();
		}
	}
}
