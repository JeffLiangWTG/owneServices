using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Data;
using Enterprise.BusinessObjectGenerator;

namespace Enterprise.Builder.Generator
{
	public class AddInfoBizObjGenerator : SingleBizObjGenerator
	{
		public AddInfoBizObjGenerator(
			string fileNameOfBusinessObject,
			string viewName,
			string tableName,
			GeneratorOutputDirectory outputDirectory,
			string oldPrefix = "",
			string oldParentSchema = "",
			string childTableName = "",
			string childForeignKey = "")
			: base(fileNameOfBusinessObject, outputDirectory, false)
		{
			this.viewName = viewName;
			this.tableName = tableName;
			this.oldPrefix = oldPrefix;
			this.oldParentSchema = oldParentSchema;
			var contextGenerator = new BusinessObjectGenerator.ModelView.ModelViewContextGenerator(outputDirectory.CWSharedSourceDirectory);
			var modelView = contextGenerator
				.GetModelFiles(Path.Combine(outputDirectory.CWSharedSourceDirectory, "CargoWise.DbUpgrader\\src\\Scripts\\Scripts.Definitions"))
				.FirstOrDefault(f => f.Contains(viewName));
			if (modelView != null)
			{
				parentView = contextGenerator.GetCodeGeneratorContext(modelView).ModelView.ParentView;
			}

			this.childTableName = childTableName;
			this.childForeignKey = childForeignKey;
		}

		readonly string tableName;
		readonly string viewName;
		readonly string oldPrefix;
		readonly string oldParentSchema;
		readonly string parentView;
		readonly string childTableName;
		readonly string childForeignKey;

		public override string[] ListOfFilesToBeGenerated
		{
			get
			{
				return new string[]
				{
					FileNameOfBusinessObject,
					FileNameOfBusinessObjectSchema,
					FileNameOfBusinessObjectValidation,
					"No Concrete Validations file is generated for AddInfo business objects.",
					FileNameOfBusinessObjectLookups,
					"No Concrete Lookups file is generated for AddInfo business objects."
				};
			}
		}

		public override string FileNameOfBusinessObjectSchema
		{
			get
			{
				var schemasPath = Path.Combine(
					outputDirectory.CWSharedSourceDirectory,
					IsCommonBizo
						? @"CargoWise.DbUpgrader\src\Database\CargoWise.Odyssey.Schema\CargoWise.Odyssey.Schema\DummySchema"
						: @"CargoWise.DbUpgrader\src\Database\CargoWise.Odyssey.Schema\CargoWise.Odyssey.Schema\Schemas");
				var fileName = viewName + "Schema.cs";
				return Path.Combine(schemasPath, fileName);
			}
		}

		protected override bool IsRegenRequiredForValidationConcreteClass() => false;

		protected override bool IsRegenRequiredForLookups() => Info.Table.Columns.OfType<DataColumn>().Any(c => c.ColumnName.IndexOf("_NK") > -1)  && base.IsRegenRequiredForLookups();

		protected override bool IsRegenRequiredForLookupsConcreteClass() => false;

		public override string TableName => viewName;

		protected virtual string FullUnderlyingTableName
		{
			get
			{
				return ActualDatabaseNameDuringRegen + "." + SqlSchemaName + "." + tableName;
			}
		}

		protected string FullUnderlyingTableNameOnly
		{
			get
			{
				var fullTableNames = FullUnderlyingTableName.Split('.');
				return fullTableNames[2];
			}
		}

		#region CodeCollection

		protected internal override AutoBusinessObjectCodeCollection CodeCollection
		{
			get
			{
				if (fCodeCollection == null)
				{
					fCodeCollection = new AutoAddInfoBusinessObjectCodeCollection(Info);
				}
				return fCodeCollection;
			}
		}

		AutoBusinessObjectCodeCollection fCodeCollection;

		#endregion

		#region BusinessObject Info

