using Enterprise.DbUpgrader.Transformations.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing
{
	[TestedType(typeof(RemoveAwsPcaArnRegistryDataTransformation))]
	public class RemoveAwsPcaArnRegistryDataTransformationTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { RemoveAwsPcaArnRegistryDataTransformation.AWSPCAAccessKey, RemoveAwsPcaArnRegistryDataTransformation.Arn, RemoveAwsPcaArnRegistryDataTransformation.AWSPCASecretKey, RemoveAwsPcaArnRegistryDataTransformation.AWSPCAArnListManager };
		}
	}
}
