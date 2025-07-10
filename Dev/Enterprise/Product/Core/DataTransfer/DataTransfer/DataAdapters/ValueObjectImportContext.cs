using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Microsoft.SqlServer.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class ValueObjectImportContext : IValueObjectImportContext
	{
		public ValueObjectImportContext(BusinessObjectFactory factory, INotifications notifications)
			: this(new BusinessObjectFactoryProvider(factory), notifications)
		{
		}

		public ValueObjectImportContext(BusinessObjectFactoryProvider factoryProvider, INotifications notifications)
			: this(factoryProvider, new Xsd.XmlInterchange(), notifications)
		{
		}

		public ValueObjectImportContext(BusinessObjectFactory factory, Xsd.XmlInterchange interchange, INotifications notifications)
			: this(new BusinessObjectFactoryProvider(factory), interchange, notifications)
		{
		}

		public ValueObjectImportContext(BusinessObjectFactoryProvider factoryProvider, Xsd.XmlInterchange interchange, INotifications notifications)
			: this(factoryProvider, interchange, new OrganisationMatching(factoryProvider, interchange, notifications), notifications)
		{
		}

		public ValueObjectImportContext(BusinessObjectFactory factory, Xsd.XmlInterchange interchange, IOrganisationMatching organisationMatching, INotifications notifications)
			: this(new BusinessObjectFactoryProvider(factory), interchange, organisationMatching, notifications)
		{
		}

		public ValueObjectImportContext(BusinessObjectFactoryProvider factoryProvider, Xsd.XmlInterchange interchange, IOrganisationMatching organisationMatching, INotifications notifications)
		{
			if (factoryProvider == null)
			{
				throw new ArgumentNullException(nameof(factoryProvider));
			}

			if (interchange == null)
			{
				throw new ArgumentNullException(nameof(interchange));
			}

			if (organisationMatching == null)
			{
				throw new ArgumentNullException(nameof(organisationMatching));
			}

			if (notifications == null)
			{
				throw new ArgumentNullException(nameof(notifications));
			}

			this.factoryProvider = factoryProvider;
			this.interchange = interchange;
			this.organisationMatching = organisationMatching;
			this.Notifications = notifications;
			this.EDIInterchange = null;
		}

		#region Properties

		public BusinessObjectFactoryProvider FactoryProvider
		{
			get { return factoryProvider; }
		}
		readonly BusinessObjectFactoryProvider factoryProvider;

		public IValueObject Interchange
		{
			get { return interchange; }
		}
		readonly IValueObject interchange;

		Xsd.XmlInterchange XmlInterchange
		{
			get { return Interchange as Xsd.XmlInterchange; }
		}

		public IOrganisationMatching OrganisationMatching
		{
			get { return organisationMatching; }
		}
		readonly IOrganisationMatching organisationMatching;

		public EDIInterchange EDIInterchange
		{
			get;
			set;
		}

		public Stream CurrentObjectXMLUTF8
		{
			get;
			set;
		}

		public BusinessObjectFactory Factory
		{
			get { return FactoryProvider.Current; }
		}

		public bool NotificationsHasErrors
		{
			get
			{
				var buffer = Notifications as NotificationBuffer;
				return buffer != null && buffer.HasErrors;
			}
		}

		#endregion

		#region String to Field Converter

		public IStringToBusinessObjectFieldConverter Converter
		{
			get
			{
				if (fConverter == null)
				{
					fConverter = NewConverter();
				}
				return fConverter;
			}
		}
		StringToBusinessObjectFieldConverter fConverter;

		StringToBusinessObjectFieldConverter NewConverter()
		{
			ZGuid mappingOrgPK = GetMappingOrgPKFromInterchange();

			return mappingOrgPK.IsValid ? StringToBusinessObjectFieldConverter.GetInstance(mappingOrgPK) : StringToBusinessObjectFieldConverter.InstanceForCurrentCompany;
		}

		ZGuid GetMappingOrgPKFromInterchange()
		{
			var result = ZGuid.Empty;
			if (XmlInterchange.InterchangeInfo.IsSpecified)
			{
				var separateImportContext = new ValueObjectImportContext(Factory, XmlInterchange, OrganisationMatching, new NotificationBuffer());
				result = separateImportContext.FindOrganisationPK(XmlInterchange.InterchangeInfo.EDIOrganisation, null, OrganisationTypes.None);
			}
			return result;
		}

		public void SetPropertyInfoValueIfValueNotEmpty(ZPropertyInfo propertyInfo, string valueAsString)
		{
			SetPropertyInfoValueIfValueNotEmpty(propertyInfo, valueAsString, ForeignKeyType.None);
		}

		public void SetPropertyInfoValueIfValueNotEmpty(ZPropertyInfo propertyInfo, string valueAsString, ForeignKeyType fkType)
		{
			SetPropertyInfoValueIfValueNotEmpty(propertyInfo, valueAsString, fkType, "");
		}

		public void SetPropertyInfoValueIfValueNotEmpty(ZPropertyInfo propertyInfo, string valueAsString, ForeignKeyType fkType, string errorContext)
		{
			if (!string.IsNullOrEmpty(valueAsString))
			{
				SetPropertyInfoValue(propertyInfo, valueAsString, fkType, errorContext);
			}
		}

		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, string valueAsString)
		{
			SetPropertyInfoValue(propertyInfo, valueAsString, true, "");
		}

		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, string valueAsString, string errorContext)
		{
			SetPropertyInfoValue(propertyInfo, valueAsString, ForeignKeyType.None, errorContext);
		}

		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, string valueAsString, bool isElementSpecified)
		{
			SetPropertyInfoValue(propertyInfo, valueAsString, isElementSpecified, "");
		}

		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, string valueAsString, bool isElementSpecified, string errorContext)
		{
			SetPropertyInfoValue(propertyInfo, valueAsString, ForeignKeyType.None, isElementSpecified, errorContext);
		}

		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, ZDecimal value, CargoWise.Schema.SchemaDecimalColumn column)
		{
			SetPropertyInfoValue(propertyInfo, value, column.Precision, column.Scale, "");
		}

		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, ZDecimal value, CargoWise.Schema.SchemaDecimalColumn column, string errorContext)
		{
			SetPropertyInfoValue(propertyInfo, value, column.Precision, column.Scale, errorContext);
		}

		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, ZDecimal value, int precision, int scale)
		{
			SetPropertyInfoValue(propertyInfo, value, precision, scale, "");
		}

		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, ZDecimal value, int precision, int scale, string errorContext)
		{
			Converter.SetPropertyInfoValue(propertyInfo, value, precision, scale, this, errorContext);
		}

		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, string valueAsString, ForeignKeyType fkType)
		{
			SetPropertyInfoValue(propertyInfo, valueAsString, fkType, "");
		}

		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, string valueAsString, ForeignKeyType fkType, string errorContext)
		{
			Converter.SetPropertyInfoValue(propertyInfo, valueAsString, fkType, this, errorContext);
		}

		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, string valueAsString, ForeignKeyType fkType, bool isElementSpecified)
		{
			SetPropertyInfoValue(propertyInfo, valueAsString, fkType, isElementSpecified, "");
		}

		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, string valueAsString, ForeignKeyType fkType, bool isElementSpecified, string errorContext)
		{
			if (isElementSpecified || !SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
			{
				SetPropertyInfoValue(propertyInfo, valueAsString, fkType, errorContext);
			}
		}

		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, DateTime value)
		{
			Converter.SetPropertyInfoValue(propertyInfo, value);
		}

		public void SetPropertyInfoValueIfValueNotEmpty(ZPropertyInfo propertyInfo, ZDateTime value)
		{
			if (!value.IsEmpty)
			{
				Converter.SetPropertyInfoValue(propertyInfo, value.ToDateTime());
			}
		}

		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, DateTimeOffset value)
		{
			Converter.SetPropertyInfoValue(propertyInfo, value);
		}

		public void SetPropertyInfoValueIfValueNotEmpty(ZPropertyInfo propertyInfo, ZDateTimeOffset value)
		{
			if (!value.IsEmpty)
			{
				Converter.SetPropertyInfoValue(propertyInfo, value.ToDateTimeOffset());
			}
		}

		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, TimeSpan value)
		{
			Converter.SetPropertyInfoValue(propertyInfo, value);
		}

		public void SetPropertyInfoValueIfValueNotEmpty(ZPropertyInfo propertyInfo, ZTime value)
		{
			if (!value.IsEmpty)
			{
				Converter.SetPropertyInfoValue(propertyInfo, value.ToDateTime());
			}
		}

		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, SqlGeography value)
		{
			Converter.SetPropertyInfoValue(propertyInfo, value);
		}

		public void SetPropertyInfoValueIfValueNotEmpty(ZPropertyInfo propertyInfo, ZGeography value)
		{
			if (!value.IsEmpty)
			{
				Converter.SetPropertyInfoValue(propertyInfo, (SqlGeography)value);
			}
		}

		public void SetPropertyInfoValueIfValueNotEmpty(ZPropertyInfo propertyInfo, ZDate value)
		{
			if (!value.IsEmpty)
			{
				if (propertyInfo.PropertyType == typeof(ZDateTime))
				{
					Converter.SetPropertyInfoValue(propertyInfo, value.ToDateTime());
				}
				else
				{
					propertyInfo.Value = value;
				}
			}
		}

		public void SetPropertyInfoValueWithinMaxLengthAndWarn(ZPropertyInfo propertyInfo, string value, bool isElementSpecified = true)
		{
			var valueToSet = propertyInfo.MaxLength > 0 && value.Length > propertyInfo.MaxLength ? value.Substring(0, propertyInfo.MaxLength) : value;

			if (isElementSpecified && propertyInfo.MaxLength > 0 && value.Length > propertyInfo.MaxLength)
			{
				Notifications.AddWarning(Res.GetString("a07695eb-af8f-487a-85b4-de83358d25d4", "'{0}' exceeds the max length that is allowed for the field, {1}. System has set '{2}' to the field instead.", value, propertyInfo.HumanReadableName, valueToSet));
			}

			SetPropertyInfoValue(propertyInfo, valueToSet, isElementSpecified);
		}

		public bool SetPropertyInfoValueAndAddErrorIfNotWithinMaxLength(ZPropertyInfo propertyInfo, string value, bool isElementSpecified = true)
		{
			if (isElementSpecified && propertyInfo.MaxLength > 0 && value.Length > propertyInfo.MaxLength)
			{
				Notifications.AddError(Res.GetString("061D91AE-8BCF-475A-8391-D8A3E50FE9F4", "'{0}' exceeds the max length that is allowed for the field ({1}) and {2} will not be imported", value, propertyInfo.MaxLength.ToString() ,propertyInfo.HumanReadableName));
				return true;
			}

			SetPropertyInfoValue(propertyInfo, value, isElementSpecified);
			return false;
		}

		#endregion

		#region INotifications Members

		public readonly INotifications Notifications;

		void INotifications.Add(INotification notification)
		{
			Notifications.Notify(notification);
		}

		public void QueryUser(IQueryUserEventArgs e)
		{
			Notifications.QueryUser(e);
		}

		public ZString LastNotificationMessage
		{
			get
			{
				var result = ZString.Empty;
				var buffer = Notifications as NotificationBuffer;
				if (buffer != null && buffer.Events.Length > 0)
				{
					result = buffer.Events[buffer.Events.Length - 1].Message;
				}
				return result;
			}
		}

		#endregion

		#region Organisation Matching

		public ZGuid FindOrganisationPK(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType)
		{
			return OrganisationMatching.FindOrganisationPK(value, sourceObject, orgType);
		}

		public OrgHeader FindOrganisation(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType)
		{
			return OrganisationMatching.FindOrganisation(value, sourceObject, orgType);
		}

		public ZGuid FindOrCreateTempOrganisationPK(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType, UnmatchOrgRecordCriteria unmatchOrgRecordCriteria)
		{
			return OrganisationMatching.FindOrCreateTempOrganisationPK(value, sourceObject, orgType, unmatchOrgRecordCriteria);
		}

		public ZGuid FindOrCreateTempOrganisationPK(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType)
		{
			return OrganisationMatching.FindOrCreateTempOrganisationPK(value, sourceObject, orgType);
		}

		public IOrgHeaderForMatching FindOrCreateTempOrganisation(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType)
		{
			return OrganisationMatching.FindOrCreateTempOrganisation(value, sourceObject, orgType);
		}

		public IOrgHeaderForMatching FindOrCreateTempOrganisation(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType, UnmatchOrgRecordCriteria unmatchOrgRecordCriteria)
		{
			return OrganisationMatching.FindOrCreateTempOrganisation(value, sourceObject, orgType, unmatchOrgRecordCriteria);
		}

		#endregion

		public bool NotificationsContainsNotifictionType(INotificationType type)
		{
			var buffer = Notifications as NotificationBuffer;
			return buffer != null && buffer.ContainsNotificationType(type);
		}

		public GlbStaff GetStaffByCodeThrowingErrorIfNotFound(BusinessObjectFactory factory, ZString staffCode)
		{
			var staff = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffCode);

			if (staff == null)
			{
				throw new Exception(String.Format("Could not find Staff code = [{0}]. Please create it and retry.", staffCode));
			}
			else
			{
				return staff;
			}
		}

		public RefCountry GetCountryByCodeThrowingErrorIfNotFound(BusinessObjectFactory factory, ZString countryCode)
		{
			var country = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);

			if (country == null)
			{
				throw new Exception(String.Format("Could not find Country code = [{0}]. Please create it and retry.", countryCode));
			}
			else
			{
				return country;
			}
		}

		public T ConvertRawStringToZTypeValue<T>(string value, ForeignKeyType fkType) where T : IZType
		{
			return (T)Converter.ConvertRawStringToZTypeValue(typeof(T), fkType, Factory, value, Notifications);
		}

		BusinessObject IValueObjectImportContext.ImportingJob { get; set; }
	}
}
