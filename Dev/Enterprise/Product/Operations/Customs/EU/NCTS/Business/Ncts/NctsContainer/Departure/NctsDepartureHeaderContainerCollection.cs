using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsDepartureHeaderContainerCollection<out TContainer, out TMaster> : IBusinessObjectCollection<TContainer>, ISupportMaxCountValidation
		where TContainer : NctsDepartureHeaderContainer
		where TMaster : NctsHeader
	{
		IEnumerable<ZString> AllSeals { get; }
		TMaster Master { get; }
		void Reload(bool reLoadExistingRows, bool assumeRowsMissingFromQueryResultsAreDeleted = false);
		void RefreshBinding();
		void SetReadOnlyIncludingChildren(bool readOnly);
		IDisposable SuspendSettingHasChanges();

		new TContainer this[int index] { get; }
	}

	public class NctsDepartureHeaderContainerCollection<TContainer, TMaster> : DependentBusinessObjectCollection<TContainer, TMaster>, INctsDepartureHeaderContainerCollection<TContainer, TMaster>
		where TContainer : NctsDepartureHeaderContainer
		where TMaster : NctsHeader
	{
		public NctsDepartureHeaderContainerCollection(TMaster master)
			: base(master)
		{
			if (master != null && master.IsPhase5Departure && master.Configuration.ValidationRuleConfiguration.IsRuleTR0024Active)
			{
				var maxContainersAllowed = 9999;
				this.EnableMaxCountValidationWithMessageError(maxContainersAllowed, warnAtHalfway: false, Res.GetString("1C7FA02F-3194-4C30-A528-B094FDA8DE04", "[{0}] The maximum number of {1} Containers/Equipments has been exceeded.", ValidationRuleCodeConstants.TR0024, maxContainersAllowed));
			}
		}

		public IEnumerator<TContainer> GetEnumerator() => Elements.Cast<TContainer>().GetEnumerator();

		protected override CargoWise.Schema.SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusInBondContainerSchema.BC_ParentID; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(CusInBondContainerSchema.BC_TypeOfService, ContainerTypeOfServiceList.Codes.DepartureContainer);
			query.IsNoResultQuery = Master.IsPhase5Arrival;
			return query;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var headerContainer = (TContainer)child;
			if (Count > 0 && Master.IsPhase5)
			{
				headerContainer.BC_Mode = this[0].BC_Mode;
				headerContainer.BC_SequenceNumber = (ZShort)(Count + 1);
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			Master.HeaderContainersLineNumberGenerator.ReCalculateAll();
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			Master.HeaderContainersLineNumberGenerator.ReCalculateAll();
		}

		public override void Load()
		{
			if (Master.IsDepartureMovement)
			{
				base.Load();
			}
		}

		public IEnumerable<ZString> AllSeals
		{
			get
			{
				foreach (TContainer container in this)
				{
					if (!container.BC_Seal1.IsEmpty)
					{
						yield return container.BC_Seal1;
					}

					if (!container.BC_Seal2.IsEmpty)
					{
						yield return container.BC_Seal2;
					}
				}
			}
		}

		protected override bool AllowNewCore => !Master.IsPhase5Arrival;

		protected override bool AllowRemoveCore
		{
			get { return true; }
		}
	}
}
