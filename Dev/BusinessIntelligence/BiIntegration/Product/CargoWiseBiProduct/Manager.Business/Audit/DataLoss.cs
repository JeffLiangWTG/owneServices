using CargoWise.EntityFramework;
using CargoWise.Types;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class DataLoss : NonPersistentBusinessObject
	{
		public DataLoss(ZString schema, ZString name)
		{
			Schema = schema;
			Name = name;
		}

		public ZString Schema { get; set; }
		public ZString Name { get; set; }
		public ZString StartLsn { get; set; }
		public ZString EndLsn { get; set; }
		public ZString MinTableLsn { get; set; }
		public ZString LossType { get; set; }
	}
}
