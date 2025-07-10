using CargoWise.EntityFramework;
using CargoWise.Types;

namespace CargoWise.Bi.Product.Manager.Business
{
	public abstract class EdwTable : NonPersistentBusinessObject
	{
		protected EdwTable(ZString schema, ZString name)
		{
			Schema = schema;
			Name = name;
		}

		public ZLong RowCount { get; set; }
		public ZDecimal SizeUsed { get; set; }
		public ZDecimal SizeUnused { get; set; }
		public ZDecimal TotalSize { get; set; }

		public ZString Schema { get; set; }
		public ZString Name { get; set; }
		public ZString CurrentState { get; set; }
		public ZString InitialLoadRequired { get; set; }
		public ZString SqlErrorMessage { get; set; }
		public ZDateTime SqlErrorDatetimeUTC { get; set; }
	}
}
