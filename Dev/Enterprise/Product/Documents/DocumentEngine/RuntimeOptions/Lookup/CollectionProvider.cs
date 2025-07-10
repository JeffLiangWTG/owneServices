using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public abstract class CollectionProvider
	{
		public CollectionProvider(BusinessObjectFactory businessObjectFactory)
		{
			this.BusinessObjectFactory = businessObjectFactory;
			Filter = new ZQuery();
		}

		protected ZQuery Filter;
		protected BusinessObjectFactory BusinessObjectFactory;

		public IBusinessObjectCollection Collection
		{
			get
			{
				if (fCollection == null || NeedRebindCollections)
				{
					fCollection = CreateCollection();
					NeedRebindCollections = false;
				}
				return fCollection;
			}
		}
		IBusinessObjectCollection fCollection;

		protected virtual IBusinessObjectCollection GetCollectionForFindbox()
		{
			return CreateCollection();
		}

		protected abstract IBusinessObjectCollection CreateCollection();
		public abstract ModuleIdentifier ModuleID { get; }

		IBusinessObjectCollection fCollectionForFindbox;
		public IBusinessObjectCollection CollectionForFindbox
		{
			get
			{
				if (fCollectionForFindbox == null || NeedRebindCollectionsForFindBox)
				{
					fCollectionForFindbox = GetCollectionForFindbox();
					NeedRebindCollectionsForFindBox = false;
					if (fCollectionForFindbox != null)
					{
						fCollectionForFindbox.IncrementReadOnlyIncludingChildren();
					}
				}
				return fCollectionForFindbox;
			}
		}

		protected bool NeedRebindCollections { get; set; }

		protected bool NeedRebindCollectionsForFindBox { get; set; }

		public void SetFilterCollection(ZQuery filter)
		{
			this.Filter = filter;
			fCollection = null;
			fCollectionForFindbox = null;
		}

		public virtual void AddValidationAndDefault(FilterField parentFilterField, ValidatorPack validatorPack)
		{
		}

		public virtual string GetFilterDescription()
		{
			return string.Empty;
		}

		public virtual void SetDependencyValue(string value)
		{
		}
	}
}
