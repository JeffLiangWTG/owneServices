using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Microsoft.SqlServer.Types;

namespace Enterprise.DataTransfer.Integration
{
	public interface IStringToBusinessObjectFieldConverter
	{
		ZGuid MappingOrgPK { get; }
		bool IsFKGuidColumn(string columnName);

		BusinessObject LoadBOAndExpectOnly1(BusinessObjectFactory factory, Type bizType, ZQuery filter, string nKStringForErrorMessage, INotifications notifications, bool notifyErrorIfNoneFound);
		ZGuid GetPKFromNK(BusinessObjectFactory factory, Type bizType, SchemaColumn naturalKey, ZString nk, INotifications notifications);
		ZGuid GetPKFromNKGivenFKType(BusinessObjectFactory factory, string naturalKeyValue, ForeignKeyType foreignKeyType, INotifications notifications);

		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, ZDecimal value, int precision, int scale, INotifications notifications, string errorContext);
		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, DateTime value);
		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, DateTimeOffset value);
		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, TimeSpan value);

		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, SqlGeography value);
		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, string valueAsString, INotifications notifications);
		void SetPropertyInfoValue(ZPropertyInfo property, string valueAsString, INotifications notifications, string errorContext);
		void SetPropertyInfoValue(ZPropertyInfo property, string valueAsString, ForeignKeyType fKType, INotifications notifications);
		void SetPropertyInfoValue(ZPropertyInfo property, string valueAsString, ForeignKeyType fKType, INotifications notifications, string errorContext);
		IZType ConvertRawStringToZTypeValue(Type propertyType, ForeignKeyType fKType, BusinessObjectFactory factory, string valueAsString, INotifications notifications);
	}
}
