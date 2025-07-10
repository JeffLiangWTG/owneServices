using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core
{
	class RemoveDummyContacts : DataTransformation
	{
		public override string UserDescription => "Remove Dummy Contacts";

		const string LastProcessedChunkPKName = "RemoveDummyContacts.LastProcessedChunkPK";
		const string TransformationHasRunToCompletion = "RemoveDummyContacts.TransformationRunToCompletion";
		const string TransformationHasProcessedCount = "RemoveDummyContacts.TransformationProcessedCount";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			ExtProperty.Database.Delete(Db.Connection, LastProcessedChunkPKName);
			ExtProperty.Database.Delete(Db.Connection, TransformationHasRunToCompletion);
			ExtProperty.Database.Delete(Db.Connection, TransformationHasProcessedCount);
		}
	}
}
