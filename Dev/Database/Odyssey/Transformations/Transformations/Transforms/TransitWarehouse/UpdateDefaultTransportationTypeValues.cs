using System;
using System.Data;
using System.Text;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class UpdateDefaultTransportationTypeValues : RegistryDataTransformation
	{
		public override string UserDescription => "Update DefaultTransportationType Registry values.";

		const string RegistryName = "DefaultTransportationType";

		protected override void OfflinePostUpgradeTransform()
		{
			var table = GetDataTable(RegistryName);

			if (table?.Rows.Count > 0)
			{
				foreach (DataRow row in table.Rows)
				{
					if (row[StmDataSchema.Constants.SD_BinaryValue] is byte[] binaryValue &&
						binaryValue.Length > 0 &&
						row[StmDataSchema.Constants.SD_Type] as string == "STR")
					{
						var existingValue = Encoding.Unicode.GetString(binaryValue);
						var newValue = TransformValue(existingValue);

						if (newValue != existingValue)
						{
							var dataRowPk = (Guid)row[StmDataSchema.Constants.PK];
							UpdateDatabaseValue(dataRowPk, "STR", Encoding.Unicode.GetBytes(newValue));
						}
					}
				}
			}
		}

		static string TransformValue(string existingValue)
		{
			return existingValue switch
			{
				"CON" => "CNT",
				"VIC" => "VEH",
				_ => existingValue
			};
		}
	}
}
