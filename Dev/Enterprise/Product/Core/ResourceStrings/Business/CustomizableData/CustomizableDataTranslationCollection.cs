using CargoWise.EntityFramework;

namespace Enterprise.ResourceStrings.Business
{
	public class CustomizableDataTranslationCollection : NonPersistentBusinessObjectCollection<CustomizableDataTranslationEntry>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotImplementedException();
		}

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
