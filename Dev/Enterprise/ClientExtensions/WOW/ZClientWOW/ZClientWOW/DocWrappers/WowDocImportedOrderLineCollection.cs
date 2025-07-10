using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers;

namespace Enterprise.Client.Wow
{
	class WowDocImportedOrderLineCollection : DocOrderLineCollection
	{
		public WowDocImportedOrderLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new WowDocImportOrderLine this[int index]
		{
			get { return (WowDocImportOrderLine)base[index]; }
		}
	}
}
