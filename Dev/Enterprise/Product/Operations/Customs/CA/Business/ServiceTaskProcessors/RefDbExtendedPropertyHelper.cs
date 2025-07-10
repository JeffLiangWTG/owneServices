using System;
using System.Globalization;
using CargoWise.Data;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	class RefDbExtendedPropertyHelper : IDisposable
	{
		public RefDbExtendedPropertyHelper()
		{
			RefdbLocator = Db.Connection;
			RefDbName = RefdbLocator.GetReferenceDatabaseName(RefDbTypeEnum.Enterprise, Core.Constants.CountryCodes.Canada);
		}

		internal IPhysicalRefDbLocation RefdbLocator { get; private set; }
		protected virtual ZString RefDbName { get; private set; }

		internal DbConnection Connection
		{
			get
			{
				if (connection == null)
				{
					ZString refDbName = RefDbName;
					if (!refDbName.IsEmpty)
					{
						connection = Db.NewAdminConnection(((DbConnection)RefdbLocator).ServerName, refDbName);
					}
				}
				return connection;
			}
		}
		DbConnection connection;

		public void Dispose()
		{
			if (connection != null)
			{
				connection.Dispose();
			}
		}

		#region LastUpdateDate

		internal const string LastUpdateDatePropertyName = "LastUpdateDate";

		public void SetLastTariffDataUpdateDate(ZDateTime lastUpdateDate)
		{
			var connection = Connection;
			var newPropertyValue = lastUpdateDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			if (connection != null)
			{
				var oldLastUpdateDate = GetLastTariffDataUpdateDate(connection);
				if (oldLastUpdateDate.IsEmpty)
				{
					DataUtils.AddDbExtendedProperty(connection, LastUpdateDatePropertyName, newPropertyValue);
				}
				else if (!oldLastUpdateDate.IsValid || lastUpdateDate > oldLastUpdateDate)
				{
					DataUtils.SaveDbExtendedProperty(connection, LastUpdateDatePropertyName, newPropertyValue);
				}
			}
		}

		public ZDateTime GetLastTariffDataUpdateDate()
		{
			return GetLastTariffDataUpdateDate(Connection);
		}

		ZDateTime GetLastTariffDataUpdateDate(DbConnection connection)
		{
			var result = ZDateTime.Empty;
			if (connection != null)
			{
				var propertyValue = DataUtils.LoadDbExtendedProperty(connection, LastUpdateDatePropertyName);
				if (propertyValue != null && !ZDateTime.TryParseExact(propertyValue, out result, "yyyyMMdd"))
				{
					result = ZDateTime.Invalid;
				}
			}
			return result;
		}

		#endregion

		#region RefreshRequired

		internal const string RefreshRequiredPropertyName = "RefreshRequired";

		public void SetRefreshRequired(bool newValue)
		{
			var connection = Connection;
			var newPropertyValue = newValue ? "Y" : "N";
			if (connection != null)
			{
				var oldValue = GetRefreshRequired(connection);
				if (!oldValue.HasValue)
				{
					DataUtils.AddDbExtendedProperty(connection, RefreshRequiredPropertyName, newPropertyValue);
				}
				else if (newValue != oldValue.Value)
				{
					DataUtils.SaveDbExtendedProperty(connection, RefreshRequiredPropertyName, newPropertyValue);
				}
			}
		}

		public bool? GetRefreshRequired()
		{
			return GetRefreshRequired(Connection);
		}

		bool? GetRefreshRequired(DbConnection connection)
		{
			bool? result = null;
			if (connection != null)
			{
				var propertyValue = DataUtils.LoadDbExtendedProperty(connection, RefreshRequiredPropertyName);
				if (propertyValue != null)
				{
					result = propertyValue == "Y";
				}
			}
			return result;
		}

		#endregion
	}
}
