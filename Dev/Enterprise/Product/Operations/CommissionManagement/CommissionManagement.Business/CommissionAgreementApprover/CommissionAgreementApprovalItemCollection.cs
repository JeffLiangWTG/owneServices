using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionAgreementApprovalItemCollection : NonPersistentBusinessObjectCollection<CommissionAgreementApprovalItem>
	{
		public CommissionAgreementApprovalItemCollection(CommissionAgreementApprovalWizard wizard)
			: base(wizard.Factory)
		{
			this.wizard = wizard;
		}

		readonly CommissionAgreementApprovalWizard wizard;

		#region Add / Remove

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CommissionAgreementApprovalItem();
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);

			if (!IsRefreshSuspended)
			{
				using (TemporarilySuspendRefreshCollection())
				{
					var item = (CommissionAgreementApprovalItem)child;
					if (item.CommissionAgreement != null)
					{
						CommissionAgreements.Add(item.CommissionAgreement);
					}
				}
			}
		}

		protected override void RemoveCollectionRelationshipsCore(BusinessObject child, bool forDelete)
		{
			base.RemoveCollectionRelationshipsCore(child, forDelete);

			if (!IsRefreshSuspended)
			{
				using (TemporarilySuspendRefreshCollection())
				{
					var item = (CommissionAgreementApprovalItem)child;
					if (item.CommissionAgreement != null)
					{
						CommissionAgreements.RemoveFromRelationship(item.CommissionAgreement);
					}
				}
			}
		}

		#endregion

		#region Load

		public void Load(OrgCommissionAgreementCollection agreements)
		{
			using (TemporarilySuspendRefreshCollection())
			{
				CommissionAgreements.RemoveAllFromRelationship();
				CommissionAgreements.AddRange(agreements);
			}

			RefreshCollection();
		}

		OrgCommissionAgreementCollection CommissionAgreements
		{
			get
			{
				if (commissionAgreements == null)
				{
					commissionAgreements = new OrgCommissionAgreementCollection(Factory, new AdhocCollectionRelationship(typeof(OrgCommissionAgreement)));
					commissionAgreements.CountChanged += CommissionAgreements_CountChanged;
				}

				return commissionAgreements;
			}
		}

		OrgCommissionAgreementCollection commissionAgreements;

		void CommissionAgreements_CountChanged(object sender, EventArgs e)
		{
			RefreshCollection();
		}

		protected virtual void RefreshCollection()
		{
			if (!IsRefreshSuspended)
			{
				using (TemporarilySuspendRefreshCollection())
				using (SuspendListChanged())
				{
					var agreementsToAdd = new HashSet<OrgCommissionAgreement>(CommissionAgreements);
					foreach (CommissionAgreementApprovalItem item in this.ToArray())
					{
						if (agreementsToAdd.Contains(item.CommissionAgreement))
						{
							agreementsToAdd.Remove(item.CommissionAgreement);
						}
						else
						{
							RemoveAndDelete(item);
						}
					}

					foreach (var agreement in agreementsToAdd)
					{
						Add(new CommissionAgreementApprovalItem(wizard, agreement));
					}
				}
			}
		}

		public IDisposable TemporarilySuspendRefreshCollection()
		{
			refreshCollectionSuspendedNestingLevel++;
			return new DisposableAction(() =>
			{
				refreshCollectionSuspendedNestingLevel--;
			});
		}

		protected bool IsRefreshSuspended => refreshCollectionSuspendedNestingLevel > 0;

		int refreshCollectionSuspendedNestingLevel;

		#endregion
	}
}
