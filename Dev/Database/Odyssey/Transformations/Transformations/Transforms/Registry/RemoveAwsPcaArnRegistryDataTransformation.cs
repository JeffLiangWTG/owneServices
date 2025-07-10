using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Registry
{
	sealed class RemoveAwsPcaArnRegistryDataTransformation : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { AWSPCAAccessKey, Arn, AWSPCASecretKey, AWSPCAArnListManager };
		}

		public const string AWSPCAAccessKey = "AWSPCAAccessKey";
		public const string Arn = "Arn";
		public const string AWSPCASecretKey = "AWSPCASecretKey";
		public const string AWSPCAArnListManager = "AWSPCAArnListManager";
	}
}
