using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using Enterprise.DbUpgrader.Data.BaseData.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data.BaseData.System
{
	public class SystemUpgradeTask : SystemInstallDataUpgradeTask
	{
		public SystemUpgradeTask()
			: base(new SystemDataFile())
		{
			IsRunningForSetup = true;
		}

		readonly Dictionary<string, string> faxDeviceConfigRecords = new Dictionary<string, string>();

		protected override void DoInsert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			bool shouldInsert = true;

			switch (targetTable.TableName)
			{
				case FaxDeviceConfigSchema.Constants.TableName:
					faxDeviceConfigRecords.Add(sourceRow[FaxDeviceConfigSchema.PK.Name].ToString(), sourceRow[FaxDeviceConfigSchema.FX_Name.Name].ToString());
					if (SameNkExists(FaxDeviceConfigSchema.FX_Name, sourceRow[FaxDeviceConfigSchema.FX_Name.Name].ToString()))
					{
						shouldInsert = false;
					}
					break;
				case FaxRouteSchema.Constants.TableName:
					if (SameNkExists(FaxRouteSchema.FR_RouteName, sourceRow[FaxRouteSchema.FR_RouteName.Name].ToString()) || SameNkExists(FaxRouteSchema.FR_Prefix, sourceRow[FaxRouteSchema.FR_Prefix.Name].ToString()))
					{
						shouldInsert = false;
					}
					else if (!FkIsNullOrReferencedPkExists(FaxDeviceConfigSchema.PK, sourceRow[FaxRouteSchema.FR_FX.Name]) && SameNkExists(FaxDeviceConfigSchema.FX_Name, faxDeviceConfigRecords[sourceRow[FaxRouteSchema.FR_FX.Name].ToString()]))
					{
						sourceRow[FaxRouteSchema.FR_FX.Name] = GetFaxDevicePK(faxDeviceConfigRecords[sourceRow[FaxRouteSchema.FR_FX.Name].ToString()]);
					}
					break;
			}

			if (shouldInsert)
			{
				base.DoInsert(sourceRow, targetTable, ref targetIndex);
			}
		}

		object GetFaxDevicePK(string faxDeviceConfigName)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture, "SELECT TOP 1 {0} FROM {1} WHERE {2} = '{3}'", FaxDeviceConfigSchema.PK.Name, FaxDeviceConfigSchema.Constants.TableName, FaxDeviceConfigSchema.FX_Name.Name, faxDeviceConfigName);

			return Db.Connection.ExecuteScalar(sqlText);
		}
	}
}
