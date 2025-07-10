using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;
using Enterprise.ZArchitecture.Core;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.CommissionManagement.Business
{
	public class AmbiguousCommissionResolver : NonPersistentBusinessObject
	{
		public AmbiguousCommissionResolver(AmbiguousCommissionResolverFilterBusinessObject filterBizObj)
			: base(filterBizObj.Factory)
		{
			this.filterBizObj = filterBizObj;
		}

		#region Properties

		#region FilterBizObj

		public AmbiguousCommissionResolverFilterBusinessObject FilterBizObj
		{
			get { return filterBizObj; }
		}
		readonly AmbiguousCommissionResolverFilterBusinessObject filterBizObj;

		#endregion

		#endregion

		#region Resolve

		public void Resolve(Progress progress)
		{
			var itemsToResolve = ResolveItems.Where(x => x.ResolutionProvided).ToArray();

			var processed = 0;
			foreach (var item in itemsToResolve)
			{
				NotifyProgress(progress, GetResolvingCommissionStatusMessage(processed + 1, itemsToResolve.Length), 100 * processed / itemsToResolve.Length);

				item.AmbiguousCommission.AC0_CA0_SelectedAgreement = item.SelectedAgreementPk;

				var createCommissionContext = new CreateCommissionContext();
				createCommissionContext.OverwriteOldValues = true;

				var commissionAgreementAndRates = item.SelectedAgreement != null ? new CommissionAgreementAndRates(item.SelectedAgreement, TransactionCommissionCreator.GetMainTransaction(item.InvoicingBase).AH_PostDate.Date) : null;
				createCommissionContext.AgreementAndRatesOverride = new Dictionary<ZString, ICommissionAgreementAndRates>() { { item.AmbiguousCommission.AC0_CommissionStream, commissionAgreementAndRates } };
				createCommissionContext.OnlyCreateForAgreementAndRatesOverrideStreams = true;

				NonJobRelatedTransactionCommissionCreator.New(item.InvoicingBase).CreateCommissions(createCommissionContext);
			}

			Factory.Save();

			RefreshResolveItems();
		}

		#endregion

		#region Progress

		static void NotifyProgress(Progress progress, string status, int percentComplete)
		{
			if (progress != null)
			{
				progress(status, percentComplete);
			}
		}

		#endregion

		#region Related Business Objects

		public AmbiguousCommissionResolveItemCollection ResolveItemCollection
		{
			get
			{
				if (resolveItemCollection == null)
				{
					resolveItemCollection = new AmbiguousCommissionResolveItemCollection(Factory);
				}

				return resolveItemCollection;
			}
		}
		AmbiguousCommissionResolveItemCollection resolveItemCollection;

		public IEnumerable<AmbiguousCommissionResolveItem> ResolveItems
		{
			get { return ResolveItemCollection.Cast<AmbiguousCommissionResolveItem>(); }
		}

		public void LoadResolveItems()
		{
			LoadResolveItems(FilterBizObj.Filter);
		}

		public void RefreshResolveItems()
		{
			if (resolveItemCollection != null && lastResolveItemsFilterUsed != null)
			{
				LoadResolveItems(lastResolveItemsFilterUsed);
			}
		}

		void LoadResolveItems(ZQuery filter)
		{
			using (ResolveItemCollection.SuspendListChanged())
			{
				ResolveItemCollection.RemoveAndDeleteAll();
				ResolveItemCollection.AddRange(Factory.Load<AccAmbiguousCommission>(filter)
					.Select(x => new AmbiguousCommissionResolveItem(x))
					.Where(x => x.MatchingOverallItems.Any()));
			}

			lastResolveItemsFilterUsed = filter;
		}
		ZQuery lastResolveItemsFilterUsed;

		#endregion

		#region Messages

		protected string GetResolvingCommissionStatusMessage(int current, int total)
		{
			return Res.GetString("7f44d2a0-0d17-43c0-8c2e-f0d19b9a8aca", "Resolving Commissions: {0} of {1}", current, total);
		}

		#endregion
	}
}
