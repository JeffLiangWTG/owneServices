using CargoWise.EntityFramework;

namespace Enterprise.DocumentScanning.Business
{
	public class SearchTypeCollection : NonPersistentBusinessObjectCollection<SearchType>
	{
		public SearchTypeCollection() : base()
		{
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}

		public bool IsAtLeastOneSelected
		{
			get
			{
				foreach (SearchType searchType in Elements)
				{
					if (searchType.IsFilterOn)
					{
						return true;
					}
				}
				return false;
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SearchType();
		}

#if DEBUG
		public SearchType this[string searchTypeName]
		{
			get
			{
				foreach (SearchType searchType in Elements)
				{
					if (searchType.Name.EqualsIgnoringCase(searchTypeName))
					{
						return searchType;
					}
				}

				return null;
			}
		}
#endif

	}
}
