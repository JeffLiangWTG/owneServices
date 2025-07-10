using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	class KeyDefinitionTest : TransactionedTestCase
	{
		public void TestProperties()
		{
			var ckDefinition = new KeyDefinition(JobComInvoiceHeaderSchema.PK, JobComInvoiceLineSchema.JI_JZ);

			CombineAssertions(() =>
			{
				AssertEquals("ChildTableName", "JobComInvoiceLine", ckDefinition.ChildTableName);
				AssertEquals("ChildFkColumnName", "JI_JZ", ckDefinition.ChildFkColumnName);
				AssertEquals("ChildClusterKey", "JI_ClusterKey", ckDefinition.ChildClusterKey);
				AssertEquals("ParentTableName", "JobComInvoiceHeader", ckDefinition.ParentTableName);
				AssertEquals("ParentPkColumnName", "JZ_PK", ckDefinition.ParentPkColumnName);
				AssertEquals("ParentClusterKey", "JZ_ClusterKey", ckDefinition.ParentClusterKey);
				AssertEquals("IsMidLevelMaster default value", false, ckDefinition.IsMidLevelMaster);
			});

			ckDefinition = new KeyDefinition(JobDeclarationSchema.PK, JobComInvoiceHeaderSchema.JZ_JE, true);

			CombineAssertions(() =>
			{
				AssertEquals("ChildTableName", "JobComInvoiceHeader", ckDefinition.ChildTableName);
				AssertEquals("ChildFkColumnName", "JZ_JE", ckDefinition.ChildFkColumnName);
				AssertEquals("ChildClusterKey", "JZ_ClusterKey", ckDefinition.ChildClusterKey);
				AssertEquals("ParentTableName", "JobDeclaration", ckDefinition.ParentTableName);
				AssertEquals("ParentPkColumnName", "JE_PK", ckDefinition.ParentPkColumnName);
				AssertEquals("ParentClusterKey", "JE_ClusterKey", ckDefinition.ParentClusterKey);
				AssertEquals("IsMidLevelMaster", true, ckDefinition.IsMidLevelMaster);
			});
		}
	}
}
