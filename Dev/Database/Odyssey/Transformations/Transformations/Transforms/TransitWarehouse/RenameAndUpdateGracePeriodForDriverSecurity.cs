using System;
using System.Data;
using System.Globalization;
using System.Text;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class RenameAndUpdateGracePeriodForDriverSecurity : RegistryDataTransformation
	{
		public override string UserDescription => "Rename and Update GracePeriodForDriverSecurity Registry";

		const string OldRegistryName = "GracePeriodForDriverSecurity";
		const string NewRegistryName = "DriverSecurityCertificationCheckingActivated";
		const string dateTimeSqlFormat = "yyyy-MM-dd HH:mm:ss.fff";

		protected override void OfflinePostUpgradeTransform()
		{
			var table = GetDataTable(OldRegistryName);
			var currDateTime = new DateTime(2025, 1, 1, 0, 0, 0);

			if (table?.Rows.Count > 0)
			{
				foreach (DataRow row in table.Rows)
				{
					if (row[StmDataSchema.Constants.SD_BinaryValue] is byte[] binaryValue &&
						binaryValue.Length > 0 &&
						row[StmDataSchema.Constants.SD_Type] as string == "DT ")
					{
						var date = Encoding.Unicode.GetString(binaryValue);
						var existingValue = DateTime.ParseExact(date, dateTimeSqlFormat, CultureInfo.InvariantCulture);
						var newValue = currDateTime >= existingValue;

						var dataRowPk = (Guid)row[StmDataSchema.Constants.PK];
						UpdateDatabaseValue(dataRowPk, "BOL", Encoding.Unicode.GetBytes(newValue ? "True" : "False"));
					}
				}

				UpdateRegistryItemName(OldRegistryName, NewRegistryName);
			}
		}
	}
}
