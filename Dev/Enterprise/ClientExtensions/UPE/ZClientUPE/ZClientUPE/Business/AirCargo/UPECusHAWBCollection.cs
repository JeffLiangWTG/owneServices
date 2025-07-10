
using CargoWise.EntityFramework;

namespace Enterprise.Client.UPE.Business
{
	public class UPECusHAWBCollection : BusinessObjectCollection<UPECusHAWB>
	{
		public UPECusHAWBCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public override void RemoveAndDeleteAll()
		{
		}

		public override void Remove(BusinessObject elementToRemove)
		{
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
		}
	}
}
