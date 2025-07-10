using CargoWise.Types;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class EdwStagingTable : EdwTable
	{
		public EdwStagingTable(ZString schema, ZString name)
			: base(schema, name)
		{
		}

		public ZString SourceSchema { get; set; }
		public ZLong InitialLoadRecordCount { get; set; }
		public ZLong InitialLoadDuration { get; set; }
		public ZLong IncrementalLoadRecordCount { get; set; }
		public ZLong IncrementalLoadDuration { get; set; }
		public ZBool EnableEtl { get; set; }
	}
}
