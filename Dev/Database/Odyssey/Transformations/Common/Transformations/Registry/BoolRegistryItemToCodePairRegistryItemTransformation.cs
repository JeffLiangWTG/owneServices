using System;
using System.Data;
using System.Text;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	/// <summary>
	/// Transforms a BooleanRegistryItem to a CodePairRegistryItem and replaces true/false value with values from the new code description pair list
	/// </summary>
	public abstract class BoolRegistryItemToCodePairRegistryItemTransformation : RegistryDataTransformation
	{
		public abstract string RegistryItemName { get; }

		public abstract string ReplaceTrueWith { get; }

		public abstract string ReplaceFalseWith { get; }

		protected sealed override void OfflinePostUpgradeTransform()
		{
			var table = GetDataTable(RegistryItemName);
			if (table != null && table.Rows.Count > 0)
			{
				foreach (DataRow row in table.Rows)
				{
					var binaryValue = row[StmDataSchema.Constants.SD_BinaryValue] as byte[];
					var dataType = row[StmDataSchema.Constants.SD_Type] as string;

					if (binaryValue != null && binaryValue.Length > 0 && dataType == "BOL")
					{
						var existingValue = Encoding.Unicode.GetString(binaryValue) == "True";

						var newValue = existingValue
							? ReplaceTrueWith
							: ReplaceFalseWith;

						var dataRowPk = (Guid)row[StmDataSchema.Constants.PK];
						UpdateDatabaseValue(dataRowPk, "STR", Encoding.Unicode.GetBytes(newValue));
					}
				}
			}
		}
	}
}
