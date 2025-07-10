using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DeviceManagement.Business
{
	class ClientDeviceHeaderSubscriberHelper
	{
		public static LicenceDatabase GetLicenseFromCodes(BusinessObjectFactory factory, string serverCode, string enterpriseCode)
		{
			if (string.IsNullOrEmpty(serverCode) || string.IsNullOrEmpty(enterpriseCode))
			{
				return null;
			}

			var licenceQuery = new ZDBOnlyQuery(typeof(LicenceDatabase));
			licenceQuery.AddToFilter(LicenceDatabaseSchema.LD_ServerCode, serverCode);

			var enterpriseSubQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceDatabaseSchema.LD_LE);
			enterpriseSubQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCode);

			licenceQuery.AddSubQuery(enterpriseSubQuery, JoinCondition.And);

			var licence = factory.LoadTop1<LicenceDatabase>(licenceQuery);

			return licence;
		}

		public static bool HasChange(DataRow dataRow, SchemaColumn schemaColumn)
		{
			return dataRow.RowState == DataRowState.Added ||
				(dataRow.RowState == DataRowState.Modified && HasChange(dataRow, schemaColumn.Name));
		}

		public static bool HasChange(DataRow dataRow, string columnName)
		{
			return dataRow[columnName] != dataRow[columnName, DataRowVersion.Original];
		}
	}
}
