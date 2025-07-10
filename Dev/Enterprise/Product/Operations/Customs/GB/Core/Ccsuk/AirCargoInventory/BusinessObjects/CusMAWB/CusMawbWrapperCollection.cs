using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusMawbWrapperCollection : NonPersistentBusinessObjectCollection<CusMawbWrapper>
	{
		public CusMawbWrapperCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public CusMAWB MAWB { get; set; }

		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotImplementedException();
		}
	}
}
