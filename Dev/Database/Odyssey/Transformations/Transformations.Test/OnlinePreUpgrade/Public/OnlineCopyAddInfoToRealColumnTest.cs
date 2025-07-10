using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing
{
	[TestsSubclassesOf(typeof(OnlineCopyAddInfoToRealColumn<>))]
	[UseSnapshotProtection]
	public abstract class OnlineCopyAddInfoToRealColumnTest<T, TOnline> : TestCase
		where T : Transformation.DataModification.AddInfoTransformationBase.CopyAddInfoToRealColumn
		where TOnline : OnlineCopyAddInfoToRealColumn<T>
	{
		public void TestEnsureThatTransformationIsInMappingForTesting()
		{
			using (CreateTemplateDB())
			{
				var testTransformation = CreateOnlineTransformation();
				AssertNoExceptionThrown(testTransformation.Run);
				var transformationType = testTransformation.GetType();
				var mappings = ((IEnumerable<Mapping>)transformationType.GetMethod("GetAllMappings", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(testTransformation, Array.Empty<object>())).Select(x => x.TransformationType).ToHashSet();
				var expectedType = typeof(T);
				if (!mappings.Any(x => expectedType.IsAssignableFrom(x)))
				{
					Fail($"Please ensure that {expectedType.FullName} is included in {typeof(Mapper).FullName}");
				}
				else
				{
					var actualMappings = Mapper.GetAllMappings().Select(x => x.TransformationType).ToHashSet();
					var offlineTransformationType = testTransformation.OfflineTransformation.GetType();
					if (mappings.Contains(offlineTransformationType))
					{
						mappings.Remove(offlineTransformationType);
					}
					if (actualMappings.Contains(offlineTransformationType))
					{
						actualMappings.Remove(offlineTransformationType);
					}
					AssertContainsExactElementsInAnyOrder("GetAllMappings should include all mappings from Mapper.GetAllMappings()", actualMappings, mappings);
				}
			}
		}

		protected void MarkAllAddInfoPropertiesAsAlreadyProcessedExcluding(params string[] excludedAddInfoPropertyNames)
		{
			var excludedNames = excludedAddInfoPropertyNames.ToHashSet();
			var addInfoColumnMapping = (IDictionary<SchemaColumn, (string AddInfoPropertyName, string SelectStatementOverride, string AddInfoValueJointStatement)>)OfflineTransformation.GetType().GetMethod("GetAddInfoColumnMapping", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(OfflineTransformation, Array.Empty<object>());

			var data = string.Join(System.Environment.NewLine, addInfoColumnMapping.Values.Select(x => x.AddInfoPropertyName).Where(x => !excludedNames.Contains(x)));
			OfflineTransformation.UpdateAlreadyProcessedData(data);
		}

		protected void UpdateAlreadyProcessedData(params string[] addInfoPropertyNames)
		{
			var data = string.Join(System.Environment.NewLine, addInfoPropertyNames);
			OfflineTransformation.UpdateAlreadyProcessedData(data);
		}

		protected void DeleteAlreadyProcessedData()
		{
			UpdateAlreadyProcessedData(Array.Empty<string>());
		}

		protected abstract void AssertFirstRunResult(TOnline testTransformation, string[] logs);
		protected abstract void AssertSecondRunResult(TOnline testTransformation, string[] logs);
		protected virtual ITableSchema TargetTableSchema => throw new NotImplementedException();
		protected virtual IDisposable CreateTemplateDB()
		{
			var targetTableSchema = TargetTableSchema;
			var tableName = targetTableSchema.TableName;
			var columns = targetTableSchema.All.Select(column =>
			{
				var defaultValue = string.Empty;
				var typeDeclaration = column.SqlDbTypeDeclaration;
				if (column.IsSparse)
				{
					defaultValue = " SPARSE NULL";
					if (typeDeclaration.Equals("varchar(1)", StringComparison.OrdinalIgnoreCase))
					{
						typeDeclaration = "char(1)";
					}
				}
				else if (column.IsNullable)
				{
					defaultValue = " NULL";
				}
				else
				{
					defaultValue = " NOT NULL";
					if (column.SqlDbDefault != DBNull.Value && column.ColumnType != SchemaColumnType.Guid)
					{
						var value = column is SchemaNumericColumn numericColumn ? column.SqlDbDefault.ToString() :
										column is SchemaBoolColumn boolColumn ? (boolColumn.IsBitField ? "0" : "'N'") : $"'{column.SqlDbDefault.ToString()}'";
						defaultValue += $" DEFAULT {value}";
					}
				}
				return $"[{column.Name}] {typeDeclaration}{defaultValue}";
			});
			return CreateTemplateDBCore((tableName, columns));
		}

		protected IDisposable CreateTemplateDBCore(params (string targetTableName, IEnumerable<string> columnList)[] targetTables)
		{
			var templateDbCreator = new TablePreSynchroniserTestTemplateDbCreator(TemplateDb, targetTables);
			templateDbCreator.CreateDropExisting();
			TablePreSynchroniser.CreatePreAddDb_ForTest();
			return new DisposableAction(() =>
			{
				TablePreSynchroniser.DropPreAddDb_ForTest();
				templateDbCreator.Drop();
			});
		}

		protected SchemaColumn SetupTargetColumnWithDifferentDefinition(SchemaColumn column, string columnDefinition, string columnDefault)
		{
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, column.TableName, column.Name, columnDefinition, columnDefault);
			return column;
		}

		protected void DropColumnAndDependency(SchemaColumn column)
		{
			new DbColumnDependencyRemover(column.TableName, column.Name).DropRelateObjects(Db.Connection);
			DBTransformationTestHelper.DropColumnIfExists(column.TableName, column.Name);
		}

		protected void UpdateAddInfo(Guid objPK, string addInfo)
		{
			var updateSql = $@"UPDATE {OfflineTransformation.SourceTableName} SET {OfflineTransformation.SourceAddInfoColumn.Name} = @addInfo WHERE {OfflineTransformation.SourceTablePK.Name} = @pk";
			var cmd = Db.Connection.Command(updateSql);
			cmd.AddParameter("@addInfo", SqlDbType.VarChar, addInfo);
			cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, objPK);
			cmd.ExecuteNonQuery();
		}
		protected abstract TOnline CreateOnlineTransformation();
		protected abstract void CreateTestData();
		protected string OffLineProcessingTableName => OfflineTransformation.OffLineProcessingTableName;
		protected string InsertUpdateAddInfoSourceTriggerName => insertUpdateAddInfoSourceTriggerName ?? (insertUpdateAddInfoSourceTriggerName = $"DBUPG_TG_{OffLineProcessingTableName}");
		string insertUpdateAddInfoSourceTriggerName;
		protected string UpdateClusterKeySourceTriggerName => updateClusterKeySourceTriggerName ?? (updateClusterKeySourceTriggerName = $"DBUPG_TG_{OffLineProcessingTableName}_{OfflineTransformation.GetClusterKeyColumn(OfflineTransformation.SourceAddInfoColumn).Name}");
		string updateClusterKeySourceTriggerName;

		protected T OfflineTransformation => offlineTransformation ?? (offlineTransformation = GetTransformation());
		T offlineTransformation;

		T GetTransformation()
		{
			var result = Activator.CreateInstance<T>();
			return result;
		}

		protected int GetRecordCount(Guid pk, SchemaGuidColumn guidColumn)
		{
			var sqlClientCountText = $"SELECT COUNT(*) FROM {guidColumn.TableName.QuoteName()} WHERE {guidColumn.Name.QuoteName()} = @pk";
			var cmd = Db.Connection.Command(sqlClientCountText);
			cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
			return (int)cmd.ExecuteScalar();
		}

		protected int GetOffLineProcessingTableCount(Guid pk)
		{
			var sqlClientCountText = $"SELECT COUNT(*) FROM {OffLineProcessingTableName} WHERE PK = @pk";
			var cmd = Db.Connection.Command(sqlClientCountText);
			cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
			return (int)cmd.ExecuteScalar();
		}

		protected override void SetUp()
		{
			base.SetUp();
			manager = new UpgradeManagerForTestWithOutputBuffer();
		}

		protected string TemplateDb => templateDb ?? (templateDb = $"testOnlineCopyAddInfoToRealColumn_{Db.Connection.CurrentDatabase}_TemplateDb");
		string templateDb;
		protected UpgradeManagerForTestWithOutputBuffer manager;
	}
}