		protected new AddInfoBusinessObjectInfo Info
			=> fInfo ??= new(
				base.Info,
				oldPrefix,
				GetBaseValidationClassName(),
				GetBaseLookupsClassName(),
				tableName,
				oldParentSchema,
				parentView,
				childTableName,
				GetChildTableProperties());

		string GetBaseValidationClassName() => BaseClassFromBusinessObjectCSharpFile(FileNameOfBusinessObjectValidation, "BusinessObjectValidation");

		string GetBaseLookupsClassName() => BaseClassFromBusinessObjectCSharpFile(FileNameOfBusinessObjectLookups, "ZLookup");

		AutoProperty[] GetChildTableProperties()
		{
			if (string.IsNullOrEmpty(childTableName))
			{
				return Array.Empty<AutoProperty>();
			}

			var childBizObjGenerator = new ChildBizObjGenerator($"Auto{childTableName}.cs", outputDirectory);
			return new AutoPropertyList(childBizObjGenerator.Info).Properties.Where(IncludeChildTableProperty).ToArray();
		}

		bool IncludeChildTableProperty(AutoProperty property)
		{
			return
				property.ColumnName != childForeignKey
				&& !property.ColumnName.EndsWith("_ClusterKey")
				&& !property.ColumnName.EndsWith("_IsValid");
		}

		#endregion

		protected override void OverrideTableNameIfNeeded(DataTable result)
		{
			result.TableName = FullUnderlyingTableNameOnly;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected override Dictionary<string, string> DbTypesFromDatabaseSchema
		{
			get
			{
				if (fDbTypesFromDatabaseSchema == null)
				{
					fDbTypesFromDatabaseSchema = new Dictionary<string, string>();

					if (FullTableName != null)
					{
						var sqlText =
							string.Format(
							$@"SELECT	c.name ColumnName,
										t.name DbType
							FROM		{FullTableDatabaseName}.sys.types t
							JOIN		{FullTableDatabaseName}.sys.columns c on c.user_type_id = t.user_type_id
							JOIN		{FullTableDatabaseName}.sys.objects o on o.object_id = c.object_id
							WHERE		c.is_computed = 0 AND o.name = '{FullTableNameOnly}'
							AND			c.name NOT IN (
											SELECT c.name
											FROM {FullTableDatabaseName}.sys.columns c
											JOIN {FullTableDatabaseName}.sys.objects o on o.object_id = c.object_id
											WHERE o.name in ('{FullUnderlyingTableNameOnly}'{(!string.IsNullOrEmpty(parentView) ? $", '{parentView}'" : "")})
											AND (c.name not like '__[_]PK' AND c.name not like '___[_]PK')
											AND (c.name not like '__[_]ClusterKey' AND c.name not like '___[_]ClusterKey')
							)");

						var schemaTable = new DataTable();

						using (var adapter = Db.Connection.Command(sqlText).NewDataAdapter())
						{
							adapter.Fill(schemaTable);
						}

						foreach (DataRow row in schemaTable.Rows)
						{
							var c = row["ColumnName"].ToString();
							var t = row["DbType"].ToString();
							if (!fDbTypesFromDatabaseSchema.ContainsKey(c))
							{
								fDbTypesFromDatabaseSchema.Add(c, t);
							}
							else
							{
								throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Key already exists in dictionary. Table={0}; Column={1}; Type={2}; SQL=\r\n {3}\r\n", FullTableName, c, t, sqlText));
							}
						}
					}
				}

				return fDbTypesFromDatabaseSchema;
			}
		}

		AddInfoBusinessObjectInfo fInfo;
		Dictionary<string, string> fDbTypesFromDatabaseSchema;

		sealed class ChildBizObjGenerator : SingleBizObjGenerator
		{
			public ChildBizObjGenerator(string fileNameOfBusinessObject, GeneratorOutputDirectory outputDirectory) : base(fileNameOfBusinessObject, outputDirectory)
			{
			}

			public new BusinessObjectInfo Info => base.Info;
		}
	}
}
