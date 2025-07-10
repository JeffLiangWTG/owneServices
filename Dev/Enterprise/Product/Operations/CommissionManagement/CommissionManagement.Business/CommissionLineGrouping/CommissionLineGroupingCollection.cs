using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;

namespace Enterprise.CommissionManagement.Business
{
	public abstract class CommissionLineGroupingCollection<TGrouping, TLine> : NonPersistentBusinessObjectCollection<TGrouping>
		where TGrouping : CommissionLineGrouping<TGrouping, TLine>
		where TLine : BusinessObject, IViewCommissionLineProvider
	{
		#region Constructor

		protected CommissionLineGroupingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region AddNew

		public TGrouping AddNew(IEnumerable<TLine> commissionLineProviders, ViewCommissionLineGrouper<TLine>[] subGroupers)
		{
			var result = CreateNew(subGroupers);
			result.Init(commissionLineProviders);
			Add(result);

			return result;
		}

		protected abstract TGrouping CreateNew(ViewCommissionLineGrouper<TLine>[] subGroupers);

		#endregion

		#region Notifications

		public void AddAmountNotifications(INotificationType notificationType, bool rollupLineNotificationsToLeafGrouping = false)
		{
			foreach (TGrouping grouping in this)
			{
				RecursivelyAddNotificationsOnAllSubGroupings(grouping, notificationType, ContainsTotalAmountIssuesMessage, AmountValidation, rollupLineNotificationsToLeafGrouping);
				grouping.Validation.ValidateAll();
			}
		}

		public void AddFullPaymentNotifications(INotificationType notificationType, bool rollupLineNotificationsToLeafGrouping = false)
		{
			foreach (TGrouping grouping in this)
			{
				RecursivelyAddNotificationsOnAllSubGroupings(grouping, notificationType, ContainsFullPaymentIssuesMessage, FullPaymentValidation, rollupLineNotificationsToLeafGrouping);
				grouping.Validation.ValidateAll();
			}
		}

		void AmountValidation(ViewCommissionLine line)
		{
			line.Validation.ValidateAllLocalAmounts();
			line.Validation.ValidateAllPreferredAmounts();
		}

		void FullPaymentValidation(ViewCommissionLine line)
		{
			line.Validation.ValidateFullyPaymentOfARInvoices();
		}

		bool RecursivelyAddNotificationsOnAllSubGroupings(TGrouping grouping, INotificationType notificationType, string message, Action<ViewCommissionLine> validationAction, bool rollupLineNotificationsToLeafGrouping = false)
		{
			if (!grouping.IsLeaf)
			{
				var hasASubgroupWithWarning = false;

				foreach (var subGroup in grouping.SubGroupings)
				{
					hasASubgroupWithWarning |= RecursivelyAddNotificationsOnAllSubGroupings(subGroup, notificationType, message, validationAction, rollupLineNotificationsToLeafGrouping);
				}

				if (hasASubgroupWithWarning)
				{
					grouping.AddRowNotification(new Notification(notificationType, message));
				}
				return hasASubgroupWithWarning;
			}
			else
			{
				bool hasALineWithWarning = false;

				foreach (var line in grouping.CommissionLines)
				{
					using (line.ResumeValidationTemporarily())
					{
						validationAction(line);
						hasALineWithWarning |= line.HasWarnings;
					}
				}

				if (rollupLineNotificationsToLeafGrouping)
				{
					grouping.ShouldRollupCommissionLineNotifications = true;
					grouping.Validation.ValidateAll();
				}

				if (hasALineWithWarning)
				{
					grouping.AddRowNotification(new Notification(notificationType, message));
				}
				return hasALineWithWarning;
			}
		}

		static string ContainsTotalAmountIssuesMessage
		{
			get { return Res.GetString("e5e83346-bde8-4ce7-bc26-0c9156ca9ff4", "Contains commission amount(s) with issues that may effect the total calculated amount."); }
		}

		static string ContainsFullPaymentIssuesMessage
		{
			get
			{
				return OrganisationsDataRegistry.Instance.DisallowCommissionPaymentIfARInvoiceNotFullyPaid.Value ? Res.GetString("8E4FD493-357B-4B57-8396-3A684148EA1A", "AR invoice(s) have to be fully paid prior to payment process action")
						: Res.GetString("D0B2CFDF-0E06-457B-9214-5B8E4BFE9066", "AR Invoice(s) are not fully paid");
			}
		}

		#endregion

		#region Overrides

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion
	}
}
