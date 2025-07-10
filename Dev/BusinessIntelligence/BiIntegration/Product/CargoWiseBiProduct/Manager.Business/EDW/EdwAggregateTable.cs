using CargoWise.Types;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class EdwAggregateTable : EdwTable
	{
		public EdwAggregateTable(ZString schema, ZString name)
			: base(schema, name)
		{
		}

		public ZLong InitialLoadRecordCount { get; set; }
		public ZLong InitialLoadDuration { get; set; }
		public ZLong IncrementalInsertRecordCount { get; set; }
		public ZLong IncrementalInsertDuration { get; set; }
		public ZLong IncrementalDeleteRecordCount { get; set; }
		public ZLong IncrementalDeleteDuration { get; set; }
		public ZBool IsIndexReorganized { get; set; }
		public ZString IndexReorganizationErrorMessage { get; set; }
	}
}
