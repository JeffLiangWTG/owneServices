using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.ResourceStrings.Cache;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace CargoWise.EntityFramework
{
	public interface IZPropertyInfoObsolete
	{
		bool ReadOnly { get; set; }
	}

	public interface IZPropertyInfo : INotifications
	{
		IZType Value { get; set; }
		void AddMessageError(string message);
		void AddWarning(string message);
	}

	public interface IZPropertyInfoInternals
	{
		void SetHumanReadableNameFromGuiCaption(ZString guiCaption);
	}

	public class ZPropertyInfo<T> : ZPropertyInfo where T : IZType
	{
		internal ZPropertyInfo(BusinessObject bizObj, string name, PropertyDescriptor propertyDescriptor)
			: base(bizObj, name, propertyDescriptor)
		{
		}

		public new T Value
		{
			get { return (T)base.Value; }
			set { base.Value = value; }
		}

		public new T DefaultValue
		{
			get { return (T)base.DefaultValue; }
		}
	}

	public class ZPropertyInfo : IZPropertyInfo, IZPropertyInfoObsolete, INotificationProvider, IZPropertyInfoInternals
	{
		internal ZPropertyInfo(BusinessObject bizObj, string name, PropertyDescriptor propertyDescriptor)
		{
			if (Object.ReferenceEquals(bizObj, null))
			{
				throw new ArgumentNullException(nameof(bizObj), "Cannot pass null to ZPropertyInfo(BusinessObject, .. ).");
			}
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name), "Cannot pass null to ZPropertyInfo(.., name, .. ).");
			}

			this.BizObj = bizObj;
			this.Name = name;
			this.propertyDescriptor = propertyDescriptor;
		}

		#region Operator Overloads

		public static bool operator !=(ZPropertyInfo a, object b)
		{
			return !(a == b);
		}

		public static bool operator ==(ZPropertyInfo a, object b)
		{
			return Object.Equals(a, b);
		}

		#endregion

		#region Equals / GetHashCode

		public override int GetHashCode()
		{
			return BizObj.GetHashCode() ^ Name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			ZPropertyInfo otherObj = obj as ZPropertyInfo;
			if (otherObj != null)
			{
				return otherObj.BizObj == BizObj && otherObj.Name == Name;
			}
			else
			{
				return false;
			}
		}

		#endregion

		#region PushValueIntoRow

		public void PushValueIntoRow()
		{
			BizObj.SetPropertyValue(this, Value, false);
		}

		#endregion

		#region Concurrency Handling Policy

		/// <summary>
		/// Gets and sets the concurrency policy for this property.
		/// Only available on persistent properties.
		/// </summary>
		public ConcurrencyPolicy ConcurrencyPolicy
		{
			get
			{
				if (!IsPersistent)
				{
					throw new MethodAccessException("Can't get ConcurrencyPolicy for transient property");
				}

				return this.GetConcurrencyPolicy(BizObj.Row);
			}
		}

		#endregion

		#region HasSetter

		public bool HasSetter
		{
			get { return PropertyDescriptor.HasSetter(); }
		}

		#endregion

		#region HasUserDescription

		public bool HasUserDescription
		{
			get { return HasUserDescriptionCore; }
		}

		protected virtual bool HasUserDescriptionCore
		{
			get { return Description != BizObj.TableName + "." + Name; }
		}

		#endregion

		#region Description

		public string Description
		{
			get { return DescriptionCore; }
		}

		protected virtual string DescriptionCore
		{
			get
			{
				string result;
				ResourceStringData resourceString = null;
				var businessObject = new DataBoundBusinessObject(BizObj);
				if (BizObj is ISupportMultipleResourceStringData multipleResourceStringDataSupporter)
				{
					try
					{
						resourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(PropertyDescriptor, businessObject, multipleResourceStringDataSupporter.MultipleKeysToUse);
					}
					catch (DeletedRowInaccessibleException)
					{
						resourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(PropertyDescriptor, null, Array.Empty<String>());
					}
				}
				else
				{
					resourceString = DataBoundResourceStrings.GetDataForProperty(PropertyDescriptor, businessObject);
				}

				if (resourceString != null && !string.IsNullOrEmpty(resourceString.Caption))
				{
					result = resourceString.Caption;
				}
				else
				{
					IDescription desc = MetaData.GetDescription(BizObj, PropertyDescriptor);
					if (desc != null)
					{
						result = desc.GetDescription(0, System.Globalization.CultureInfo.CurrentCulture);
					}
					else
					{
						result = BizObj.TableName + "." + Name;
					}
				}
				return result;
			}
		}

		#endregion

		#region HumanReadableName

		public ZString HumanReadableName
		{
			get { return GetHumanReadableNameCore(); }
			set { SetHumanReadableNameCore(value); }
		}

		ZString? uiCaption;

		protected virtual ZString GetHumanReadableNameCore()
		{
			IHumanReadableNameProvider service;
			ZString result = ZString.Empty;
			if (!uiCaption.HasValue && BizObj != null && BizObj.Factory != null)
			{
				service = BizObj.Factory.ServiceContainer.GetService<IHumanReadableNameProvider>();
				if (service != null)
				{
					uiCaption = service.GetHumanReadableName(this);
				}
			}
			if (uiCaption.HasValue)
			{
				result = uiCaption.Value;
			}
			if (result.IsEmpty)
			{
				result = PropertyInfoStorage.GetHumanReadableName(this).Value;
			}
			if (result.IsEmpty)
			{
				result = DefaultHumanReadableName;
			}
			return result;
		}

		protected virtual void SetHumanReadableNameCore(ZString value, bool guiCaption = false)
		{
			PropertyInfoStorage.SetHumanReadableName(this, value, guiCaption);
		}

		public bool HasHumanReadableName
		{
			get { return HasUserDescription || HumanReadableNameRepresentingGUICaption || HumanReadableName != GetFriendlyColumnName(Name); }
		}

		bool HumanReadableNameRepresentingGUICaption
		{
			get
			{
				var result = PropertyInfoStorage.GetHumanReadableName(this);
				if (result.IsGuiCaption)
				{
					uiCaption = result.Value;
				}
				return !result.Value.IsEmpty && result.IsGuiCaption;
			}
		}

		protected virtual ZString DefaultHumanReadableName
		{
			get { return HasUserDescription ? Description : GetFriendlyColumnName(Name); }
		}

		protected string GetFriendlyColumnName(string columnName)
		{
			return GetFriendlyColumnNameShared(columnName);
		}

		public static string GetFriendlyColumnNameShared(string columnName)
		{
			string result;
			var indexOfLastUnderscore = columnName.LastIndexOf('_');

			if (indexOfLastUnderscore + 3 == columnName.Length) // don't want to truncate eg. JS_JK
			{
				result = columnName;
			}
			else
			{
				var friendlyName = new StringBuilder(columnName.Substring(indexOfLastUnderscore + 1));

				if (friendlyName[0] == 'N' && friendlyName[1] == 'K') // don't do ToString().StartsWith("NK") as it's expensive :P
				{
					friendlyName.Remove(0, 2);
				}

				for (var i = 2; i < friendlyName.Length; i++)
				{
					var currentChar = friendlyName[i];
					var previousChar = friendlyName[i - 1];

					if ((char.IsUpper(currentChar) && char.IsLower(previousChar)) // eg ParentID -> Parent ID, Section321A -> Section 321A (NOT Section 321 A)
						|| (char.IsDigit(currentChar) && char.IsLower(previousChar))) // eg Inbond7512 -> Inbond 7512
					{
						friendlyName.Insert(i, ' ');
						i += 2;
					}
					else if (char.IsLower(currentChar) && char.IsUpper(previousChar)) // eg MAWBBill -> MAWB Bill, C4Codes -> C4 Codes, A2XY32Code - > A2XY32 Code (NOT A2 XY32 Code)
					{
						friendlyName.Insert(i - 1, ' ');
						i += 2;
					}
				}

				result = friendlyName.ToString();
			}

			return result;
		}

		#endregion HumanReadableName

		#region IsNullable

		public bool IsNullable
		{
			get { return IsNullableCore; }
		}

		protected virtual bool IsNullableCore
		{
			get { return ((IKnowPropertyInformation)BizObj).IsNullable(Name); }
		}

		#endregion

		#region IsPersistent

		/// <summary>
		/// Will this property be stored persistently in the database when Factory.Save() is called?
		/// </summary>
		public bool IsPersistent
		{
			get { return IsPersistentCore; }
		}

		protected virtual bool IsPersistentCore
		{
			get
			{
				return
					BizObj.Row != null &&
					BizObj.Row.Table != null &&
					BizObj.Row.Table.Columns[Name] != null &&
					ObjectFactory.Get<IApplicationSchemaResolver>().SchemaColumnExists(Name, BizObj.TableName);
			}
		}

		#endregion

		#region IsDbColumnSmallDateTime

		public bool IsDbColumnSmallDateTime
		{
			get
			{
				var column = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(Name, BizObj.TableName);
				return column != null && column.SqlDbType == SqlDbType.SmallDateTime;
			}
		}

		#endregion

		#region MaxLength

		public virtual bool SupportsMaxLength
		{
			get { return true; }
		}

		public int MaxLength
		{
			get { return MetaData.GetMaxLength(BizObj, PropertyDescriptor); }
		}

		#endregion

		#region RunAdditionalValidation

		//TODO: Change to internal - should only be called by architecture but old validation requires it
		public void RunAdditionalValidation()
		{
			RunValidationInvoker handler;
			if (PropertyInfoStorage.AdditionalValidationDictionary.TryGetValue(this, out handler))
			{
				handler();
			}
		}

		public virtual event RunValidationInvoker AdditionalValidation
		{
			add
			{
				RunValidationInvoker handler;
				if (PropertyInfoStorage.AdditionalValidationDictionary.TryGetValue(this, out handler))
				{
					handler += value;
				}
				else
				{
					handler = value;
				}
				PropertyInfoStorage.AdditionalValidationDictionary[this] = handler;
			}
			remove
			{
				RunValidationInvoker handler;
				if (PropertyInfoStorage.AdditionalValidationDictionary.TryGetValue(this, out handler))
				{
					handler -= value;
				}
				if (handler == null)
				{
					PropertyInfoStorage.AdditionalValidationDictionary.Remove(this);
				}
				else
				{
					PropertyInfoStorage.AdditionalValidationDictionary[this] = handler;
				}
			}
		}

		#endregion

		#region Name

		public string Name { get; private set; }

		#endregion

		#region ReadOnly

		public bool ReadOnly
		{
			get { return MetaData.GetReadOnly(BizObj, PropertyDescriptor); }
			private set
			{
#if DEBUG
				if (!HasSetter && !value)
				{
					throw new InvalidOperationException("The property '" + Name + "' does not have a setter, thus it doesn't make sense for ZPropertyInfo.ReadOnly to be set to false.");
				}
#endif
				SetMetaDataProperty(MetaDataTypes.ReadOnly, value);
			}
		}

		bool IZPropertyInfoObsolete.ReadOnly
		{
			get { return ReadOnly; }
			set { ReadOnly = value; }
		}

		#endregion

		#region PropertyType

		public Type PropertyType
		{
			get { return PropertyTypeCore; }
		}

		protected virtual Type PropertyTypeCore
		{
			get { return PropertyDescriptor.PropertyType; }
		}

		#endregion

		#region PropertyDescriptor

		public PropertyDescriptor PropertyDescriptor
		{
			get
			{
				if (propertyDescriptor == null)
				{
					propertyDescriptor = BizObj.GetProperties().AllProperties[Name];
					if (propertyDescriptor == null)
					{
						throw new ZException("PropertyDescriptor could not be found for property " + Name);
					}
				}
				return propertyDescriptor;
			}
		}
		PropertyDescriptor propertyDescriptor;

		public static implicit operator PropertyDescriptor(ZPropertyInfo property)
		{
			return property.PropertyDescriptor;
		}

		#endregion

		#region DefaultValue

		public IZType DefaultValue
		{
			get { return DefaultValueCore; }
		}

		protected virtual IZType DefaultValueCore
		{
			get { return null; }
		}

		public object DatabaseDefault
		{
			get { return DatabaseDefaultCore; }
		}

		protected virtual object DatabaseDefaultCore
		{
			get { return ((IKnowPropertyInformation)BizObj).GetDefaultValue(Name); }
		}

		#endregion

		#region Value / PersistentValue

		public IZType Value
		{
			get { return ValueCore; }
			set { ValueCore = value; }
		}

		protected virtual IZType ValueCore
		{
			get
			{
				object bizObjValue = PropertyDescriptor.GetValue(BizObj);
				IZType result;
				if (bizObjValue != null)
				{
					result = bizObjValue as IZType;
					if (result == null)
					{
						if (bizObjValue is SQLComparisonOperator)
						{
							result = ZDataType.ObjectToZType(typeof(ZString), bizObjValue.ToString());
						}
						else
						{
							result = ZDataType.ObjectToZType(PropertyDescriptor.PropertyType, bizObjValue);
						}
					}
				}
				else
				{
					result = DefaultValue;
				}
				return result;
			}
			set { PropertyDescriptor.SetValue(BizObj, value); }
		}

		public IZType PersistentValue
		{
			get
			{
				IZType result;

				if (IsPersistent && !BizObj.IsDeleted)
				{
					result = ZDataType.ObjectToZType(PropertyType, BizObj.Row[Name]);
				}
				else
				{
					result = Value;
				}

				return result;
			}
		}

		#endregion

		#region ValueChanged & ConcurrencyMerged

		public virtual event EventHandler ValueChanged
		{
			add
			{
				AddEventHandler(value, PropertyInfoStorage.ValueChangedDictionary);
			}
			remove
			{
				RemoveEventHandler(value, PropertyInfoStorage.ValueChangedDictionary);
			}
		}

		protected internal void OnValueChanged(EventArgs eventArgs)
		{
			if (PropertyInfoStorage.ValueChangedIsActive
				&& !IsOnValueChangedSuspended)
			{
				if (PropertyInfoStorage.ValueChangedDictionary.TryGetValue(this, out var handler))
				{
					handler(BizObj, eventArgs);
				}
			}
		}

		public IDisposable SuspendOnValueChanged() => BizObj.SuspendOnValueChanged(Name);

		public bool IsOnValueChangedSuspended => BizObj.IsOnValueChangeSuspended(Name);

		public event EventHandler ConcurrencyMerged
		{
			add => AddEventHandler(value, PropertyInfoStorage.ConcurrencyMergedDictionary);
			remove => RemoveEventHandler(value, PropertyInfoStorage.ConcurrencyMergedDictionary);
		}

		protected internal void OnConcurrencyMerged(EventArgs eventArgs)
		{
			EventHandler handler;
			if (PropertyInfoStorage.ConcurrencyMergedDictionary.TryGetValue(this, out handler))
			{
				handler(BizObj, eventArgs);
			}
		}

		void AddEventHandler(EventHandler value, DictionaryKeyedByZPropertyInfo<EventHandler> dictionary)
		{
			EventHandler handler;
			if (dictionary.TryGetValue(this, out handler))
			{
				handler += value;
			}
			else
			{
				handler = value;
			}
			dictionary[this] = handler;
		}

		void RemoveEventHandler(EventHandler value, DictionaryKeyedByZPropertyInfo<EventHandler> dictionary)
		{
			EventHandler handler;
			if (dictionary.TryGetValue(this, out handler))
			{
				handler -= value;
			}
			if (handler == null)
			{
				dictionary.Remove(this);
			}
			else
			{
				dictionary[this] = handler;
			}
		}

		#endregion

		#region OriginalValue / HasChanges

		public IZType OriginalValue
		{
			get { return OriginalValueCore; }
		}

		public bool HasChanges
		{
			get { return HasChangesCore; }
		}

		protected virtual bool HasChangesCore
		{
			get { return BizObj.IsInDatabase && !Value.Equals(OriginalValue); }
		}

		protected virtual IZType OriginalValueCore
		{
			get
			{
				IZType result;
				if (BizObj.Row == null || !BizObj.Table.Columns.Contains(Name))
				{
					if (BizObj is IOriginalValueProvider provider)
					{
						result = provider.GetOriginalValue(this);
					}
					else
					{
						result = Value;
					}
				}
				else
				{
					object rowValue = GetBizOOriginalValue() ?? DBNull.Value;
					result = (IZType)Activator.CreateInstance(PropertyType, new object[] { rowValue });
				}
				return result;
			}
		}

