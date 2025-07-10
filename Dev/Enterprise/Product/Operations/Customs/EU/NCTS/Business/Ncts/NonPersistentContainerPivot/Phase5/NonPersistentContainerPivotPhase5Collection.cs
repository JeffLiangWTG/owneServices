using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NonPersistentContainerPivotPhase5Collection : NonPersistentBusinessObjectCollection<NonPersistentContainerPivotPhase5>
	{
		public NonPersistentContainerPivotPhase5Collection(NctsPackage package)
			: base(package.Factory)
		{
			this.package = package;
			var header = package.Parent?.Header;
			if (header != null)
			{
				isPhase5Arrival = header.IsPhase5Arrival;
				var containers = isPhase5Arrival ? header.ArrivalHeaderContainers : (BusinessObjectCollection)header.DepartureHeaderContainers;
				using (containers.SuspendSettingHasChanges())
				{
					foreach (NctsCusInBondContainer container in containers)
					{
						if (isPhase5Arrival)
						{
							container.BC_UnloadedStateInfo.ValueChanged += BC_UnloadedStateInfo_ValueChanged;
							if (container.BC_UnloadedState != NctsUnloadedStateList.Codes.MIS)
							{
								AddNewNonPersistentContainer(container);
							}
						}
						else
						{
							AddNewNonPersistentContainer(container);
						}
					}
				}

				containers.CountChanged += Containers_CountChanged;
			}
		}

		readonly bool isPhase5Arrival;

		protected virtual void AddNewNonPersistentContainer(NctsCusInBondContainer container)
		{
			using (SuspendSettingHasChanges())
			{
				Add(new NonPersistentContainerPivotPhase5(package, container));
			}
		}

		void RemoveNonPersistentContainer(NctsCusInBondContainer container)
		{
			foreach (NonPersistentContainerPivotPhase5 containerForDelete in ToArray())
			{
				if (containerForDelete.Container == container)
				{
					RemoveAndDelete(containerForDelete);
					return;
				}
			}
		}

		protected virtual void Containers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var container = (NctsCusInBondContainer)e.BizObject;
			if (e.ItemAdded)
			{
				if (isPhase5Arrival)
				{
					container.BC_UnloadedStateInfo.ValueChanged += BC_UnloadedStateInfo_ValueChanged;
				}
				AddNewNonPersistentContainer(container);
			}
			else // removed
			{
				if (isPhase5Arrival)
				{
					container.BC_UnloadedStateInfo.ValueChanged -= BC_UnloadedStateInfo_ValueChanged;
				}
				RemoveNonPersistentContainer(container);
			}
		}

		protected virtual void BC_UnloadedStateInfo_ValueChanged(object sender, EventArgs e)
		{
			if (e is ValueChangedEventArgs valueChangedEventArgs)
			{
				var container = ((NctsCusInBondContainer)sender);

				if (!container?.IsDeleted ?? false)
				{
					if (valueChangedEventArgs.OldValue.ToString() == NctsUnloadedStateList.Codes.MIS)
					{
						AddNewNonPersistentContainer(container);
					}
					if (valueChangedEventArgs.NewValue.ToString() == NctsUnloadedStateList.Codes.MIS)
					{
						RemoveNonPersistentContainer(container);
					}
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotSupportedException();

		protected override bool AllowNewCore => false;

		protected NctsPackage package;
	}
}
