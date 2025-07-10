using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceDatabaseCollectionView : BusinessObjectCollectionView<LicenceDatabase>
	{
		public LicenceDatabaseCollectionView(BusinessObjectCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return true;
		}
	}

	public class ActiveOrAllLicenceDatabaseCollection : LicenceDatabaseCollectionView
	{
		public ActiveOrAllLicenceDatabaseCollection(LicenceCompanyLicenceDatabaseCollection collectionToFilter)
			: base(collectionToFilter)
		{
			Rebuild();
		}

		LicenceCompanyLicenceDatabaseCollection Collection
		{
			get { return (LicenceCompanyLicenceDatabaseCollection)CollectionToFilter; }
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return IncludeInactiveDatabases || Collection.Parent.IsActive((LicenceDatabase)element);
		}

		#region IncludeInactiveDatabases

		public bool IncludeInactiveDatabases
		{
			get
			{
				return includeInactiveDatabases;
			}
			set
			{
				if (value != includeInactiveDatabases)
				{
					includeInactiveDatabases = value;
					Rebuild();
				}
			}
		}

		bool includeInactiveDatabases;

		#endregion
	}
}


