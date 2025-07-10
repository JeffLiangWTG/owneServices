using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class UnderbondSelectorLineCollection : NonPersistentBusinessObjectCollection<UnderbondSelectorLine>
	{
		public UnderbondSelectorLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotImplementedException();
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}
	}
}
