using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework.TestHelper;

namespace Enterprise.ZArchitecture.Business.ClusterKey.Testing
{
	public abstract class ClusterKeyEntityTest : TestCaseWithFactory
	{
		public void TestEntityIsEnterpriseBusinessObject()
		{
			Assert("ClusterKey Entity must be an EnterpriseBusinessObject", typeof(EnterpriseBusinessObject).IsAssignableFrom(TestedTypeHelper.GetTestedType(GetType())));
		}

		public void TestEntityIsFirstSubclassOfItsAutoBusinessObject()
		{
			AssertEquals("BaseType Name", $"Auto{ClusterKeyEntityToTest.ClusterKeyPty.BizObj.TableName}", TestedTypeHelper.GetTestedType(GetType()).BaseType.Name);
		}

		protected bool IsColumnNullable(ZPropertyInfoGuid fkPty)
		{
			return IsColumnNullable(fkPty.BizObj.TableName, fkPty.Name);
		}

		bool IsColumnNullable(string tableName, string columnName)
		{
			var sql = $@"
				SELECT c.is_nullable
				FROM sys.tables AS t
				INNER JOIN sys.columns AS c ON c.object_id = t.object_id
				WHERE t.name = '{tableName}'
				AND c.name = '{columnName}'";

			return Convert.ToBoolean(TestConnection.ExecuteScalar(sql));
		}

		protected void AssertUpdateClusterKeyException(EnterpriseBusinessObject bizObj, int keyValue, string assertMsgSuffix = null)
		{
			var sql = $"UPDATE {bizObj.TableName} SET {ClusterKeyEntityToTest.ClusterKeyPty.Name} = {keyValue} WHERE {bizObj.PKSchemaColumn.Name} = '{bizObj.PK}'";

			AssertExceptionThrown(
				$"Attempt to set ClusterKey = {keyValue} in the database {assertMsgSuffix}",
				typeof(SqlException),
				$"The UPDATE statement conflicted with the CHECK constraint \"Constraint_{ClusterKeyEntityToTest.ClusterKeyPty.Name}\"",
				() => TestConnection.ExecuteNonQuery(sql),
				assertStartsWith: true);
		}

		protected EnterpriseBusinessObject ClusterKeyEntityToTestAsBizObj => ClusterKeyEntityToTest as EnterpriseBusinessObject;

		protected IClusterKeyEntity ClusterKeyEntityToTest
		{
			get
			{
				if (clusterKeyEntityToTest == null)
				{
					clusterKeyEntityToTest = NewClusterKeyEntity();
				}

				return clusterKeyEntityToTest;
			}
		}

		IClusterKeyEntity clusterKeyEntityToTest;

		protected abstract IClusterKeyEntity NewClusterKeyEntity();
	}
}
