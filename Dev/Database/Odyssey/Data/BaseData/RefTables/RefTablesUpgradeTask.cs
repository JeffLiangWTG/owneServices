using System;
using System.Data;
using System.Globalization;
using Enterprise.DbUpgrader.Data.BaseData.Common;
using Enterprise.ZArchitecture.Schema;
namespace Enterprise.DbUpgrader.Data.BaseData.RefTables
{
	public class RefTablesUpgradeTask : BaseDataUpgradeTask
	{
		public RefTablesUpgradeTask()
			: base(new RefTablesDataFile())
		{
		}

		/// <summary>
		/// Existing records are not updated
		/// </summary>
		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			// Skips update
			if (columnName == "RC_Description" && (string)targetRow["RC_Description"] == "45-foot Pan Refigerator" && (string)sourceRow["RC_Description"] == "45-foot Pan Refrigerator")
			{
				base.UpdateColumn(columnName, targetRow, sourceRow);
			}
			if (columnName == "RH_IsSystem" && SameNkExists(RefCommodityCodeSchema.RH_Code, sourceRow[RefCommodityCodeSchema.RH_Code.Name].ToString()))
			{
				base.UpdateColumn(columnName, targetRow, sourceRow);
			}
		}

		protected override void DoInsert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			bool shouldInsert = true;

			switch (targetTable.TableName)
			{
				case RefCommodityCodeSchema.Constants.TableName:
					// NK = RH_Code
					if (SameNkExists(RefCommodityCodeSchema.RH_Code, sourceRow[RefCommodityCodeSchema.RH_Code.Name].ToString()))
					{
						shouldInsert = false;
					}
					break;

				case RefContainerSchema.Constants.TableName:
					//NK = RC_Code
					if (SameCompositeRefContainerNkExists(sourceRow[RefContainerSchema.RC_Code.Name].ToString(), sourceRow[RefContainerSchema.RC_ShippingMode.Name].ToString(), sourceRow[RefContainerSchema.RC_Contour.Name].ToString(), sourceRow[RefContainerSchema.RC_IATARateClass.Name].ToString()))
					{
						shouldInsert = false;
					}
					break;

				case RefServiceLevelSchema.Constants.TableName:
					//NK = RS_Code
					if (SameNkExists(RefServiceLevelSchema.RS_Code, sourceRow[RefServiceLevelSchema.RS_Code.Name].ToString()))
					{
						shouldInsert = false;
					}
					break;

				case RefPacksSchema.Constants.TableName:
					// NK = CommercialPack + CustomsPack + CustomsCountry + Supplier
					// FK = RP_OH_Supplier (OrgHeader)
					if (SameCompositeRefPacksNkExists(sourceRow[RefPacksSchema.Constants.RP_CommercialPack].ToString(), sourceRow[RefPacksSchema.Constants.RP_CustomsPack].ToString(), sourceRow[RefPacksSchema.Constants.RP_CustomsCountry].ToString(), sourceRow[RefPacksSchema.Constants.RP_OH_Supplier]))
					{
						shouldInsert = false;
					}
					else if (!FkIsNullOrReferencedPkExists(OrgHeaderSchema.PK, sourceRow[RefPacksSchema.RP_OH_Supplier.Name]))
					{
						sourceRow[RefPacksSchema.RP_OH_Supplier.Name] = DBNull.Value;
					}
					break;
				case RefContainerCodeMapSchema.Constants.TableName:
					var containerPK = sourceRow[RefContainerCodeMapSchema.Constants.RCM_RC_Container].ToString();
					var countryCode = sourceRow[RefContainerCodeMapSchema.Constants.RCM_RN_NKCountry].ToString();
					var usage = sourceRow[RefContainerCodeMapSchema.Constants.RCM_Usage].ToString();
					shouldInsert = RefContainerExists(containerPK) && !SameCompositeRefContainerCodeMapExists(containerPK, countryCode, usage);
					break;
			}

			if (shouldInsert)
			{
				base.DoInsert(sourceRow, targetTable, ref targetIndex);
			}
		}

		bool SameCompositeRefContainerCodeMapExists(string containerPK, string countryCode, string usage)
		{
			return IsRecordInDatabase(string.Format(CultureInfo.InvariantCulture, "SELECT 1 FROM dbo.RefContainerCodeMap WHERE [RCM_RC_Container] = '{0}' and [RCM_RN_NKCountry] = '{1}' and [RCM_Usage] = '{2}'", containerPK, countryCode, usage));
		}

		bool RefContainerExists(string containerPK)
		{
			return IsRecordInDatabase(string.Format(CultureInfo.InvariantCulture, "SELECT 1 FROM dbo.RefContainer WHERE [RC_PK] = '{0}'", containerPK));
		}

		bool SameCompositeRefPacksNkExists(string commercialPack, string customsPack, string customsCountry, object supplierFk)
		{
			string sqlText = String.Format("SELECT TOP 1 RP_PK FROM dbo.RefPacks WHERE RP_CommercialPack = '{0}' AND RP_CustomsPack = '{1}' AND RP_CustomsCountry = '{2}' AND {3}",
				commercialPack, customsPack, customsCountry, GetGuidWhere("RP_OH_Supplier", supplierFk));
			return IsRecordInDatabase(sqlText);
		}

		bool SameCompositeRefContainerNkExists(string code, string shippingMode, string contour, string iataRateClass)
		{
			string sqlText = String.Format("SELECT TOP 1 RC_PK FROM dbo.RefContainer WHERE RC_Code = '{0}' AND RC_ShippingMode = '{1}' AND RC_Contour = '{2}' AND RC_IATARateClass = '{3}'",
				code, shippingMode, contour, iataRateClass);
			return IsRecordInDatabase(sqlText);
		}

		string GetGuidWhere(string guidColumn, object value)
		{
			return (value != DBNull.Value) ? string.Format("{0} = '{1}'", guidColumn, value.ToString()) : string.Format("{0} is null", guidColumn);
		}
	}
}
