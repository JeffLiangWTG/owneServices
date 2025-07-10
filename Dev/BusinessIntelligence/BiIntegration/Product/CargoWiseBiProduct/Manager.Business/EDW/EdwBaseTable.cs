using CargoWise.Types;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class EdwBaseTable : EdwTable
	{
		public EdwBaseTable(ZString schema, ZString name)
			: base(schema, name)
		{
		}

		public ZInt TransformId { get; set; }
		public ZInt DependencyOrder { get; set; }
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
