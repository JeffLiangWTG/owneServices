using CargoWise.EntityFramework;

namespace Enterprise.ResourceStrings.Business
{
	public class RefLocalLanguageCollection : BusinessObjectCollection<RefLocalLanguage>
	{
		public RefLocalLanguageCollection(BusinessObjectFactory factory) : base(factory)
		{ }

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}
	}
}
