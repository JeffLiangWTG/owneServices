using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.DataTransfer.Native.DB.Helpers;
using Enterprise.DataTransfer.Native.DB.Keys;

namespace Enterprise.DataTransfer.Native.Common.EntityBuilders
{
	public class DataRowEntityBuilder : EntityBuilder
	{
		public DataRowEntityBuilder(DataRow dataRow, IEntityDefinition definition, AncillaryImportServices sessionServices)
		{
			this.dataRow = dataRow;
			this.definition = definition;
			Entity = new Entity(definition, sessionServices);
		}
		readonly DataRow dataRow;
		readonly IEntityDefinition definition;

		public override void BuildInternalPK()
		{
			var pkDef = definition.Id.ColumnDef;
			Entity.InternalPK = (Guid)dataRow[pkDef.Name];
		}

		public override void BuildAction()
		{
			return;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public override void BuildProperties()
		{
			var errors = new List<string>();

			foreach (var propertyDef in definition.PropertyDefinitions)
			{
				var property = new Property(propertyDef);
				var column = propertyDef.ColumnDef;

				var key = column as PolymorphicKey;
				if (key != null)
				{
					var discriminator = key.Discriminator;
					if (discriminator != null)
					{
						if (discriminator.Type == ColumnType.TableCode)
						{
							var parentTableCode = (string)dataRow[key.Discriminator.Name];
							var parentTableName = TableNameHelper.GetTableNameFromPrefix(parentTableCode);
							key.ReferenceTable = Table.Get(parentTableName);
						}
						else if (discriminator.Type == ColumnType.TableName)
						{
							var parentTableName = (string)dataRow[key.Discriminator.Name];
							key.ReferenceTable = Table.Get(parentTableName);
						}
						else
						{
							throw new InvalidOperationException("foreignKey.Discriminator should only be set with a Type of TableCode or TableName.");
						}

						property = new Property(new PropertyDefinition(key));
					}
				}
				property.Value = dataRow[column.Name];
				var (success, error) = Entity.AddProperty(property);
				if (!success)
				{
					errors.Add(error);
				}
			}

			if (errors.Count > 0)
			{
				var sb = new StringBuilder("Validation errors building Entity:\r\n");
				errors.ForEach(s => sb.AppendLine(s));
				throw new NativeXMLUserVisibleException(sb.ToString());
			}
		}
	}
}