#if DEBUG
		protected virtual
#endif
 object GetBizOOriginalValue()
		{
			return ((IBusinessObjectInternals)BizObj).GetValueFromRowSafely(this, DataRowVersion.Original);
		}

		#endregion

		#region SetValueFromString

		public bool SetValueFromString(ZString stringValue)
		{
			return SetValueFromStringCore(stringValue);
		}

		protected virtual bool SetValueFromStringCore(ZString stringValue)
		{
			ErrorReporter.ReportOnce("SetValueFromStringCore", "Use typed ZPropertyInfo to parse values.");
			return false;
		}

		#endregion

		#region ClearValue

		public void ClearValue()
		{
			Value = DefaultValue;
		}

		#endregion

		#region RefreshBinding

		public void RefreshBinding(IZType oldValue)
		{
			OnValueChanged(new ValueChangedEventArgs(oldValue, this));
			BizObj.OnElementChanged();
		}

		/// <summary>
		/// Updates the property value by forcing the property to call its Get() method.
		/// </summary>
		public void RefreshBinding()
		{
			OnValueChanged(new InfoEventArgs(this));
			BizObj.OnElementChanged();
		}

		public void RefreshBindingForParentRelationFK()
		{
			BizObj.OnElementReset();
		}

		#endregion

		#region Notifications

		public virtual bool IsBusinessObjectValidationSuspended()
		{
			return BizObj.IsValidationSuspended;
		}

		#region Clearing Notifications

		internal virtual void ClearNotification(INotificationType type, string message)
		{
			if (HasNotification(type, message))
			{
				NotificationsStorage.Remove(type, message);
				BizObj.NotifyPropertyNotificationsChanged(this, type, -1);
			}
		}

		public void ClearAllNotifications()
		{
			ClearAllNotificationsCore();
		}

		public void ReplaceNotification(INotification oldNotification, INotificationType newType, string newMessage)
		{
			RemoveNotificationObject(oldNotification);
			AddNotification(newType, newMessage);
		}

		void RemoveNotificationObject(INotification notification)
		{
			NotificationsStorage.Remove(notification);
			BizObj.NotifyPropertyNotificationsChanged(this, notification.Type, -1);
			BizObj.OnNotificationsChanged(true);
		}

		protected virtual void ClearAllNotificationsCore()
		{
			CheckInfoCanBeCleared();
			if (HasNotifications())
			{
				RemoveNotificationObject();
			}
		}

		void RemoveNotificationObject()
		{
			foreach (INotification notification in Notifications)
			{
				BizObj.NotifyPropertyNotificationsChanged(this, notification.Type, -1);
			}
			PropertyInfoStorage.NotificationDictionary.Remove(this);
			BizObj.OnNotificationsChanged(true);
		}

		#endregion

		#region Adding Notifications

		public void AddError(string message)
		{
			(this as INotifications).AddError(message);
		}

		public void AddWarning(string message)
		{
			(this as INotifications).AddWarning(message);
		}

		public void AddMessageError(string message)
		{
			(this as INotifications).AddMessageError(message);
		}

		public void AddMessageErrorWithoutValidationCheck(string message)
		{
			AddNotification(NotificationType.MessageError, message, false);
		}

		public void AddErrorWithoutValidationCheck(string message)
		{
			AddNotification(NotificationType.Error, message, false);
		}

		public void AddWarningWithoutValidationCheck(string message)
		{
			AddNotification(NotificationType.Warning, message, false);
		}

		public void AddNotification(INotificationType type, string message)
		{
			AddNotification(type, message, true);
		}

		void AddNotification(INotificationType type, string message, bool checkInfoCanBeSet)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (string.IsNullOrWhiteSpace(message))
			{
				ErrorReporter.ReportOnce("InvalidMessageValueForNotification", FormattableString.Invariant($"Cannot pass null/empty notification message for property '{Name}'. Additional info:'{HumanReadableName}-BizObj:{BizObj.HumanReadableName}'"));
			}

			AddNotificationCore(type, message, checkInfoCanBeSet);
		}

		protected virtual void AddNotificationCore(INotificationType type, string message, bool checkInfoCanBeSet)
		{
			if (checkInfoCanBeSet)
			{
				CheckInfoCanBeSet();
			}
			NotificationsInternal.Add(new PropertyNotification(Name, type, message));
			BizObj.NotifyPropertyNotificationsChanged(this, type, 1);
			BizObj.OnNotificationsChanged(true);
		}

		#endregion

		#region Has Notifications

		public bool HasError(string message)
		{
			return this.HasErrors() && Notifications.GetErrors().Contains(message);
		}

		public bool HasWarning(string message)
		{
			return this.HasWarnings() && Notifications.GetWarnings().Contains(message);
		}

		public bool HasMessageError(string message)
		{
			return this.HasMessageErrors() && Notifications.GetMessageErrors().Contains(message);
		}

		public bool HasNotification(string message)
		{
			return this.HasNotifications() && Notifications.Contains(message);
		}

		public bool HasNotification(INotificationType type, string message)
		{
			return
				NotificationsStorage != null &&
				NotificationsStorage.HasNotifications(type) &&
				Notifications.GetNotifications(type).Contains(message);
		}

		#endregion

		#region AddAllNotificationsFrom

		public void AddAllNotificationsFrom(ZPropertyInfo source)
		{
			AddAllNotificationsFromCore(source);
		}

		protected virtual void AddAllNotificationsFromCore(ZPropertyInfo source)
		{
			if (source.HasNotifications())
			{
				foreach (INotification notification in source.Notifications)
				{
					BizObj.NotifyPropertyNotificationsChanged(this, notification.Type, 1);
				}
				NotificationsInternal.AddRange(source.Notifications);
				BizObj.OnNotificationsChanged(true);
			}
		}

		#endregion

		#region Notification Storage

		public IEnumerable<INotification> Notifications
		{
			get { return NotificationsStorage ?? NotificationCollection.Empty; }
		}

		protected NotificationCollection NotificationsInternal
		{
			get { return NotificationsStorage ?? CreateNewZNotifications(); }
		}

		internal virtual NotificationCollection CreateNewZNotifications()
		{
			NotificationCollection result = new NotificationCollection();
			PropertyInfoStorage.NotificationDictionary[this] = result;
			return result;
		}

		protected virtual NotificationCollection NotificationsStorage
		{
			get
			{
				NotificationCollection result;
				PropertyInfoStorage.NotificationDictionary.TryGetValue(this, out result);
				return result;
			}
		}

		#endregion

		#region CheckInfoCanBeSet / CheckInfoCanBeCleared

		[System.Diagnostics.Conditional("DEBUG")]
		void CheckInfoCanBeSet()
		{
#if DEBUG
			BizObj.CheckValidationAction(this);
#endif
		}

		[System.Diagnostics.Conditional("DEBUG")]
		void CheckInfoCanBeCleared()
		{
#if DEBUG
			BizObj.CheckValidationActionClear(this);
#endif
		}

		#endregion

		#endregion

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			AddNotification(notification.Type, notification.Message);
		}

		#endregion

		#region INotificationProvider Members

		bool INotificationProvider.HasNotifications(INotificationType type)
		{
			return NotificationsStorage != null && NotificationsStorage.HasNotifications(type);
		}

		public bool HasNotifications()
		{
			return NotificationsStorage != null && NotificationsStorage.HasNotifications();
		}

		public INotificationType GetHighestSeverityNotificationType()
		{
			return NotificationsStorage == null ? null : NotificationsStorage.GetHighestSeverityNotificationType();
		}

		#endregion

		#region IAmZPropertyInfoInternals Members

		void IZPropertyInfoInternals.SetHumanReadableNameFromGuiCaption(ZString guiCaption)
		{
			SetHumanReadableNameCore(guiCaption, true);
		}

		internal bool HasValidationBeenRun
		{
			get { return BizObj.Factory != null && BizObj.Factory.HasValidationBeenRun(this); }
		}

		internal void SetValidationHasBeenRun()
		{
			if (BizObj.Factory != null)
			{
				BizObj.Factory.SetHasValidationBeenRun(this);
			}
		}

		#endregion

		public bool IsNAddInfoField()
		{
			var propertyInfoString = this as ZPropertyInfoString;
			if (propertyInfoString != null)
			{
				return PropertyDescriptor.Attributes[typeof(IsNAddInfoFieldAttribute)] != null;
			}

			var wrapperPropertyInfo = this as ZWrappedPropertyInfo;
			return wrapperPropertyInfo != null && wrapperPropertyInfo.InnerInfo.IsNAddInfoField();
		}

		public CustomizableDataResourceStrings CustomizableDataResourceStrings
		{
			get { return (CustomizableDataResourceStrings)MetaData.GetMetaData(BizObj, PropertyDescriptor, TranslatableDataFieldAttribute.CDRSMetaDataTypeId); }
		}

		#region Implementation

		public readonly BusinessObject BizObj;

		protected internal virtual void SetMetaDataProperty(string metaDataTypeId, object value)
		{
			KPropertyDescriptor metaDataProperty = MetaData.GetMetaDataProperty(BizObj.GetType(), PropertyDescriptor, metaDataTypeId);
			if (metaDataProperty != null && metaDataProperty.HasSetter())
			{
				if (!object.Equals(metaDataProperty.GetValue(BizObj), value))
				{
					metaDataProperty.SetValue(BizObj, value);
					BizObj.OnElementChanged();
				}
			}
			else
			{
				BizObj.SetObsoletePropertyInfoMetaData(metaDataTypeId, Name, value);
			}
		}

		internal ZPropertyInfoStorage PropertyInfoStorage
		{
			get { return BizObj.PropertyInfoStorage; }
		}

		#endregion
	}
}
