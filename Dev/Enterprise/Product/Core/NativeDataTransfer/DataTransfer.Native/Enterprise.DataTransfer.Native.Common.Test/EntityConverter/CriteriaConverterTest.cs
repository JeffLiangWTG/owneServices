
using System.Linq;
using Enterprise.DataTransfer.Native.Common.Converters;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common.EntityConverter
{
	public class CriteriaConverterTest : TransactionedTestCase
	{
		public void TestTransformCriterias()
		{
			var pair = new EntityCriteria
						{
							EntityName = "DummyBizo",
							PropertyName = "Code",
							Value = "ZUBIN TEST ORG"
						};
			var criterias = CriteriaConverter.Convert(entitySetDefinition, new[] { pair });
			AssertEquals(1, criterias.Count());
			var result = criterias.First();
			AssertEquals("DummyBizo", result.TableName);
			AssertEquals("Z0_Code", result.ColumnName);
		}

		public void TestTransformCriterias_InvalidProperty()
		{
			var pair = new EntityCriteria
						{
							EntityName = "DummyBizo",
							PropertyName = "CrapColumn",
							Value = "ZUBIN TEST ORG"
						};

			AssertExceptionThrown(
				"Exception should be thrown when property name is invalid",
				typeof(NativeXMLUserVisibleException),
				() => CriteriaConverter.Convert(entitySetDefinition, new[] { pair }));
		}

		public void TestTransformCriterias_EmptyCriteria()
		{
			var criterias = CriteriaConverter.Convert(entitySetDefinition, System.Array.Empty<EntityCriteria>());
			AssertEquals("Empty collection should be returned", 0, criterias.Count());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestUtil.AlterDummyTable();
			entitySetDefinition = TestUtil.GetEntitySetDefinition("Dummy");
		}

		#endregion

		EntitySetDefinition entitySetDefinition;
	}
}
