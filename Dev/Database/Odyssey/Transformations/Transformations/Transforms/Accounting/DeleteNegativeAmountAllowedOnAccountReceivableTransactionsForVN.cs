using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	public class DeleteNegativeAmountAllowedOnAccountReceivableTransactionsForVN : RegistryDataTransformation
	{
		const string RegistryItemName = "NegativeAmountAllowedOnAccountReceivableTransactions";

		public override string UserDescription => @"Delete record of NegativeAmountAllowedOnAccountReceivableTransactions registry item which value is override to ALL for VN company";

		protected override void OfflinePostUpgradeTransform()
		{
			var table = GetDataTable(RegistryItemName);
			if (table?.Rows.Count > 0)
			{
				var vNCompanyPks = GetVNCompanyPK();
				var helper = new RegistryTransformationHelper();

				foreach (DataRow row in table.Rows)
				{
					var binaryValue = row[StmDataSchema.Constants.SD_BinaryValue] as byte[];
					var company = (Guid)row[StmDataSchema.Constants.SD_Owner];
					if (vNCompanyPks.Contains(company) && binaryValue != null && binaryValue.Length > 0)
					{
						var originalValue = Encoding.Unicode.GetString(binaryValue);
						if (originalValue != "NAL")
						{
							helper.DeleteStmDataRow((Guid)row[StmDataSchema.Constants.PK]);
						}
					}
				}
			}
		}

		static List<Guid> GetVNCompanyPK()
		{
			var result = new List<Guid>();

			var sql = @"
SELECT
	GC_PK
FROM
	dbo.GlbCompany
	JOIN
	dbo.StmData ON SD_Owner = GC_PK AND SD_Name = 'EnableEInvoicingFunctionality'
WHERE
	GC_RN_NKCountryCode = 'VN'
	AND
	CONVERT(NVARCHAR(MAX), SD_BinaryValue) = 'True'";
			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					result.Add(reader.GetGuid(0));
				}
			}

			return result;
		}
	}
}
