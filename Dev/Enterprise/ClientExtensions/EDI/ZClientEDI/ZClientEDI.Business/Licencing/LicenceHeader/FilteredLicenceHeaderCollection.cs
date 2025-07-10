using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class FilteredLicenceHeaderCollection : BusinessObjectCollectionView<LicenceHeader>
	{
		public FilteredLicenceHeaderCollection(LicenceHeaderCollection collectionToFilter)
			: base(collectionToFilter)
		{
			includeAllItems = false;
			Rebuild();
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			LicenceHeader item = (LicenceHeader)element;
			return (IncludeAllItems || !item.IsCargoWiseInstallation);
		}

		#region IncludeAllItems

		public bool IncludeAllItems
		{
			get { return includeAllItems; }
			set
			{
				if (value != includeAllItems)
				{
					includeAllItems = value;
					Rebuild();
				}
			}
		}

		bool includeAllItems;

		#endregion
	}
}
