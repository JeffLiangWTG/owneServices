using System;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class RefPacksUpgradeTask : EmbeddedUpgradeTask
	{
		public RefPacksUpgradeTask() : base(new RefPacksDataFile())
		{
		}

		internal RefPacksUpgradeTask(EmbeddedDataFile resourceDataFile) : base(resourceDataFile)
		{
		}

		#region Overrides

		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			if (DoesntConflictWithAnotherRow(sourceRow))
			{
				base.UpdateColumn(columnName, targetRow, sourceRow);
			}
		}

		protected override void DoInsert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			if (DoesntConflictWithAnotherRow(sourceRow))
			{
				base.DoInsert(sourceRow, targetTable, ref targetIndex);
			}
		}

		// Only delete system rows
		protected override void DoDelete(DataRow targetRow, ref int targetIndex)
		{
			if ((bool)targetRow[RefPacksSchema.Constants.RP_IsSystem])
			{
				DeleteRefPacks((Guid)targetRow[RefPacksSchema.Constants.PK]);
			}

			targetIndex++;
		}

		void DeleteRefPacks(Guid pk)
		{
			string sqlText = "DELETE dbo.RefPacks WHERE RP_PK = @Pk";
			DbCommand cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@Pk", pk, RefPacksSchema.PK);
			cmd.ExecuteNonQuery();
		}

		bool DoesntConflictWithAnotherRow(DataRow sourceRow)
		{
			if (sourceRow.Table != null)
			{
				// Can't use ZQuery as it contains a bug for GUIDs (see eDocs on WI00127615).
				var sb = new StringBuilder();
				sb.Append("select top 1 * from dbo.refpacks where 1=1 ");
				foreach (DataColumn col in sourceRow.Table.Columns)
				{
					switch (col.ColumnName)
					{
						case RefPacksSchema.Constants.PK:
							sb.AppendFormat(CultureInfo.InvariantCulture, " AND {0} != '{1}'", RefPacksSchema.PK.Name, sourceRow[col.ColumnName]);
							break;
						case RefPacksSchema.Constants.RP_CommercialPack:
							sb.AppendFormat(CultureInfo.InvariantCulture, " AND {0} = '{1}'", RefPacksSchema.RP_CommercialPack.Name, sourceRow[col.ColumnName]);
							break;
						case RefPacksSchema.Constants.RP_CustomsPack:
							sb.AppendFormat(CultureInfo.InvariantCulture, " AND {0} = '{1}'", RefPacksSchema.RP_CustomsPack.Name, sourceRow[col.ColumnName]);
							break;
						case RefPacksSchema.Constants.RP_CustomsCountry:
							sb.AppendFormat(CultureInfo.InvariantCulture, " AND {0} = '{1}'", RefPacksSchema.RP_CustomsCountry.Name, sourceRow[col.ColumnName]);
							break;
						case RefPacksSchema.Constants.RP_OH_Supplier:
							if (string.IsNullOrEmpty(sourceRow[col.ColumnName].ToString()))
							{
								sb.AppendFormat(CultureInfo.InvariantCulture, " AND {0} is null", RefPacksSchema.RP_OH_Supplier.Name);
							}
							else
							{
								sb.AppendFormat(CultureInfo.InvariantCulture, " AND {0} = '{1}'", RefPacksSchema.RP_OH_Supplier.Name, sourceRow[col.ColumnName]);
							}
							break;
						case RefPacksSchema.Constants.RP_Type:
							sb.AppendFormat(CultureInfo.InvariantCulture, " AND {0} = '{1}'", RefPacksSchema.RP_Type.Name, sourceRow[col.ColumnName]);
							break;
					}
				}
				var existingRowThatWouldViolateUnqiueKey = Db.Connection.ExecuteScalar(sb.ToString());
				return existingRowThatWouldViolateUnqiueKey == null;
			}
			return true;
		}

		#endregion
	}
}
