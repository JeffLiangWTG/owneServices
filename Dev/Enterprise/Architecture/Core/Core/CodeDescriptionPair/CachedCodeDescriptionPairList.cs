namespace Enterprise.ZArchitecture.Core
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class CachedCodeDescriptionPairList : CodeDescriptionPairList
	{
		public CachedCodeDescriptionPairList() : base()
		{
		}

		public bool NeedsRefresh(string filterString)
		{
			bool refresh;
			if ((FilterString == null && filterString != null) || (FilterString != null && filterString == null))
			{
				refresh = true;
			}
			else
			{
				refresh = FilterString != filterString;
			}
			return refresh;
		}

		protected string fFilterString;
		public string FilterString
		{
			get
			{
				return fFilterString;
			}
			set
			{
				fFilterString = value;
				Elements.Clear();
			}
		}
	}
}
