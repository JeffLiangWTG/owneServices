using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Microsoft.SqlServer.Types;

namespace Enterprise.DataTransfer.Integration
{
	public enum ForeignKeyType
	{
		None,
		Contact,
		WebUrl,
		OrganisationMatchWithFullName,
		OrganisationMatchWithCode,
		RefServiceLevelNK,
		CurrencyNK,
		OrgAddressNK,
		PortNK,
		CountryNK,
		VesselNameNK,
		ContainerCodeNK,
		ChargeCodeNK,
		PackTypeCodeNK,
		IncoTermNK,
		EventCodeNK,
		WarehouseNK,
		IntZoneNK
	}

	public interface IValueObjectImportContext : INotifications, INotificationSubscriberQueryUser
	{
		BusinessObject ImportingJob { get; set; }
		BusinessObjectFactoryProvider FactoryProvider { get; }
		IValueObject Interchange { get; }
		IOrganisationMatching OrganisationMatching { get; }
		EDIInterchange EDIInterchange { get; set; }
		Stream CurrentObjectXMLUTF8 { get; set; }
		ZString LastNotificationMessage { get; }
		BusinessObjectFactory Factory { get; }
		IStringToBusinessObjectFieldConverter Converter { get; }

		bool NotificationsHasErrors { get; }
		bool NotificationsContainsNotifictionType(INotificationType type);

		void SetPropertyInfoValueIfValueNotEmpty(ZPropertyInfo propertyInfo, string valueAsString);
		void SetPropertyInfoValueIfValueNotEmpty(ZPropertyInfo propertyInfo, string valueAsString, ForeignKeyType fkType);
		void SetPropertyInfoValueIfValueNotEmpty(ZPropertyInfo propertyInfo, string valueAsString, ForeignKeyType fkType, string errorContext);
		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, string valueAsString);
		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, string valueAsString, string errorContext);
		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, string valueAsString, bool isElementSpecified);
		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, string valueAsString, bool isElementSpecified, string errorContext);
		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, ZDecimal value, CargoWise.Schema.SchemaDecimalColumn column);
		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, ZDecimal value, CargoWise.Schema.SchemaDecimalColumn column, string errorContext);
		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, ZDecimal value, int precision, int scale);
		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, ZDecimal value, int precision, int scale, string errorContext);
		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, string valueAsString, ForeignKeyType fkType);
		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, string valueAsString, ForeignKeyType fkType, string errorContext);
		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, string valueAsString, ForeignKeyType fkType, bool isElementSpecified);
		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, string valueAsString, ForeignKeyType fkType, bool isElementSpecified, string errorContext);
		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, DateTime value);
		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, TimeSpan value);
		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, DateTimeOffset value);
		void SetPropertyInfoValue(ZPropertyInfo propertyInfo, SqlGeography value);
		void SetPropertyInfoValueIfValueNotEmpty(ZPropertyInfo propertyInfo, ZDateTime value);
		void SetPropertyInfoValueIfValueNotEmpty(ZPropertyInfo propertyInfo, ZDate value);
		void SetPropertyInfoValueIfValueNotEmpty(ZPropertyInfo propertyInfo, ZTime value);
		void SetPropertyInfoValueIfValueNotEmpty(ZPropertyInfo propertyInfo, ZDateTimeOffset value);
		void SetPropertyInfoValueIfValueNotEmpty(ZPropertyInfo propertyInfo, ZGeography value);
		void SetPropertyInfoValueWithinMaxLengthAndWarn(ZPropertyInfo propertyInfo, string value, bool isElementSpecified);

		ZGuid FindOrganisationPK(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType);
		OrgHeader FindOrganisation(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType);
		ZGuid FindOrCreateTempOrganisationPK(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType);
		ZGuid FindOrCreateTempOrganisationPK(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType, UnmatchOrgRecordCriteria unmatchOrgRecordCriteria);
		IOrgHeaderForMatching FindOrCreateTempOrganisation(IValueObject value, BusinessObject sourceObject, OrganisationTypes prgType);
		IOrgHeaderForMatching FindOrCreateTempOrganisation(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType, UnmatchOrgRecordCriteria unmatchOrgRecordCriteria);
		GlbStaff GetStaffByCodeThrowingErrorIfNotFound(BusinessObjectFactory factory, ZString staffCode);
		RefCountry GetCountryByCodeThrowingErrorIfNotFound(BusinessObjectFactory factory, ZString countryCode);
		T ConvertRawStringToZTypeValue<T>(string valueAsString, ForeignKeyType fkType) where T : IZType;
	}
}
