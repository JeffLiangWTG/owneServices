using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations
{
	sealed class TransformationDirectorForTesting : TransformationDirector
	{
		public TransformationDirectorForTesting()
			: base(new DummyUpgradeManager())
		{
		}

		public DataTransformation[] Transformations_Exposed
		{
			get { return Transformations; }
		}

		#region DataCopy Methods Exposed For Testing

		public void CreateDataCopyDb_Exposed()
		{
			CreateDataCopyDb();
		}

		public void DropDataCopyDb(AdminConnection conn)
		{
			DataCopyDbCreator.Drop(conn);
		}

		#endregion
	}
}
