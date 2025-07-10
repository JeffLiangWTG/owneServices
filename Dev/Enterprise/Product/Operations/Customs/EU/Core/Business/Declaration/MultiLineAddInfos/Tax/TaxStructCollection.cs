using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class TaxStructCollection : NonPersistentBusinessObjectCollection<TaxStruct>
	{
		public TaxStructCollection() : base() { }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TaxStruct();
		}
	}
}
