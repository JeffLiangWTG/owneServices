using CargoWise.Types;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class EdwCustomTable : EdwTable
	{
		public EdwCustomTable(ZString schema, ZString name)
			: base(schema, name)
		{
		}

		public ZLong InitialTransformRecordCount { get; set; }
		public ZLong InitialTransformDuration { get; set; }
		public ZLong MergeTransformInsertRecordCount { get; set; }
		public ZLong MergeTransformInsertDuration { get; set; }
		public ZLong MergeTransformDeleteRecordCount { get; set; }
		public ZLong MergeTransformDeleteDuration { get; set; }
		public ZBool IsIndexReorganized { get; set; }
		public ZString IndexReorganizationErrorMessage { get; set; }
	}
}
