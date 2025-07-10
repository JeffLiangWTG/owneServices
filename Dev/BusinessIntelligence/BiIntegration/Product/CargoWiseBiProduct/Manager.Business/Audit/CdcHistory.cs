using CargoWise.EntityFramework;
using CargoWise.Types;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class CdcHistory : NonPersistentBusinessObject
	{
		public CdcHistory(ZString schema, ZString table)
		{
			Schema = schema;
			Table = table;
		}

		public ZString Schema { get; }
		public ZString Table { get; }
		public ZString Lsn { get; set; }
		public ZInt NumberOfRows { get; set; }
		public ZDateTime TransactionDateUtc { get; set; }
		public ZDateTime TransactionDateLocal { get; set; }
	}
}
