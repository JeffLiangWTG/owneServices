using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing
{
	class EncryptValueInRegistryTransformForTest : EncryptValueInRegistryTransform
	{
		public const string GoodRegItem = "goodRegItem";
		public const string NullRegItem = "nullRegItem";

		public override string[] RegistryNamesToEncrypt => new[] { GoodRegItem, NullRegItem };
	}
}
