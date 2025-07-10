using Enterprise.ZArchitecture.Core.Test;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Test
{
	[TestedType(typeof(ClusterKeyMetaDataAttribute))]
	sealed class ClusterKeyMetaDataAttributeTest : AssemblyMetaDataAttributeTestCase<ClusterKeyMetaDataAttribute>
	{
		public void TestEquals_AllPropertiesInEquals()
		{
			var attribute1 = GetAssemblyMetaDataAttributeForTesting();
			var attribute2 = GetAssemblyMetaDataAttributeForTesting();
			Assert(attribute1.Equals(attribute2));

			attribute1.ParentTableName = "ParentTable";
			Assert(!attribute1.Equals(attribute2));

			attribute2.ParentTableName = "ParentTable";
			Assert(attribute1.Equals(attribute2));

			attribute1.ParentFkColumnName = "ParentFk";
			Assert(!attribute1.Equals(attribute2));

			attribute2.ParentFkColumnName = "ParentFk";
			Assert(attribute1.Equals(attribute2));

			attribute1.SecondaryParentTableName = "SecondaryParentTable";
			Assert(!attribute1.Equals(attribute2));

			attribute2.SecondaryParentTableName = "SecondaryParentTable";
			Assert(attribute1.Equals(attribute2));

			attribute1.SecondaryParentFkColumnName = "SecondaryParentFk";
			Assert(!attribute1.Equals(attribute2));

			attribute2.SecondaryParentFkColumnName = "SecondaryParentFk";
			Assert(attribute1.Equals(attribute2));

			attribute1.TableName = "Table";
			Assert(!attribute1.Equals(attribute2));

			attribute2.TableName = "Table";
			Assert(attribute1.Equals(attribute2));
		}
	}
}
