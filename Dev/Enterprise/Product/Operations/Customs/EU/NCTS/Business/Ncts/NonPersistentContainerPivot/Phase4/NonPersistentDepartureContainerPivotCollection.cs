using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NonPersistentDepartureContainerPivotCollection : NonPersistentBusinessObjectCollection<NonPersistentDepartureContainerPivot>
	{
		public NonPersistentDepartureContainerPivotCollection(NctsCommonCargoDesc line)
			: base(line.Factory)
		{
			this.line = line;
			var header = line.Header;
			if (header != null)
			{
				var containers = header.DepartureHeaderContainers;

				using (containers.SuspendSettingHasChanges())
				{
					foreach (NctsDepartureHeaderContainer container in containers)
					{
						AddNewNonPersistentContainer(container);
					}
				}
				containers.CountChanged += new CollectionCountChangedEventHandler(Containers_CountChanged);
			}
		}

		public NonPersistentDepartureContainerPivot AddNewNonPersistentContainer(NctsDepartureHeaderContainer container)
		{
			using (SuspendSettingHasChanges())
			{
				var npContainer = AddNew();
				npContainer.Container = container;
				return npContainer;
			}
		}

		public void RemoveNonPersistentContainer(NctsDepartureHeaderContainer container)
		{
			foreach (NonPersistentDepartureContainerPivot containerForDelete in ToArray())
			{
				if (containerForDelete.Container.PK == container.PK)
				{
					RemoveAndDelete(containerForDelete);
					return;
				}
			}
		}

		protected virtual void Containers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				AddNewNonPersistentContainer((NctsDepartureHeaderContainer)e.BizObject);
			}
			else // removed
			{
				RemoveNonPersistentContainer((NctsDepartureHeaderContainer)e.BizObject);
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new NonPersistentDepartureContainerPivot(line);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		internal void DeleteAll()
		{
			RemoveAll();
		}

		protected NctsCommonCargoDesc line;
	}
}
