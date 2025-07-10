using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.Common.Testing
{
	sealed class TransformationIndexProviderTest : TransactionedTestCase
	{
		public void TestAdd()
		{
			var provider = new TransformationIndexProvider(transformationForTable1);
			provider.New(SchemaName, TableName_1).Key("T1_Col1").GetInfo();

			var dummyProvider = new TransformationIndexProvider(new DescriptionOnlyTransformation($"{DummyBizoSchema.Constants.SqlSchemaName}_{DummyBizoSchema.Constants.TableName}"));
			dummyProvider.Add(provider);
			dummyProvider.New(DummyBizoSchema.Instance).Key(DummyBizoSchema.Constants.Z0_Code).GetInfo();

			var newProvider = new TransformationIndexProvider(transformationForTable2);
			newProvider.Add(dummyProvider);
			newProvider.New(SchemaName, TableName_2).Key("T2_Col1").GetInfo();

			var expected = new string[]
			{
				$"NONCLUSTERED INDEX [{IndexInfo.MANUALLY_CREATED_WTG_INDEX_PREFIX}_{SchemaName}_{TableName_1}_1] ON [{SchemaName}].[{TableName_1}] ([T1_Col1]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
				$"NONCLUSTERED INDEX [{IndexInfo.MANUALLY_CREATED_WTG_INDEX_PREFIX}_{DummyBizoSchema.Constants.SqlSchemaName}_{DummyBizoSchema.Constants.TableName}_2] ON [{DummyBizoSchema.Constants.SqlSchemaName}].[{DummyBizoSchema.Constants.TableName}] ([{DummyBizoSchema.Constants.Z0_Code}]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
				$"NONCLUSTERED INDEX [{IndexInfo.MANUALLY_CREATED_WTG_INDEX_PREFIX}_{SchemaName}_{TableName_2}_3] ON [{SchemaName}].[{TableName_2}] ([T2_Col1]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			};

			AssertContainsExactElementsInAnyOrder(expected, newProvider.Select(index => index.Definition));
		}

		public void TestCreateIndexes()
		{
			var indexName_1 = $"{IndexInfo.MANUALLY_CREATED_WTG_INDEX_PREFIX}_{SchemaName}_{TableName_1}_1";
			var indexName_2 = $"{IndexInfo.MANUALLY_CREATED_WTG_INDEX_PREFIX}_{SchemaName}_{TableName_1}_2";
			var indexName_3 = $"{IndexInfo.MANUALLY_CREATED_WTG_INDEX_PREFIX}_{SchemaName}_{TableName_2}_3";

			PrepareDataForTest();
			IndexInfo.Builder.New(SchemaName, TableName_2, indexName_3).Key("T2_Col1", "T2_Col2").GetInfo().Create(TestConnection);

			var provider = new TransformationIndexProvider(transformationForTable1);
			provider.New(SchemaName, TableName_1).Key("T1_Col1").GetInfo();
			provider.New(SchemaName, TableName_1).Key("T1_Col2", "T1_Col1").GetInfo();

			var finalProvider = new TransformationIndexProvider(transformationForTable2);
			finalProvider.Add(provider);
			var index_3 = finalProvider.New(SchemaName, TableName_2).Key("T2_Col1").GetInfo();

			AssertEquals("Precondition", false, IndexLoader.Exists(TestConnection, null, null, indexName_1));
			AssertEquals("Precondition", false, IndexLoader.Exists(TestConnection, null, null, indexName_2));
			AssertEquals("Precondition", true, IndexLoader.Exists(TestConnection, null, null, indexName_3));
			AssertEquals("Precondition", false, index_3.Equals(IndexLoader.LoadTop1(TestConnection, SchemaName, TableName_2, indexName_3)));

			finalProvider.CreateIndexes(TestConnection);

			AssertEquals(true, IndexLoader.Exists(TestConnection, null, null, indexName_1));
			AssertEquals(true, IndexLoader.Exists(TestConnection, null, null, indexName_2));
			AssertEquals(true, IndexLoader.Exists(TestConnection, null, null, indexName_3));
			AssertEquals(true, index_3.Equals(IndexLoader.LoadTop1(TestConnection, SchemaName, TableName_2, indexName_3)));
		}

		public void TestDropIndexes()
		{
			PrepareDataForTest();

			var indexName_1 = $"{IndexInfo.MANUALLY_CREATED_WTG_INDEX_PREFIX}_{SchemaName}_{TableName_1}_1";
			var indexName_2 = $"{IndexInfo.MANUALLY_CREATED_WTG_INDEX_PREFIX}_{SchemaName}_{TableName_1}_2";

			var provider = new TransformationIndexProvider(transformationForTable1);
			provider.New(SchemaName, TableName_1).Key("T1_Col1").GetInfo();
			provider.New(SchemaName, TableName_1).Key("T1_Col2", "T1_Col1").GetInfo();

			AssertEquals("Precondition", false, IndexLoader.Exists(TestConnection, null, null, indexName_1));
			AssertEquals("Precondition", false, IndexLoader.Exists(TestConnection, null, null, indexName_2));

			provider.CreateIndexes(TestConnection);

			AssertEquals(true, IndexLoader.Exists(TestConnection, null, null, indexName_1));
			AssertEquals(true, IndexLoader.Exists(TestConnection, null, null, indexName_2));

			provider.DropIndexes(TestConnection);

			AssertEquals(false, IndexLoader.Exists(TestConnection, null, null, indexName_1));
			AssertEquals(false, IndexLoader.Exists(TestConnection, null, null, indexName_2));
		}

		public void TestTransformation()
		{
			ITransformationIndexProvider transform = new Transformation_ForTest();

			var expected = new string[]
			{
				"NONCLUSTERED INDEX [_WTG__Some transformation to fix data_1] ON [dbo].[TableName_1] ([T1_Col1]) INCLUDE ([T1_Col2]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
				"NONCLUSTERED INDEX [_WTG__Some transformation to fix data_2] ON [dbo].[TableName_2] ([T2_Col1]) WHERE (T2_Col2 = 'CODE') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			};

			AssertContainsExactElementsInAnyOrder(expected, transform.IndexProvider.Select(index => index.Definition));
		}

		public void TestExceptionThrownOnEmptyDescription()
		{
			var transformationWithEmptyDescription = new DescriptionOnlyTransformation("");
			AssertExceptionThrown<ArgumentNullException>(() => new TransformationIndexProvider(transformationWithEmptyDescription));

			AssertExceptionThrown<ArgumentNullException>(() => new TransformationIndexProvider(null));
		}

		#region Implementation

		const string SchemaName = Db.SqlDbOwnerSchema;
		const string TableName_1 = "TableName_1";
		const string TableName_2 = "TableName_2";

		void PrepareDataForTest()
		{
			TestConnection.ExecuteNonQuery($@"
if (OBJECT_ID(N'[{SchemaName}].[{TableName_1}]', N'U') is NOT NULL) DROP TABLE [{SchemaName}].[{TableName_1}];
CREATE TABLE [{SchemaName}].[{TableName_1}]
(
	T1_Col1 bit,
	T1_Col2 bit,
);

if (OBJECT_ID(N'[{SchemaName}].[{TableName_2}]', N'U') is NOT NULL) DROP TABLE [{SchemaName}].[{TableName_2}];
CREATE TABLE [{SchemaName}].[{TableName_2}]
(
	T2_Col1 bit,
	T2_Col2 varchar(5),
);

"
				);
		}

		#endregion // Implementation

		#region Helper classes

		class Transformation_ForTest : DataTransformation, ITransformationIndexProvider
		{
			public override string UserDescription => "Some transformation to fix data";

			#region Interface ITransformationIndexProvider

			TransformationIndexProvider indexProvider;
			TransformationIndexProvider ITransformationIndexProvider.IndexProvider
			{
				get
				{
					if (indexProvider == null)
					{
						indexProvider = new TransformationIndexProvider(this);

						indexProvider.New(SchemaName, TableName_1)
							.Key("T1_Col1")
							.Include("T1_Col2")
							.GetInfo();

						indexProvider.New(SchemaName, TableName_2)
							.Key("T2_Col1")
							.Where("T2_Col2 = 'CODE'")
							.GetInfo();
					}

					return indexProvider;
				}
			}

			#endregion // Interface ITransformationIndexProvider
		}

		DescriptionOnlyTransformation transformationForTable1 => new DescriptionOnlyTransformation($"{SchemaName}_{TableName_1}");

		DescriptionOnlyTransformation transformationForTable2 => new DescriptionOnlyTransformation($"{SchemaName}_{TableName_2}");

		#endregion // Helper classes
	}
}
