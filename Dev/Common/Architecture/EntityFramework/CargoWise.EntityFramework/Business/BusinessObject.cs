using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Microsoft.SqlServer.Types;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Represents a real world entity and encapsulates all logic to do with the entity.
	/// </summary>
	[IncludeOnlyNamedPropertiesForPropertyDescriptorReflection(IncludedPropertyNames = new[] { "PK", "ReadOnly", "IsInDatabase", "HumanReadableName" })]
	public abstract partial class BusinessObject : ZCustomTypeDescriptor,
		IBusiness,
		INeedTable,
		IBusinessObjectInternals,
		IAccessBusinessObject,
		ICodeDescription,
		IFactoryProvider,
		ILinkable,
		ComponentModel.INullable,
		ICanDelete,
		IColumnIndexer,
		ICanDetach,
		ICanSuspendSettingHasChanges
	{
		#region Fields

		// keep all class fields here so that per instance memory may be quantified

		ZGuid pk;
		readonly BusinessObjectFactory factory;
		ZPropertyInfoHashtable fZPropertyInfoHash;
		ZPropertyInfoStorage propertyInfoStorage;

		Flags currentMask;

		int SuspendMarkingAsNeedingValidationIndex;
		int readOnlyIndex;
		int preSaveValidationDepthCount;
		uint lastChangeNumber;

		NotificationCollection rowNotifications;

		PropertyInfo typedValidationInfo;
		bool hasNoValidation;
		IBusiness[] children = noChildren;

		IValidateForController fValidationFromController;
		Dictionary<MetaDataAndPropertyName, object> obsoleteMetaData;

		KeyValuePair<INotificationType, int>[] notificationTypeCounts;
		IBusinessObjectStrategy[] defaultStrategies;

#if DEBUG
		bool IsReloading; // this isn't used by any release code: causes a warning and was not optimized.
#endif

		#region Statics

		[SuppressThreadStaticFieldMessage]
		static readonly IBusiness[] noChildren = Array.Empty<IBusiness>();
		[SuppressThreadStaticFieldMessage]
		static readonly LRUCache<Type, PropertyDescriptor[]> childEditableProperties = new LRUCache<Type, PropertyDescriptor[]>(PropertyDescriptorCollectionFactory.NumberOfTypesToStringReferenceCache);

		#endregion

		#endregion

		#region Null Object Support

		public bool IsNull
		{
			[DebuggerStepThrough]
			get { return GetBool(Flags.Null); }
			set { SetBool(Flags.Null, value); }
		}

		[DebuggerStepThrough]
		public static bool operator !=(BusinessObject a, object b)
		{
			return !(a == b);
		}

		[DebuggerStepThrough]
		public static bool operator ==(BusinessObject a, object b)
		{
			if (((object)a) != null && b == null)
			{
				return a.IsNull;
			}
			else if (((object)a) == null && b != null)
			{
				return b is BusinessObject && ((BusinessObject)b).IsNull;
			}
			else
			{
				return ((object)a) == b;
			}
		}

		[DebuggerStepThrough]
		public override bool Equals(object obj)
		{
			return this == obj;
		}

		[SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required for Equals override")]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion

		protected BusinessObject(BusinessObjectFactory factory, DataRow row)
			: this(row)
		{
			SetInstantiationTime();

			using (SuspendListChanged())
			{
				this.factory = factory;
				InitialiseBusinessObject();
			}
			if (BusinessObjectCreationStackTraceRecorder.IsEnabled)
			{
				creationStackTrace = new StackTrace().ToString();
			}
		}

		protected virtual void SetInstantiationTime()
		{
			InstantiationTime = ZDateTime.Now;
		}

		#region Boolean State

		[Flags]
		protected enum Flags
		{
			Null = 1,
			TopLevel = 1 << 1,

			HasChanges = 1 << 2,
			AcceptChangesDelayed = 1 << 3,

			Copying = 1 << 4,
			Deleting = 1 << 5,
			Deleted = 1 << 6,

			DefaultValuesCalled = 1 << 7,
			ListChangedCalled = 1 << 8,
			OnSavingCalled = 1 << 9,

			ReadOnly = 1 << 10,
			ReadOnlySet = 1 << 11,

			LightValidationIsValid = 1 << 12,
			LightValidationIsValidSet = 1 << 13,
			DefaultStrategiesRetrieved = 1 << 14,

			RemovingFromRelationship = 1 << 15,

			FetchForLoadChildEditableObjectsCalled = 1 << 16,

			DataRowHasNotBeenAddedToDataTableYet = 1 << 17,

			FromUniversalCopy = 1 << 18,
		}

		bool GetBool(Flags flags)
		{
			return (currentMask & flags) == flags;
		}

		void SetBool(Flags flags, bool value)
		{
			if (value)
			{
				currentMask = currentMask | flags;
			}
			else
			{
				currentMask = currentMask & ~flags;
			}
		}

		#endregion

		#region Table Prefix

		public virtual string TablePrefix
		{
			get
			{
				if (tablePrefix == null)
				{
					tablePrefix = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(TableName);
				}
				return tablePrefix;
			}
		}
		string tablePrefix;

		#endregion

		#region Light Validation Support

		public bool ShouldValidateOnSave
		{
			get { return !IsDeleted && (!LightValidationEnabled || !LightValidationIsValid); }
		}

		public bool LightValidationEnabled
		{
			get
			{
				return EnableLightValidationIfAvailable
				&& (this is ILightValidationInternals)
				&& ObjectFactory.Get<IEntityFrameworkSettings>().LightValidationEnabled;
			}
		}

		protected virtual bool EnableLightValidationIfAvailable
		{
			get { return true; }
		}

		public void MarkAsNeedingValidation()
		{
			if (!IsDeleted && LightValidationEnabled && !IsMarkingAsNeedingValidationSuspended)
			{
				LightValidationIsValid = false;

#if DEBUG
				if (Factory != null)
				{
					Factory.NotifyMarkedAsNeedingValidation(this);
				}

				ManualCallsToMarkAsNeedingValidation_CountForTesting++;
#endif

				MarkAsNeedingValidationCore();

				if (!this.IsDeleting && this is IAddInfoChildSupporter addInfoChildSupporter)
				{
					addInfoChildSupporter.AddInfoChild?.MarkAsNeedingValidation();
				}
			}
		}

		protected virtual void MarkAsNeedingValidationCore()
		{
		}

#if DEBUG
		internal long ManualCallsToMarkAsNeedingValidation_CountForTesting;
#endif

		public virtual void MarkAsNeedingValidationIncludingChildren()
		{
			LoadChildEditableObjects();
			MarkAsNeedingValidation();
			Array.ForEach(GetChildren(), child => child.MarkAsNeedingValidationIncludingChildren());
		}

		public bool LightValidationIsValid
		{
			get
			{
				CheckCanGetSetLightValidationIsValid();
				return IsLightValidationValid ?? ((ILightValidationInternals)this).IsValid;
			}
			internal set
			{
				CheckCanGetSetLightValidationIsValid();
				IsLightValidationValid = value;
			}
		}

#if DEBUG
		public void MarkLightValidationAsValidForTesting()
		{
			LightValidationIsValid = true;
		}

		protected internal virtual void ResetHasChangesForTest()
		{
			HasChanges = false;
		}
#endif

		void CheckCanGetSetLightValidationIsValid()
		{
			if (!EnableLightValidationIfAvailable || !(this is ILightValidationInternals))
			{
				throw new NotSupportedException("LightValidationIsValid cannot be used as light validation is not enabled for " + GetType().FullName);
			}
		}

		void UpdateLightValidationPersistentPropertyIfRequired()
		{
			if (!IsDeleted && IsLightValidationValid.HasValue)
			{
				using (SuspendMarkingAsNeedingValidation())
				{
					((ILightValidationInternals)this).IsValid = IsLightValidationValid.Value;
					IsLightValidationValid = null;
				}
			}
		}

		bool? IsLightValidationValid
		{
			get
			{
				return GetBool(Flags.LightValidationIsValidSet)
						? GetBool(Flags.LightValidationIsValid)
						: null;
			}
			set
			{
				if (value == null)
				{
					SetBool(Flags.LightValidationIsValidSet, false);
				}
				else
				{
					SetBool(Flags.LightValidationIsValidSet, true);
					SetBool(Flags.LightValidationIsValid, value.Value);
				}
			}
		}

		/// <summary>
		/// Use when it is certain that changes about to be made do not affect validation state
		/// eg. When AU AddInfo is serialised to XX_AddInfo, we do not want to mark parent as needing validation.
		/// </summary>
		/// <returns></returns>
		public IDisposable SuspendMarkingAsNeedingValidation()
		{
			return new MarkAsNeedingValidationSuspender(this);
		}

		public bool IsMarkingAsNeedingValidationSuspended
		{
			get { return SuspendMarkingAsNeedingValidationIndex > 0; }
		}

		sealed class MarkAsNeedingValidationSuspender : IDisposable
		{
			public MarkAsNeedingValidationSuspender(BusinessObject bizObj)
			{
				this.bizObj = bizObj;
				bizObj.SuspendMarkingAsNeedingValidationIndex++;
			}

			readonly BusinessObject bizObj;

			public void Dispose()
			{
				bizObj.SuspendMarkingAsNeedingValidationIndex--;
			}
		}

		string IBusinessObjectInternals.CreationStackTrace => creationStackTrace;
		readonly string creationStackTrace;

		void IBusinessObjectInternals.ValidateShallowIfImprovesPreSaveValidationPerformance()
		{
			ValidateShallowIfImprovesPreSaveValidationPerformance();
		}

		internal void ValidateShallowIfImprovesPreSaveValidationPerformance()
		{
			if (!IsValidationSuspended)
			{
				((IBusiness)this).ValidateIfQuickAndImprovesPreSaveValidationPerformance();
				for (int i = 0; i < GetChildren().Length; i++)
				{
					GetChildren()[i].ValidateIfQuickAndImprovesPreSaveValidationPerformance();
				}
			}
		}

		void IBusiness.ValidateIfQuickAndImprovesPreSaveValidationPerformance()
		{
			if (LightValidationEnabled && ShouldValidateOnSave)
			{
				RunPreSaveValidationInternal(false);
			}
		}

		void EnsureLightValidationIsIgnoredForConcurrency()
		{
			ILightValidationInternals asLightValidator = this as ILightValidationInternals;

			if (asLightValidator != null && IsInDatabase)
			{
				Row.SetConcurrencyPolicy(asLightValidator.IsValidSchemaColumn.Name, LightValidationConcurrencyPolicy.Instance);
			}
		}

		void EnsureBlobFieldForConcurrencyCheck()
		{
			var row = ((INeedRow)this).Row;
			if (row != null)
			{
				var schemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
				if (!row.Table.TableName.Equals("OrgPatternMatch", StringComparison.OrdinalIgnoreCase))
				{
					for (int i = 0; i < row.Table.Columns.Count; i++)
					{
						var col = row.Table.Columns[i];
						var schemaColumn = schemaResolver.GetSchemaColumnSafe(col.ColumnName, row.Table.TableName);
						if (schemaColumn == null)
						{
							continue;
						}

						var policy = ConcurrencyInfo.Get(row, col);
						if (schemaColumn.IsLargeBinaryOrText && policy != ConcurrencyPolicy.Ignore && policy != ConcurrencyPolicy.Default)
						{
							EnsureBlobField(schemaColumn);
						}
					}
				}
			}
		}

		#endregion

		#region Testing Only
#if DEBUG

		/// <summary>
		/// Tests validation integrity for classes using new validation.
		/// </summary>
		internal bool ValidationTestingEnabled
		{
			get
			{
				if (!validationTestingEnabledAlreadySet)
				{
					validationTestingEnabledAlreadySet = true;
					validationTestingEnabled = true;
				}
				return validationTestingEnabled;
			}
		}

		/// <summary>
		/// Use this method only if you're directly calling validation methods from within your testcase.
		/// DO NOT use this to merely suppress test failures.
		/// </summary>
		public IDisposable SuspendValidationTesting()
		{
			return new ValidationTestingSuspender(this);
		}

		class ValidationTestingSuspender : IDisposable
		{
			public ValidationTestingSuspender(BusinessObject bizObj)
			{
				this.bizObj = bizObj;
				bizObj.validationTestingEnabled = false;
				bizObj.validationTestingEnabledAlreadySet = true;
				DisposableLeakListener.Instance.RegisterDisposable(this);
			}

			public void Dispose()
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
				bizObj.validationTestingEnabledAlreadySet = false;
			}

			readonly BusinessObject bizObj;
		}

		protected internal void AddInfoForValidation(ZPropertyInfo propertyInfo)
		{
			if (propertyInfosCurrentlyValidating == null)
			{
				propertyInfosCurrentlyValidating = new List<ZPropertyInfo>();
			}
			propertyInfosCurrentlyValidating.Add(propertyInfo);
		}

		protected internal void RemoveInfoForValidation(ZPropertyInfo propertyInfo)
		{
			if (CanSetValidationOnInfo(propertyInfo))
			{
				int count = propertyInfosCurrentlyValidating.Count;
				if (count > 1)
				{
					propertyInfosCurrentlyValidating.RemoveAt(propertyInfosCurrentlyValidating.Count - 1);
				}
				else
				{
					propertyInfosCurrentlyValidating = null;
				}
			}
		}

		internal ZPropertyInfo PropertyInfoCurrentlyClearing
		{
			get { return propertyInfoCurrentlyClearing; }
			set { propertyInfoCurrentlyClearing = value; }
		}

		public bool CanSetValidationOnInfo(ZPropertyInfo propertyInfo)
		{
			return (propertyInfosCurrentlyValidating != null) && (propertyInfo == propertyInfosCurrentlyValidating[propertyInfosCurrentlyValidating.Count - 1]);
		}

		public void CheckValidationAction(ZPropertyInfo propertyInfo)
		{
			if (ValidationTestingEnabled)
			{
				if (!(propertyInfo.BizObj is IObsoleteValidation))
				{
					if (!CanSetValidationOnInfo(propertyInfo))
					{
						ReportValidationError("Validation", propertyInfo, "Attempt to change validation on a property info outside of its Check method");
					}
				}
			}
		}

		public void CheckValidationActionClear(ZPropertyInfo propertyInfo)
		{
			if (ValidationTestingEnabled)
			{
				if (!(propertyInfo.BizObj is IObsoleteValidation))
				{
					if (propertyInfoCurrentlyClearing != propertyInfo && !IsReloading)
					{
						ReportValidationError("CheckValidationActionClear", propertyInfo, "ClearAllNotifications should not be called when using New validation.");
					}
				}
			}
		}

		void ReportValidationError(string key, ZPropertyInfo info, string description)
		{
			ErrorReporter.ReportOnce(key + ":" + info.Name, description + System.Environment.NewLine +
					"Field: " + info.BizObj.GetType().ToString() + "." + info.Name + System.Environment.NewLine);
		}

		bool validationTestingEnabledAlreadySet;
		bool validationTestingEnabled;
		ZPropertyInfo propertyInfoCurrentlyClearing;
		List<ZPropertyInfo> propertyInfosCurrentlyValidating;

		#region FillWithValidTestData / FillWithValidTestDataCore

		public void FillWithValidTestData()
		{
			FillWithValidTestData(TestBusinessObjectKind.MinimumRequiredToSave, Array.Empty<PropertyDescriptor>());
		}

		public void FillWithValidTestData(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			using (GetValidationSuspender())
			{
				IsCopying = true;
				try
				{
					if (ValidTestDataGenerationException.ShouldThrow)
					{
						throw new ValidTestDataGenerationException("You should only use this during a unit test");
					}
					if (!hasRunFillWithValidTestData)
					{
						hasRunFillWithValidTestData = true;
						FillWithValidTestDataCore(kind, propertyPath);
					}
				}
				finally
				{
					IsCopying = false;
				}
			}
		}

		protected virtual void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			NewBusinessObjectTestDataHelper().FillWithValidTestData(this, kind, propertyPath);
		}

		protected virtual BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return TestDataHelper ?? new BusinessObjectTestDataHelper();
		}

		bool hasRunFillWithValidTestData;

		[NonSerialized()]
		internal BusinessObjectTestDataHelper TestDataHelper;

		#endregion
#endif

		#endregion

		#region Change Number

		public uint LastChangeNumber
		{
			get { return lastChangeNumber; }
		}

		void IncrementChangeNumber()
		{
			lastChangeNumber = Factory.GetNextChangeNumber();
		}

		#endregion

		#region Factory

		public BusinessObjectFactory Factory
		{
			[DebuggerStepThrough]
			get { return factory; }
		}

		protected virtual void AddToFactoryCache()
		{
			factory.AddNewBusinessObjectToCache(this);
		}

		internal void AddToFactoryCacheInternal()
		{
			if (factory != null)
			{
				AddToFactoryCache();
			}
		}

		#endregion

		#region Set Default Values

		internal bool SetDefaultValuesCalled
		{
			get { return GetBool(Flags.DefaultValuesCalled); }
		}

		void SetDefaultValuesInternal()
		{
			SetDefaultValues();
			SetXmlColumnDefaultValues();
			if (this is IAddInfoChildSupporter supporter)
			{
				_ = supporter.AddInfoChild; // Ensure AddInfoChild is created
			}
			if (this is IAddInfoWithSyncPropertySupporter addInfoWithSyncPropertySupporter)
			{
				addInfoWithSyncPropertySupporter.AddInfo?.EnableSynchronization();
			}
		}

		void SetXmlColumnDefaultValues()
		{
			if (XmlSerialisedColumns.Any())
			{
				foreach (var column in BusinessObjectXmlHelper.GetXmlColumnStrategies(this))
				{
					if (column.DefaultValue != null)
					{
						BusinessObjectXmlHelper.SetValue(column.DefaultValue, column, this);
					}
				}

				BusinessObjectXmlHelper.SerialiseXmlColumns(this);
			}
		}

		/// <summary>
		/// SetDefaultValues() will be called during construction of the business object.
		/// Override this method to set your business object's default values.
		/// </summary>
		protected virtual void SetDefaultValues()
		{
#if DEBUG
			SetBool(Flags.DefaultValuesCalled, true);
#endif
		}

		#endregion

		#region IsNullOrDeleted

		public static bool IsNullOrDeleted(BusinessObject bizObj)
		{
			return bizObj == null || bizObj.IsDeleted;
		}

		#endregion

		#region GetActiveFilter

		public static ZQuery GetActiveFilter(Type bizObjType)
		{
			PropertyInfo property = bizObjType.GetProperty("ActiveFilter", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static);
			if (property != null)
			{
				ZQuery query = (ZQuery)property.GetValue(null, null);
				return query.DeepClone();
			}

			return null;
		}

		#endregion

		#region Notifications

		IEnumerable<INotification> INotificationProvider.Notifications
		{
			get { return NotificationsIncludingChildren; }
		}

		public IEnumerable<INotification> Notifications
		{
			get { return new ZNotificationCollector(this, false, true, ZNotificationCollector.PropertyDescriptionType.ColumnName); }
		}

		public IEnumerable<INotification> NotificationsIncludingChildren
		{
			get { return new ZNotificationCollector(this, true, true, ZNotificationCollector.PropertyDescriptionType.ColumnName); }
		}

		public INotificationType GetHighestSeverityNotificationType()
		{
			return GetHighestSeverityNotificationTypesIncludingChildren().GetHighestSeverityNotificationType();
		}

		IEnumerable<INotificationType> GetHighestSeverityNotificationTypesIncludingChildren()
		{
			var highestPropertyNotification = notificationTypeCounts?.Select(n => n.Key).GetHighestSeverityNotificationType();
			if (highestPropertyNotification != null)
			{
				yield return highestPropertyNotification;
			}

			var highestRowNotification = rowNotifications?.GetHighestSeverityNotificationType();
			if (highestRowNotification != null)
			{
				yield return highestRowNotification;
			}

			foreach (var business in children)
			{
				var bizo = business as BusinessObject;
				if (bizo == null || !bizo.IsTopLevel)
				{
					yield return business.GetHighestSeverityNotificationType();
				}
			}
		}

		#region Checking for Errors/Warnings (property errors & row errors)

		/// <summary>
		/// An array of all PropertyInfo that have errors, warnings or message errors.
		/// </summary>
		public ZPropertyInfo[] PropertiesWithNotifications
		{
			get
			{
				List<ZPropertyInfo> result = new List<ZPropertyInfo>();
				foreach (ZPropertyInfo info in ZPropertyInfoHash)
				{
					if (info.HasNotifications())
					{
						result.Add(info);
					}
				}

				return result.ToArray();
			}
		}

		/// <summary>
		/// Clear all row and property notifications on the business object.
		/// </summary>
		public void ClearAllNotifications()
		{
			ClearAllNotificationsCore();
		}

		protected virtual void ClearAllNotificationsCore()
		{
			ClearRowNotifications();

			foreach (ZPropertyInfo info in PropertiesWithNotifications)
			{
				info.ClearAllNotifications();
			}
		}

		#region Has Notifications (Errors, Warnings, MessageErrors)

		#region HasErrors

		/// <summary>
		/// Does the business object, or any of its children, have errors?
		/// </summary>
		public bool HasErrors
		{
			get { return HasNotifications(NotificationType.Error); }
		}

		/// <summary>
		/// Does the business object and the business object alone have any errors?
		/// </summary>
		public bool HasErrorsNotIncludingChildren
		{
			get { return HasNotificationsNotIncludingChildren(NotificationType.Error); }
		}

		internal bool HasErrorsOnChild
		{
			get { return HasNotificationsOnChild(NotificationType.Error); }
		}

		#endregion

		#region HasMessageErrors

		/// <summary>
		/// Does the business object have message errors?
		/// </summary>
		public bool HasMessageErrors
		{
			get { return HasNotifications(NotificationType.MessageError); }
		}

		/// <summary>
		/// Does the business object and the business object alone have any message errors?
		/// </summary>
		public bool HasMessageErrorsNotIncludingChildren
		{
			get { return HasNotificationsNotIncludingChildren(NotificationType.MessageError); }
		}

		internal bool HasMessageErrorsOnChild
		{
			get { return HasNotificationsOnChild(NotificationType.MessageError); }
		}

		#endregion

		#region HasWarnings

		/// <summary>
		/// Does the business object have warnings?
		/// </summary>
		public bool HasWarnings
		{
			get { return HasNotifications(NotificationType.Warning); }
		}

		internal bool HasWarningsOnChild
		{
			get { return HasNotificationsOnChild(NotificationType.Warning); }
		}

		#endregion

		#region HasNotifications

		public bool HasNotifications()
		{
			return HasNotificationsCore();
		}

		protected virtual bool HasNotificationsCore()
		{
			return
				(notificationTypeCounts != null && notificationTypeCounts.Length > 0) ||
				(rowNotifications != null && rowNotifications.HasNotifications()) ||
				HasNotificationsOnChild();
		}

		public bool HasNotifications(INotificationType type)
		{
			return
				HasNotificationsNotIncludingChildren(type) ||
				HasNotificationsOnChild(type);
		}

#if DEBUG
		protected virtual
#endif
		bool HasNotificationsNotIncludingChildren(INotificationType notificationType)
		{
			return
				!IsDeleted &&
				(
					(notificationTypeCounts != null && notificationTypeCounts.Any(n => n.Key == notificationType)) ||
					(rowNotifications != null && rowNotifications.HasNotifications(notificationType)) ||
					HasZWrappedPropertyInfoNotifications(notificationType)
				);
		}

		#region ZWrappedPropertyInfoNotifications

		bool HasZWrappedPropertyInfoNotifications(INotificationType notificationType)
		{
			foreach (ZWrappedPropertyInfo property in ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.Wrapping))
			{
				if (property.IsLoaded && property.InnerInfo != null && property.InnerInfo.BizObj != this)
				{
					if (((INotificationProvider)property).HasNotifications(notificationType))
					{
						return true;
					}
				}
			}
			return false;
		}

		internal void MarkWrappedPropertyAsLoaded(ZWrappedPropertyInfo info)
		{
			int infoIndex = GetWrappedPropertyIndex(info);
			if (infoIndex != -1)
			{
				AdjustWrappedPropertyFlagsLength(infoIndex);
				wrappedPropetiesLoadedFlags.Set(infoIndex, true);
			}
		}

		internal bool IsWrappedPropertyLoaded(ZWrappedPropertyInfo info)
		{
			int infoIndex = GetWrappedPropertyIndex(info);

			return infoIndex != -1 &&
					 wrappedPropetiesLoadedFlags != null &&
					 wrappedPropetiesLoadedFlags.Count > infoIndex &&
					 wrappedPropetiesLoadedFlags.Get(infoIndex);
		}

		static readonly object wrappedPropertyIndex = new object();

		int GetWrappedPropertyIndex(ZWrappedPropertyInfo info)
		{
			KPropertyDescriptor descriptor = (KPropertyDescriptor)info.PropertyDescriptor;
			int? index = (int?)descriptor.UserData[wrappedPropertyIndex];

			if (index == null)
			{
				index = GetWrappedPropertyIndexFromPropertyInfoHash(info);
				descriptor.UserData[wrappedPropertyIndex] = index;
			}

			return index.Value;
		}

		int GetWrappedPropertyIndexFromPropertyInfoHash(ZWrappedPropertyInfo info)
		{
			int index = -1;

			foreach (ZWrappedPropertyInfo property in ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.Wrapping))
			{
				index++;
				if (info.PropertyDescriptor.Equals(property.PropertyDescriptor))
				{
					break;
				}
			}

			return index;
		}

		void AdjustWrappedPropertyFlagsLength(int index)
		{
			int newLength = ((index + 33) / 32) * 32;
			if (wrappedPropetiesLoadedFlags == null)
			{
				wrappedPropetiesLoadedFlags = new BitArray(newLength);
			}
			else if (wrappedPropetiesLoadedFlags.Length < newLength)
			{
				wrappedPropetiesLoadedFlags.Length = newLength;
			}
		}

		BitArray wrappedPropetiesLoadedFlags;

		#endregion

		internal void NotifyPropertyNotificationsChanged(ZPropertyInfo property, INotificationType type, int count)
		{
			if (IsNull)
			{
				ErrorReporter.ReportOnce("NullObjectPropertyCountChanged", "Attempted to change Error Count on 'Null' business object");
			}

			if (notificationTypeCounts == null)
			{
				notificationTypeCounts = new KeyValuePair<INotificationType, int>[1] { new KeyValuePair<INotificationType, int>(type, count) };
			}
			else
			{
				List<KeyValuePair<INotificationType, int>> list = new List<KeyValuePair<INotificationType, int>>(3);
				bool found = false;
				foreach (KeyValuePair<INotificationType, int> item in notificationTypeCounts)
				{
					if (item.Key == type)
					{
						int newCount = item.Value + count;
						if (newCount != 0)
						{
							list.Add(new KeyValuePair<INotificationType, int>(item.Key, item.Value + count));
						}
						found = true;
					}
					else
					{
						list.Add(item);
					}
				}
				if (!found)
				{
					list.Add(new KeyValuePair<INotificationType, int>(type, count));
				}
				notificationTypeCounts = list.Count == 0 ? null : list.ToArray();
			}
		}

		#endregion

		#region HasNotificationsOnChild

		string GenerateCyclicReferenceReport()
		{
			var message = new StringBuilder();
			message.Append(string.Format((NoResString)"We are a {0}.\r\nOur direct children are: ", this.GetType().Name));
			message.Append(GetChildren().Take(5).Select(x => x.GetType().Name).Aggregate((x, y) => { return x + ", " + y; }));
			var childCount = GetChildren().Length;
			if (childCount > 5)
			{
				message.AppendLine(string.Format((NoResString)" ({0} more not listed)", childCount - 5));
			}
			else
			{
				message.AppendLine(".");
			}
			message.Append(FindFirstReferenceToThisBizo(this, this, 0));
			return message.ToString();
		}

		string FindFirstReferenceToThisBizo(BusinessObject original, IBusiness current, int depth)
		{
			var result = FindFirstReferenceToThisBizoCore(original, current, depth);
			//kind of hacky, but distinguishing between different failure cases by returning some things that are not lists of cycles
			if (result == null)
			{
				return (NoResString)"Could not find a cyclic reference.";
			}
			else if (result.Count == 0)
			{
				return (NoResString)"Tried to find the cyclic reference, but depth exceeded 10.";
			}
			else
			{
				result.Reverse();
				return string.Format((NoResString)"The cycle is {0} -> (the start).", result.Aggregate((x, y) => { return x + " -> " + y; }));
			}
		}

		List<string> FindFirstReferenceToThisBizoCore(BusinessObject original, IBusiness current, int depth)
		{
			if (depth > 10)
			{
				return new List<string>();
			}

			List<string> result = null;

			foreach (IBusiness child in current.Children)
			{
				if (child == original)
				{
					return new List<string>() { current.GetType().Name };
				}
				else
				{
					result = FindFirstReferenceToThisBizoCore(original, child, depth + 1);
					if (result != null && result.Count > 0)
					{
						result.Add(current.GetType().Name);
						return result;
					}
				}
			}

			return result; //minor bug: 'couldn't find it' and 'depth exceeded 10' don't have any priority over each other. either way, we didn't find it though, so nbd
		}

		int inHasNotificationsOnChild;

		internal bool HasNotificationsOnChild()
		{
			if (inHasNotificationsOnChild < 2)
			{
				inHasNotificationsOnChild++;
				try
				{
					if (!IsDeleted)
					{
						foreach (IBusiness child in GetChildren())
						{
							BusinessObject bizo = child as BusinessObject;
							if ((bizo == null || !bizo.IsTopLevel) && child.HasNotifications())
							{
								return true;
							}
						}
					}
				}
				finally
				{
					inHasNotificationsOnChild--;
				}
			}
			else
			{
				ErrorReporter.ReportOnce("CyclicReferenceInChildren", GenerateCyclicReferenceReport());
			}

			return false;
		}

		internal bool HasNotificationsOnChild(INotificationType notificationType)
		{
			if (!IsDeleted)
			{
				foreach (IBusiness child in GetChildren())
				{
					BusinessObject bizo = child as BusinessObject;
					if ((bizo == null || !bizo.IsTopLevel) && child.HasNotifications(notificationType))
					{
						return true;
					}
				}
			}
			return false;
		}

		#endregion

		#endregion

		#region Row Notifications (Errors, Warnings, MessageErrors)

		public void ClearRowNotifications()
		{
			if (HasRowNotifications)
			{
				rowNotifications = null;
				OnNotificationsChanged(true);
			}
		}

		public void ClearRowNotificationsContaining(string partialMessage)
		{
			if (HasRowNotifications)
			{
				foreach (var notification in RowNotifications.Where(n => n.Message.IndexOf(partialMessage, StringComparison.CurrentCulture) >= 0).ToArray())
				{
					rowNotifications.Remove(notification.Type, notification.Message);
				}

				OnNotificationsChanged(true);
			}
		}

		public void AddRowError(string message)
		{
			AddRowNotification(new Notification(NotificationType.Error, message));
		}

		public void AddRowWarning(string message)
		{
			AddRowNotification(new Notification(NotificationType.Warning, message));
		}

		public void AddRowMessageError(string message)
		{
			AddRowNotification(new Notification(NotificationType.MessageError, message));
		}

		public void AddRowNotification(INotification notification)
		{
			RowNotificationsInternal.Add(notification);
			OnNotificationsChanged(true);
		}

		public void RemoveRowNotification(INotification notification)
		{
			RowNotificationsInternal.Remove(notification);
			OnNotificationsChanged(true);
		}

		public void RemoveRowError(string message)
		{
			RemoveRowError(message, false);
		}

		public void RemoveRowError(string message, bool containing)
		{
			RemoveRowNotification(NotificationType.Error, message, containing);
		}

		public void RemoveRowWarning(string message)
		{
			RemoveRowWarning(message, false);
		}

		public void RemoveRowWarning(string message, bool containing)
		{
			RemoveRowNotification(NotificationType.Warning, message, containing);
		}

		public void RemoveRowMessageError(string message)
		{
			RemoveRowMessageError(message, false);
		}

		public void RemoveRowMessageError(string message, bool containing = false)
		{
			RemoveRowNotification(NotificationType.MessageError, message, containing);
		}

		void RemoveRowNotification(INotificationType notificationType, string message, bool containing = false)
		{
			RowNotificationsInternal.Remove(notificationType, message, containing);
			OnNotificationsChanged(true);
		}

		public bool HasRowErrors
		{
			get { return rowNotifications != null && rowNotifications.HasErrors(); }
		}

		public bool HasRowWarnings
		{
			get { return rowNotifications != null && rowNotifications.HasWarnings(); }
		}

		public bool HasRowMessageErrors
		{
			get { return rowNotifications != null && rowNotifications.HasMessageErrors(); }
		}

		public bool HasRowNotifications
		{
			get { return rowNotifications != null && rowNotifications.HasNotifications(); }
		}

		internal NotificationCollection RowNotificationsInternal
		{
			get { return rowNotifications ?? (rowNotifications = new NotificationCollection()); }
		}

		public IEnumerable<INotification> RowNotifications
		{
			get { return rowNotifications ?? Enumerable.Empty<INotification>(); }
		}

		public IEnumerable<INotification> RowErrors
		{
			get { return RowNotifications.GetErrors(); }
		}

		public IEnumerable<INotification> RowWarnings
		{
			get { return RowNotifications.GetWarnings(); }
		}

		public IEnumerable<INotification> RowMessageErrors
		{
			get { return RowNotifications.GetMessageErrors(); }
		}

		#endregion

		#endregion

		#endregion

		#region Initialise Business Object (ie. Setting PK + Default Values etc.)

		/// <summary>
		/// The business object's primary key schema column.
		/// </summary>
		public abstract SchemaGuidColumn PKSchemaColumn
		{
			get;
		}

		internal virtual ZGuid GetPKInternal()
		{
			return pk;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected internal virtual void SetPKAndDefaults()
		{
			try
			{
				pk = new ZGuid(Row[PKSchemaColumn.Name]);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				if (Row.Table != null)
				{
					var columnsStrings = new List<string>();
					foreach (DataColumn column in Row.Table.Columns)
					{
						columnsStrings.Add(column.Caption + ", " + column.ColumnName + ", " + column.DataType.ToString());
					}
					string columnsString = string.Join("\r\n", columnsStrings);
					var error = string.Format((NoResString)"Row unexpectely lacks PKSchemaColumn.\r\nPKSchemaColumn.Name: {0}, Row.Table.TableName: {1}, this.GetType().ToString(): {2}. Columns information:\r\n{3}",
						PKSchemaColumn.Name, Row.Table.TableName, this.GetType().ToString(), columnsString);
					ErrorReporter.ReportOnce("PKSchemaColumnMissing", error, e);
				}
				throw;
			}

			if (pk.IsEmpty)
			{
				pk = ZGuid.NewZGuid();
				Row[PKSchemaColumn.Name] = pk.ToGuid();

				AddToFactoryCacheInternal();
				SetDefaultsSuspendingAndResumingHasChanges();
			}
			else
			{
				AddToFactoryCacheInternal();
			}
		}

		internal void SetDefaultsSuspendingAndResumingHasChanges()
		{
#if DEBUG
			if (Factory != null)
			{
				Factory.AddIgnoredBusinessObjectForTestingLightValidation(this);
			}
			try
			{
#endif
				using (SuspendSettingHasChanges())
				using (GetValidationSuspender())
				{
					SetDefaultValuesInternal();
					OnElementChanged();
				}
#if DEBUG
			}
			finally
			{
				if (Factory != null)
				{
					Factory.RemoveIgnoredBusinessObjectForTestingLightValidation(this);
				}
			}
#endif
		}

		#endregion

		#region Indexer by PropertyName

		[EditorBrowsable(EditorBrowsableState.Never)]
		[SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
		protected internal bool TryGetPropertyValueByName(string propertyName, out object result)
		{
			try
			{
				result = GetPropertyValueByName(propertyName);
				return true;
			}
			catch (ArgumentException)
			{
				result = null;
				return false;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected internal object GetPropertyValueByName(string propertyName)
		{
			Argument.NotNull(propertyName, "propertyName");
			object result = null;
			KPropertyDescriptor propertyDesc = GetPropertyDescriptor(propertyName);
			if (propertyDesc != null)
			{
				result = propertyDesc.GetValue(this);
			}
			else
			{
				if (PropertyNameIsPK(propertyName))
				{
					result = PK;
				}
				else
				{
					var splitterIndex = propertyName.IndexOfAny(new[] { '+', '.' });
					if (splitterIndex >= 0)
					{
						var outerPropertyName = propertyName.Substring(0, splitterIndex);
						var innerPropertyName = propertyName.Substring(splitterIndex + 1);

						try
						{
							var outerBizo = GetPropertyValueByName(outerPropertyName) as BusinessObject;
							if (outerBizo != null)
							{
								result = outerBizo.GetPropertyValueByName(innerPropertyName);
							}
						}
						catch (ArgumentException ex)
						{
							throw new ArgumentException(string.Format("{0}[{1}] is not a property of {0}.", GetType().Name, propertyName), ex);
						}
					}
					else
					{
						throw new ArgumentException(string.Format("{0}[{1}] is not a property of {0}.", GetType().Name, propertyName));
					}
				}
			}
			if (IsDeleted)
			{
				ReportRowDeletedError(propertyName, propertyValue: result);
			}
			return result;
		}

		internal KPropertyDescriptor GetPropertyDescriptor(string propertyName)
		{
			return (KPropertyDescriptor)GetProperties()[propertyName];
		}

		/// <summary>
		/// The property value for the specified column
		/// </summary>
		public object this[SchemaColumn column]
		{
			get
			{
				if (column == null)
				{
					throw new ArgumentNullException(nameof(column));
				}
				return this[column.Name];
			}
			set { this[column.Name] = value; }
		}

		/// <summary>
		/// The property value for the specified name.
		/// </summary>
		public virtual object this[string propertyName]
		{
			get { return GetPropertyValueByName(propertyName); }
			set
			{
				KPropertyDescriptor propertyDesc = (KPropertyDescriptor)GetProperties()[propertyName];
				if (propertyDesc == null)
				{
					string bizObjName = GetType().Name;
					throw new ArgumentException(bizObjName + "['" + propertyName + "'] is not a property of " + bizObjName + ".");
				}
				else if (!propertyDesc.HasSetter())
				{
					string bizObjName = GetType().Name;
					throw new ArgumentException(bizObjName + "['" + propertyName + "'] is read-only and cannot be modified (no Set() was found).");
				}
				else
				{
					propertyDesc.SetValue(this, ZDataType.ObjectToZType(propertyDesc.Inner.PropertyType, value));
				}
			}
		}

		public Type GetPropertyType(string propertyName)
		{
			KPropertyDescriptor propertyDesc = GetPropertyDescriptor(propertyName);
			return propertyDesc != null ? propertyDesc.PropertyType : null;
		}

		#endregion

		#region Clone, CopyPersistentValuesFrom and CopyValuesFrom

		protected virtual bool IsCopying
		{
			get { return GetBool(Flags.Copying); }
			private set { SetBool(Flags.Copying, value); }
		}

		/// <summary>
		/// Copy all persistent values from another BusinessObject into this one.
		/// Please note that this method ONLY copies the values in the rows based on ColumnNames
		/// </summary>
		public void CopyPersistentValuesFrom(BusinessObject sourceObject)
		{
			CopyPersistentValuesFrom(sourceObject, new BusinessObjectCloneArgs());
		}

		public void CopyPersistentValuesFrom(BusinessObject sourceObject, BusinessObjectCloneArgs args)
		{
			if (sourceObject == null)
			{
				throw new ArgumentNullException(nameof(sourceObject));
			}
			try
			{
				args.AddExcludedColumns(GetPropertiesThatShouldNotBeCopied());
				var fixValueProvider = args.GetValueOverrideProvider(sourceObject.GetType());

				CheckCanCopyPersistentValuesFrom();
				IsCopying = true;
				ISingleElementListInternal bizObjListInternals = this;
				using (SuspendListChanged())
				{
					foreach (DataColumn column in Row.Table.Columns)
					{
						var columnName = column.ColumnName;
						if (fixValueProvider != null && fixValueProvider.TryGetValue(columnName, out var fixValue))
						{
							SetValue(args, column, columnName, fixValue);
						}
						else if (!args.IsExcludedFromCloning(columnName)
							&& sourceObject.Row.Table.Columns.Contains(columnName)
							&& (args.CopyDecider?.Invoke(sourceObject, this, column) ?? true)
							&& !sourceObject.Row[columnName].Equals(Row[columnName])
							&& sourceObject.TryGetPropertyValueByName(columnName, out object r)
							&& r is IZTypeInternals source)
						{
							SetValue(args, column, columnName, source);
						}
					}
					CopyAdditionalPersistentValuesFrom(sourceObject, args);
				}
				OnElementChanged();
			}
			catch (RowNotInTableException ex)
			{
				ErrorReporter.ReportOnce($"CopyPersistentValuesFrom_RowNotInTableException[{sourceObject.GetType().FullName}]", $"sourceObject.Row.RowState is {sourceObject.Row.RowState}, current.Row.RowState is {Row.RowState}.", ex);
			}
			finally
			{
				IsCopying = false;
			}
		}

		protected virtual void CopyAdditionalPersistentValuesFrom(BusinessObject sourceObject, BusinessObjectCloneArgs args)
		{
		}

		void SetValue(BusinessObjectCloneArgs args, DataColumn column, string columnName, IZTypeInternals source)
		{
			if (args.PerformRowCopyWithoutTriggeringValidationAndSetter)
			{
				Row[columnName] = source.GetValueForLogicalDataLayer(column.AllowDBNull);
			}
			else
			{
				this[columnName] = source;
			}
		}

		protected virtual void CheckCanCopyPersistentValuesFrom()
		{
		}

		/// <summary>
		/// Copy ALL values from another BusinessObject into this one.
		/// </summary>
		protected void CopyValuesFrom(BusinessObject sourceObject)
		{
			IsCopying = true;
			try
			{
				if (!(sourceObject.GetType().IsSubclassOf(this.GetType()) || sourceObject.GetType().Equals(this.GetType())))
				{
					throw new ApplicationException("Can not CopyValuesFrom this business object as it is only possible to copy from objects of the same type or child types." + System.Environment.NewLine +
						"Source Object Type = " + sourceObject.GetType().FullName + System.Environment.NewLine +
						"This Object Type = " + this.GetType().FullName);
				}
				else
				{
					ISingleElementListInternal bizObjListInternals = this;
					using (bizObjListInternals.SuspendListChanged())
					using (GetValidationSuspender())
					{
						foreach (ZPropertyInfo propertyInfo in ZPropertyInfoHash)
						{
							if (propertyInfo.HasSetter)
							{
								PropertyInfo property = GetType().GetProperty(propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public);
								if (property != null)
								{
									this[propertyInfo.Name] = sourceObject[propertyInfo.Name];
								}
							}
						}
					}
					OnElementChanged();
				}
			}
			finally
			{
				IsCopying = false;
			}
		}

		protected IEnumerable<string> GetPropertiesThatShouldNotBeCopied()
		{
			List<string> list = new List<string>();
			string tablePrefix = TablePrefix;
			if ((tablePrefix.Length == 2 || tablePrefix.Length == 3) && !tablePrefix.EndsWith("_", StringComparison.Ordinal))
			{
				tablePrefix += "_";
			}

			list.Add(ZDataUtils.GetPKNameFromTable(Table));
			list.Add(tablePrefix + "SystemCreateTimeUtc");
			list.Add(tablePrefix + "SystemCreateUser");
			list.Add(tablePrefix + "SystemCreateBranch");
			list.Add(tablePrefix + "SystemCreateDepartment");
			list.Add(tablePrefix + "IsValid");
			if (this is IClusterKeyEntity clusterKeyEntity)
			{
				list.Add(clusterKeyEntity.ClusterKeyPty.Name);
			}

			list.AddRange(GetPropertiesToExcludeFromCloning());

			return list;
		}

		protected virtual IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return Array.Empty<string>();
		}

		/// <summary>
		/// Create a new Business Object of this type and copy all persistent values to it.
		/// </summary>
		public BusinessObject Clone()
		{
			return Clone(new BusinessObjectCloneArgs(Array.Empty<string>(), GetType()));
		}

		public BusinessObject Clone(BusinessObjectCloneArgs args)
		{
			if (!SupportsClone())
			{
				ErrorReporter.ReportOnce("CloneInternalNeedsOverride" + GetType().FullName, "Clone() not supported on this business object (" + GetType().FullName + "). To support cloning on this business object, override SupportsClone() and return true, and if you have dependent collections or other dependent relationships override CloneInternal() on your BusinessObject to provide this functionality.");
			}
			return CloneInternal(args);
		}

		/// <summary>
		/// If you need to override this method, consider building a BusinessObjectCloneStrategy instead and let it handle Clone
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		internal protected virtual BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			Type typeToCloneAs = args.TypeToCloneAs ?? GetType();

			BusinessObjectFactory factory = args.AlternativeFactoryToInstantiateCloneIn ?? Factory;

			BusinessObject result = factory.New(typeToCloneAs);

			using (result.GetValidationSuspender())
			using (result.SuspendSettingHasChanges())
			using (result.MarkAsBeingCloned())
			{
				result.CopyPersistentValuesFrom(this, args);
				CopyXmlColumns(result);
				CopyAddInfoChild(result);
			}

			return result;
		}

		void CopyAddInfoChild(BusinessObject copy)
		{
			if (this is IAddInfoChildSupporter addInfoSupporter && copy is IAddInfoChildSupporter copyAddInfoSupporter
				&& addInfoSupporter.AddInfoChild is BusinessObject addInfoChild && copyAddInfoSupporter.AddInfoChild is BusinessObject copyAddInfoChild)
			{
				using (copyAddInfoChild.GetValidationSuspender())
				using (copyAddInfoChild.SuspendSettingHasChanges())
				using (copyAddInfoChild.MarkAsBeingCloned())
				{
					copyAddInfoChild.CopyPersistentValuesFrom(addInfoChild, new BusinessObjectCloneArgs(new[] { copyAddInfoSupporter.ChildForeignKeyColumn.Name }));
				}
			}
		}

		public bool SupportsClone()
		{
			return SupportsCloneCore();
		}

		protected virtual bool SupportsCloneCore()
		{
			return false;
		}

		IDisposable MarkAsBeingCloned()
		{
			isBeingClonedIndex++;

			return new DisposableAction(() => isBeingClonedIndex--);
		}

		protected bool IsBeingCloned => isBeingClonedIndex > 0;

		int isBeingClonedIndex;

		#endregion

		#region Data Import

		public virtual void PrepareDataImport()
		{
		}

		#endregion

		#region Load & Save

		/// <summary>
		/// Creates new BusinessObjectFactory appropriate to use with this Type.
		/// </summary>
		/// <remarks>
		/// This dynamic factory resolution takes in account any special
		/// factory requirements defined for this instance.
		/// </remarks>
		/// <returns>An instance of <see cref="BusinessObjectFactory"/>.</returns>
		/// <example>
		/// Document processing has special processing factory (inherited from <see cref="BusinessObjectFactory"/>
		/// which in its turn creates factory based on property value
		/// <code>
		///		return new DocumentFactory().GetFactory(this.ParentMain.SM_DB);
		///	</code>
		/// </example>
		public virtual BusinessObjectFactory CreateNewFactory()
		{
			return new BusinessObjectFactory();
		}

		/// <summary>
		/// Align values of transient properties with the one in a given copy.
		/// </summary>
		/// <remarks>
		/// All transient properties in a copy will be set to corresponding values from this instance.
		/// </remarks>
		/// <param name="copy">The copy of current business object.</param>
		public virtual void CopyTransientProperties(BusinessObject copy)
		{
		}

		/// <summary>
		/// Align values of properties stored in XML columns with those in a given copy. This includes:
		/// - Properties whose values will be copied are those with a type implementing IZType and have the XmlColumnPropertyAttribute defined.
		/// - Properties implementing IBusiness, Including NonPersistentBusinessObjectCollection's that have the XmlColumnPropertyAttribute defined, and any XML properties they have, recursively.
		/// </summary>
		/// <param name="copy">The copy of the current business object.</param>
		public void CopyXmlColumns(BusinessObject copy)
		{
			if (HasChanges)
			{
				BusinessObjectXmlHelper.SerialiseXmlColumns(this);
			}

			foreach (var xmlSerialisedColumn in XmlSerialisedColumns)
			{
				var newValue = BusinessObjectXmlHelper.GetXmlColumnPropertyValue(this, xmlSerialisedColumn);
				BusinessObjectXmlHelper.SetXmlColumnPropertyValue(copy, xmlSerialisedColumn, newValue);
			}

			BusinessObjectXmlHelper.DeserialiseXmlColumns(copy);
		}

		public virtual bool IsNotExcludedFromSavingByFactory
		{
			get
			{
				bool canBeSavedOrServiceNotSet = true;
				var isSavedByFactoryService = Factory.ServiceContainer.GetService<IBOIsSavedByFactoryService>();
				if (isSavedByFactoryService != null)
				{
					canBeSavedOrServiceNotSet = isSavedByFactoryService.IsBOSavedByFactory(this);
				}

				return canBeSavedOrServiceNotSet;
			}
		}

		public virtual bool IsSavedByFactory
		{
			get
			{
				return !IsNull && IsNotExcludedFromSavingByFactory && (HasChangesNotIncludingChildren || !IsInDatabase || (!IsDeleted && this is ILightValidationInternals && ((ILightValidationInternals)this).IsValidHasChanges));
			}
		}

		public virtual bool HasChangesInAuditDetails
		{
			get { return false; }
		}

		/// <summary>
		/// Called once, after the object has been constructed by the BusinessObjectFactory
		/// and all constructors have run.
		/// </summary>
		internal void OnInitialized()
		{
			foreach (var hook in CachedExtensionHooks.OnInitializedHooks)
			{
				hook.OnBusinessObjectInitialized(this);
			}
		}

		internal void OnLoadedInternal()
		{
			using (GetValidationSuspender())
			{
				using (PerformanceStatisticsCollector.StartMonitoring("BusinessObject.OnLoaded", GetType()))
				{
					OnLoaded();
				}
				Factory.InvalidateCachedProperties();
			}
		}

		/// <summary>
		/// Called once the BusinessObject is loaded from the factory.
		/// </summary>
		public virtual void OnLoaded()
		{
			var hasXMLColumns = XmlSerialisedColumns.Any();
			var supporter = this as IAddInfoWithSyncPropertySupporter;
			if (hasXMLColumns || supporter != null)
			{
				using (SuspendSettingHasChanges())
				{
					using (GetValidationSuspender())
					{
						if (hasXMLColumns)
						{
							BusinessObjectXmlHelper.DeserialiseXmlColumns(this);
						}
						supporter?.AddInfo?.EnableSynchronization();
					}
				}
			}
		}

		internal void OnFactorySavingInternal()
		{
			using (PerformanceStatisticsCollector.StartMonitoring("BusinessObject.OnFactorySaving", GetType()))
			{
				OnFactorySaving();
				UpdateLightValidationPersistentPropertyIfRequired();
			}
		}

		public bool CanContinueWithSave
		{
			get { return CanContinueWithSaveCore; }
		}

		protected virtual bool CanContinueWithSaveCore
		{
			get { return true; }
		}

		protected internal void OnFactorySavingBeforeTransaction()
		{
			isSavingStarted = true;
			if (restoreSuspendedHasChanges)
			{
				HasChanges = true;
				restoreSuspendedHasChanges = false;
			}

			OnFactorySavingBeforeTransactionCore();
		}

		protected virtual void OnFactorySavingBeforeTransactionCore()
		{
		}

		/// <summary>
		/// Called once the save transaction starts, but before saving data.
		/// Called before OnSaving() on all BusinessObjects, no matter if they have changes or
		/// not. There is no deterministic order in which BusinessObject get
		/// OnFactorySaving() called.
		/// </summary>
		protected virtual void OnFactorySaving()
		{
			DoStrategyAction((IBusinessObjectStrategy strategy) => { strategy.OnFactorySaving(this); });

			if (XmlSerialisedColumns.Any() && HasChanges)
			{
				BusinessObjectXmlHelper.SerialiseXmlColumns(this);
			}
		}

		internal bool OnSavingCalledInternal
		{
			get { return GetBool(Flags.OnSavingCalled); }
			set { SetBool(Flags.OnSavingCalled, value); }
		}

		protected virtual bool IsForcedPublish
		{
			get;
		}

		public bool IsForcePublishForNonPersistentBusinessObject
		{
			get
			{
				return IsForcedPublish && !IsSavedByFactory;
			}
		}

		internal bool HasSkippedOnSavingDueToLightValidation { get; set; }

		internal void OnSavingInternal()
		{
			using (PerformanceStatisticsCollector.StartMonitoring("BusinessObject.OnSaving", GetType()))
			{
				if (!OnSavingCalledInternal)
				{
					if (IsDeleted)
					{
						OnSavingCalledInternal = true;
						OnSavingForDelete();
					}
					else if (this is ILightValidationInternals && ((ILightValidationInternals)this).IsValidHasChanges && IsInDatabase && !HasChangesNotIncludingChildren)
					{
						EnsureLightValidationIsIgnoredForConcurrency();
						OnSavingCalledInternal = true; // Do not call OnSaving() just for light validation
						HasSkippedOnSavingDueToLightValidation = true;
					}
					else
					{
						OnSaving();
						if (!OnSavingCalledInternal)
						{
							ErrorReporter.ReportOnce("OnSavingInternal" + GetType().FullName, GetType().FullName + " or one of its parent classes does not call base on OnSaving()");
						}
					}
				}
			}
		}

		protected virtual void OnSavingForDelete()
		{
		}

		/// <summary>
		/// Called once the save transaction starts, but before saving data.
		/// Only called on BusinessObjects that have had changes made to their
		/// data. OnSaving is called on BusinessObjects in the order that they
		/// will be written to the database. Eg, Masters before Dependents.
		/// </summary>
		public virtual void OnSaving()
		{
			EnsureLightValidationIsIgnoredForConcurrency();
			OnSavingCalledInternal = true;
			DoStrategyAction((IBusinessObjectStrategy strategy) => { strategy.OnSaving(this); });
		}

		public virtual void OnSavingInObjectsWithLateChanges()
		{
			DoStrategyAction((IBusinessObjectStrategy strategy) => { strategy.OnSavingInObjectsWithLateChanges(this); });
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual void SetPropertyIfItExistsAndDisableConcurrencyCheckIfPersistent(string propertyName, IZType value, bool onlySetIfEmpty)
		{
			ZPropertyInfo info = ZPropertyInfoHash.GetPropertySafe(propertyName);

			if (info != null && (!onlySetIfEmpty || info.Value.IsEmpty))
			{
				info.Value = value;
				if (info.IsPersistent)
				{
					ConcurrencyInfo.SetConcurrencyPolicy(this.Row, info.Name, ConcurrencyPolicy.Ignore);
				}
			}
		}

		bool inOnSavedCalledInternal;
		internal void OnSavedInternal(bool saveSucceeded)
		{
			using (PerformanceStatisticsCollector.StartMonitoring("BusinessObject.OnSaved", GetType()))
			{
				if (!inOnSavedCalledInternal)
				{
					inOnSavedCalledInternal = true;
					try
					{
						if (!IsDeleted)
						{
							OnSaved(saveSucceeded);
						}
						else
						{
							OnSavedForDeletedObject(saveSucceeded);
						}
					}
					finally
					{
						inOnSavedCalledInternal = false;
					}
				}
			}
		}

		protected virtual void OnSavedForDeletedObject(bool saveSucceeded)
		{
		}

		/// <summary>
		/// Called after the business object has been saved.
		/// If save failed, SaveSucceeded is false.
		/// Only called on BusinessObjects that have had changes made to their
		/// data. OnSaved is called on BusinessObjects in the order that they
		/// were written to the database. Eg, Masters before Dependents.
		/// </summary>
		public virtual void OnSaved(bool saveSucceeded)
		{
			DoStrategyAction((IBusinessObjectStrategy strategy) => { strategy.OnSaved(this, saveSucceeded); });
		}

		/// <summary>
		/// Called after the business object factory has been saved.
		/// Called before OnSaved() on all BusinessObjects, no matter if they have changes or
		/// not. There is no deterministic order in which BusinessObject get
		/// OnFactorySaved() called.
		/// If save failed, SaveSucceeded is false.
		/// </summary>
		protected virtual void OnFactorySaved(bool saveSucceeded)
		{
			DoStrategyAction((IBusinessObjectStrategy strategy) => { strategy.OnFactorySaved(this, saveSucceeded); });
		}

		internal void OnFactorySavedInternal(bool saveSucceeded)
		{
			isSavingStarted = false;
			using (PerformanceStatisticsCollector.StartMonitoring("BusinessObject.OnFactorySaved", GetType()))
			{
				if (!IsDeleted && saveSucceeded && IsNotExcludedFromSavingByFactory)
				{
					ClearHasChanges();
					ResetHasChangesFromDeleteInCollections();
				}

				OnFactorySaved(saveSucceeded);
			}
		}

		IEnumerable<IUniqueIndexFailureHandler> IBusinessObjectInternals.UniqueIndexFailureHandlers
		{
			get { return UniqueIndexFailureHandlers; }
		}

		protected internal virtual IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { return Enumerable.Empty<IUniqueIndexFailureHandler>(); }
		}

		#region Lazy-Loading Blobs

		bool BlobFieldsNeedLoading(SchemaColumn schemaColumn)
		{
			return IsInDatabase && !IsDeletingForDataRefresh && Row != null && LazyLoading.LoadRequired(Row[schemaColumn.Name]);
		}

#if DEBUG
		public bool BlobFieldsNeedLoadingExposedForTest(SchemaColumn schemaColumn)
		{
			return BlobFieldsNeedLoading(schemaColumn);
		}
#endif

		protected void EnsureBlobField(SchemaColumn schemaColumn)
		{
			try
			{
				if ((Row is ZDataRow) && ((ZDataRow)Row).HasSource(schemaColumn.Name) && !(Row[schemaColumn.Name] is string))
				{
					if (schemaColumn.IsBinary)
					{
						using (var source = ((ZDataRow)Row).GetStreamSource(schemaColumn.Name).GetStream())
						using (var buffer = new MemoryStream())
						{
							source.CopyTo(buffer);
							Row[schemaColumn.Name] = buffer.ToArray();
						}
					}
					else
					{
						using (var reader = ((ZDataRow)Row).GetReaderSource(schemaColumn.Name).GetReader())
						{
							Row[schemaColumn.Name] = reader.ReadToEnd();
						}
					}
				}
				else if (BlobFieldsNeedLoading(schemaColumn))
				{
					Factory.LoadBlobField(this, schemaColumn);
				}
			}
			catch (DeletedRowInaccessibleException)
			{
				// Will be handled later on attempt to access value from this column
			}
			catch (RowNotInTableException)
			{
				// Will be handled later on attempt to access value from this column
			}
			catch (SqlStreamReaderRowNotFoundException rowNotFoundException)
			{
				throw new ZBlobReadException("Error reading blob field " + schemaColumn.Name, rowNotFoundException);
			}
		}

		protected void SetSource(IStreamSource source, SchemaColumn schemaColumn)
		{
			((ZDataRow)Row).SetReaderSource(source, schemaColumn.Name);
			HasChanges = true;
		}

		protected void SetSource(ITextReaderSource source, SchemaColumn schemaColumn)
		{
			((ZDataRow)Row).SetReaderSource(source, schemaColumn.Name);
			HasChanges = true;
		}

		protected void SetSource(IStreamSource source, string columnName)
		{
			((ZDataRow)Row).SetReaderSource(source, columnName);
			HasChanges = true;
		}

		protected void SetSource(ITextReaderSource source, string columnName)
		{
			((ZDataRow)Row).SetReaderSource(source, columnName);
			HasChanges = true;
		}

		#endregion

		#endregion

		#region Keeping track of state of children

		/// <summary>
		/// Tells this BusinessObject that a BusinessObject/Collection (Child) needs to
		/// be taken into account when calculating this.HasChanges, this.IsPersistent etc.
		/// </summary>
		/// <param name="child"></param>
		/// <remarks>Notification propagation will be enabled in this method overload.</remarks>
		public virtual void RegisterEditableChildObject(IBusiness child)
		{
			if (child != null && !IsRegisteredEditableChildObject(child))
			{
				AddChild(child);
				child.NotifyRegisteredChildEditable();
				child.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(HandleUpdateOfChildHasChanges);
				if (updatedByDataRefreshIncludingChildren != null)
				{
#pragma warning disable
					child.UpdatedByDataRefreshIncludingChildren += HandleChildUpdatedByDataRefreshIncludingChildren;
#pragma warning restore
				}
				child.NotificationsChanged += new EventHandler<NotificationsChangedEventArgs>(HandleChildNotificationChanged);

				IActiveBusinessObjectCollection childCollection = child as IActiveBusinessObjectCollection;
				if ((childCollection == null || childCollection.IsLoaded) && child.HasNotifications())
				{
					OnNotificationsChanged();
				}

				SuspendValidationOnChild(child);
				UpdateChildReadOnlyWhenRegistering(child);

				if (settingHasChangesWithChildrenIndex > 0 && childrenHasChangesSuspenders != null && !childrenHasChangesSuspenders.ContainsKey(child))
				{
					var suspendableChild = child as ICanSuspendSettingHasChanges;
					if (suspendableChild != null)
					{
						childrenHasChangesSuspenders.Add(child, suspendableChild.SuspendSettingHasChanges());
					}
				}

				if ((childCollection == null || childCollection.IsLoaded) && child.HasChanges)
				{
					var factoryInternals = Factory as IBusinessObjectFactoryInternals;
					var sureHasChanges = factoryInternals != null && !factoryInternals.IsProcessingOnAllTransactionsCommitted;

					OnHasChangesChanged(HasChangesChangedEventArgs.Create(sureHasChanges, this, true));
				}
			}
		}

		protected virtual void UpdateChildReadOnlyWhenRegistering(IBusiness child)
		{
			if (readOnlyIndex > 0)
			{
				using (SuspendSettingHasChanges())
				{
					child.IncrementReadOnlyIncludingChildren();
				}
			}
		}

		protected void SuspendValidationOnChild(IBusiness child)
		{
			for (int i = 0; i < validationIndex; i++)
			{
				child.SuspendValidation();
			}
		}

		protected void ResumeValidationOnChild(IBusiness child)
		{
			for (int i = 0; i < validationIndex; i++)
			{
				child.ResumeValidation();
			}
		}

		/// <summary>
		/// Tells this BusinessObject that a BusinessObject/Collection (Child) need no longer
		/// be taken into account when calculating this.HasChanges, this.IsPersistent etc.
		/// </summary>
		/// <param name="child"></param>
		public virtual void UnRegisterEditableChildObject(IBusiness child)
		{
			if (IsRegisteredEditableChildObject(child))
			{
				child.HasChangesChanged -= new EventHandler<HasChangesChangedEventArgs>(HandleUpdateOfChildHasChanges);
				child.NotificationsChanged -= new EventHandler<NotificationsChangedEventArgs>(HandleChildNotificationChanged);
				if (updatedByDataRefreshIncludingChildren != null)
				{
#pragma warning disable
					child.UpdatedByDataRefreshIncludingChildren -= HandleChildUpdatedByDataRefreshIncludingChildren;
#pragma warning restore
				}
				RemoveChild(child);
				if (child.HasNotifications())
				{
					OnNotificationsChanged();
				}

				if (IsValidationSuspended)
				{
					ResumeValidationOnChild(child);
				}
				if (readOnlyIndex > 0)
				{
					child.DecrementReadOnlyIncludingChildren();
				}

				IDisposable childHasChangesSuspender;
				if (childrenHasChangesSuspenders != null && childrenHasChangesSuspenders.TryGetValue(child, out childHasChangesSuspender))
				{
					childrenHasChangesSuspenders.Remove(child);
					childHasChangesSuspender.Dispose();
				}

				OnHasChangesChanged(HasChangesChangedEventArgs.Create(false, this, true));
			}
		}

		public ZBool IsRegisteredEditableChildObject(IBusiness child)
		{
			return children != null && ContainsChild(child);
		}

		#endregion

		#region FromUniversalCopy

		public bool IsFromUniversalCopy
		{
			get
			{
				return GetBool(Flags.FromUniversalCopy);
			}
			set
			{
				SetBool(Flags.FromUniversalCopy, value);
			}
		}

		#endregion

		#region HasChanges

		/// <summary>
		/// Returns true if any state or data of the business object has been modified (or the entire business object has been deleted)
		/// </summary>
		public virtual bool HasChanges
		{
			get { return Factory != null && Factory.IsBusinessObjectHasChangesCached ? Factory.GetHasChangesFromCached(this) : GetHasChangesCore(); }
			set
			{
				if (value)
				{
					InvalidateCachedProperties();
				}

				if (!IsSettingHasChangesSuspended)
				{
					using (SuspendSettingHasChanges())
					{
						var oldValue = GetBool(Flags.HasChanges);
						SetBool(Flags.HasChanges, value);

						if (Factory != null && Factory.IsBusinessObjectHasChangesCached && oldValue != value)
						{
							Factory.InvalidateHasChangesCache();
						}

						if (!value)
						{
							ResetHasChangesFromDeleteInCollections();
						}

						OnHasChangesChanged(HasChangesChangedEventArgs.Create(value, this));

						if (Factory != null)
						{
							if (value)
							{
								IncrementChangeNumber();
							}

							if (SingleObjectAroundARow.HasAttribute(GetType()))
							{
								Factory.UpdateHasChangesOnOtherBusinessObjects(this);
							}
							else if (Row != null)
							{
								Factory.UpdateHasChangesOnOtherBusinessObjectsAroundThisRow(Row, value);
							}
						}
					}
				}
				else if (value && KeepValidationAndSavingWithSuspendedSettingHasChanges)
				{
					restoreSuspendedHasChanges = true;
					if (!IsDeleted)
					{
						MarkAsNeedingValidation();
					}
				}
			}
		}

		protected virtual void InvalidateCachedProperties()
		{
			if (Factory != null)
			{
				Factory.InvalidateCachedProperties();
			}
		}

		public bool IsRowChanged
		{
			get
			{
				return Row != null && Row.RowState != DataRowState.Unchanged;
			}
		}

		internal bool GetHasChangesCore()
		{
			return GetBool(Flags.HasChanges) || DoChildrenHaveChanges();
		}

		protected bool HasChangesNotIncludingChildren
		{
			get { return GetBool(Flags.HasChanges); }
		}

		bool IBusinessObjectState.HasChangesNotIncludingChildren
		{
			get { return HasChangesNotIncludingChildren; }
		}

		void ResetHasChangesFromDeleteInCollections()
		{
			foreach (IBusiness child in GetChildren())
			{
				if (child is IBusinessObjectCollection && ((IBusinessObjectCollectionInternals)child).HasChangesFromDelete)
				{
					((IBusinessObjectCollectionInternals)child).HasChangesFromDelete = false;
				}
			}
		}

		public event EventHandler<HasChangesChangedEventArgs> HasChangesChanged;

		void HandleUpdateOfChildHasChanges(object sender, HasChangesChangedEventArgs e)
		{
			if (!IsSettingHasChangesSuspended)
			{
				OnHasChangesChanged(e);
			}
		}

		void OnHasChangesChanged(HasChangesChangedEventArgs e)
		{
			HasChangesChanged?.Invoke(this, e);
		}

		int inDoChildrenHaveChanges;

		protected bool DoChildrenHaveChanges()
		{
			if (inDoChildrenHaveChanges < 2)
			{
				inDoChildrenHaveChanges++;
				try
				{
					foreach (IBusiness child in GetChildren())
					{
						if (child.HasChanges)
						{
							return true;
						}
					}
				}
				finally
				{
					inDoChildrenHaveChanges--;
				}
			}
			else
			{
				ErrorReporter.ReportOnce("CyclicReferenceInChildren", GenerateCyclicReferenceReport());
			}

			return false;
		}

		#endregion

		#region IsInDatabase

		/// <summary>
		/// Does the business object's record and all its child records exist in the database?
		/// </summary>
		public virtual bool IsInDatabaseIncludingChildren
		{
			get
			{
				return IsInDatabase && GetChildren().Where(child => !(child is BusinessObject childBizo) || childBizo.ShouldCheckIsInDatabase).All(child => child.IsInDatabaseIncludingChildren);
			}
		}

		public bool ShouldCheckIsInDatabase
		{
			get
			{
				return ShouldCheckIsInDatabaseCore;
			}
		}

		protected virtual bool ShouldCheckIsInDatabaseCore
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Does the business object's record exist in the database?
		/// </summary>
		public virtual bool IsInDatabase
		{
			get
			{
				return Row != null && Row.HasVersion(DataRowVersion.Original);
			}
		}

		#endregion

		#region ReadOnly

		/// <summary>
		/// Is the business object read-only? Children will *not* inherit this value.
		/// Note that read-only means the GUI becomes read-only; property values can still be modified.
		/// </summary>
		public virtual bool ReadOnly
		{
			get { return (IsReadOnly ?? readOnlyIndex > 0) || IsNull || (Factory != null && ((IBusinessObjectFactoryInternals)Factory).ReadOnly); }
			set
			{
				if ((IsReadOnly == null || IsReadOnly != value) && !IsDeleted)
				{
					bool oldValue = ReadOnly;
					IsReadOnly = value;
					if (ReadOnly != oldValue)
					{
						OnElementChanged();
					}
				}
			}
		}

		/// <summary>
		/// Set the business object and all its children's ReadOnly properties to the specified value.
		/// Note that read-only means the GUI becomes read-only; property values can still be modified.
		/// </summary>
		public void SetReadOnlyIncludingChildren(bool readOnly)
		{
			if (readOnly)
			{
				IncrementReadOnlyIncludingChildren();
			}
			else
			{
				DecrementReadOnlyIncludingChildren(true);
			}
		}

		void IBusinessObjectState.IncrementReadOnlyIncludingChildren()
		{
			IncrementReadOnlyIncludingChildren();
		}

		void IncrementReadOnlyIncludingChildren()
		{
			// required to honor overrides of the ReadOnly property setter
			ReadOnly = true;
			readOnlyIndex++;
			foreach (IBusiness child in GetChildren())
			{
				if (child != null)
				{
					var bizoChild = child as BusinessObject;
					if (bizoChild == null || !bizoChild.IsDeleted)
					{
						UpdateChildReadOnlyWhenRegistering(child);
					}
				}
			}

			IsReadOnly = null; // the ReadOnly property will still remain true because readOnlyIndex > 0
		}

		void IBusinessObjectState.DecrementReadOnlyIncludingChildren(bool decrementToZero)
		{
			DecrementReadOnlyIncludingChildren(decrementToZero);
		}

		void DecrementReadOnlyIncludingChildren(bool decrementToZero)
		{
			foreach (IBusiness child in GetChildren())
			{
				if (child != null)
				{
					var bizoChild = child as BusinessObject;
					if (bizoChild == null || !bizoChild.IsDeleted)
					{
						child.DecrementReadOnlyIncludingChildren(decrementToZero);
					}
				}
			}
			if (readOnlyIndex == 1)
			{
				// required to honor the setter for the ReadOnly property
				ReadOnly = false;
				IsReadOnly = null; // the ReadOnly property will still remain FALSE because readOnlyIndex == 0
			}
			if (readOnlyIndex > 0)
			{
				readOnlyIndex--;
				if (decrementToZero)
				{
					DecrementReadOnlyIncludingChildren(true);
				}
			}
		}

		bool? IsReadOnly
		{
			get
			{
				return GetBool(Flags.ReadOnlySet)
						? GetBool(Flags.ReadOnly)
						: null;
			}
			set
			{
				if (value == null)
				{
					SetBool(Flags.ReadOnlySet, false);
				}
				else
				{
					SetBool(Flags.ReadOnlySet, true);
					SetBool(Flags.ReadOnly, value.Value);
				}
			}
		}

		#endregion

		#region Delete

		/// <summary>
		/// Runs all delete checker plug-ins for this BusinessObject. Throws an exception if delete is not possible.
		/// </summary>
		public void RunDeleteCheckers()
		{
			if (!IsDeleted)
			{
				StringBuilder cannotDeleteReasons = new StringBuilder();
				foreach (IBusinessObjectStrategy checker in Strategies)
				{
					DeleteDetails details = checker.DeleteDetails(this);
					if (details != null && !details.CanDelete)
					{
						cannotDeleteReasons.AppendLine(details.Reason);
					}
				}
				if (cannotDeleteReasons.Length > 0)
				{
					throw new CannotDeleteException(cannotDeleteReasons.ToString());
				}
			}
		}

		protected virtual void BeforeSuccessfulDelete()
		{
			DoStrategyAction((IBusinessObjectStrategy strategy) => { strategy.BeforeSuccessfulDelete(this); });
		}

		/// <summary>
		/// Delete a business object that isn't in the database yet in a very fast (and possibly unsafe) way.
		/// </summary>
		public void Exile()
		{
			if (IsInDatabase)
			{
				Delete();
				return;
			}
			try
			{
				if (!IsDeleting && !WasDeleted && Factory != null)
				{
					IsDeleting = true;
					foreach (BusinessObjectCollection parentCollection in ParentCollections.ToList())
					{
						parentCollection.Remove(this);
					}
					DeleteRow();
					foreach (var child in children)
					{
						if (child is BusinessObject bizoChild)
						{
							bizoChild.Exile();
						}
						else if (child is IEnumerable<BusinessObject> bocChild) //should cover BOC, ABOC and anything else similar
						{
							foreach (var childchild in bocChild.ToArray())
							{
								childchild.Exile();
							}
						}
					}
				}
			}
			finally
			{
				IsDeleting = false;
			}
		}

		/// <summary>
		/// Delete the business object's row.
		/// Posting a deleted business object will result in the corresponding record's deletion from the DataBase.
		/// </summary>
		public virtual void Delete()
		{
			var addInfoChild = (this as IAddInfoChildSupporter)?.AddInfoChild;

			Delete(false);

			addInfoChild?.Delete();
		}

		protected void Delete(bool isDeletingForDataRefresh)
		{
			var wasDeletingForDataRefresh = IsDeletingForDataRefresh;
			IsDeletingForDataRefresh = isDeletingForDataRefresh;

			try
			{
				if (IsNull)
				{
					ErrorReporter.ReportOnce("DeletingNullObj", "Null object cannot be deleted. Type is: " + GetType().FullName);
				}
				else if (!IsDeleting && !WasDeleted && Factory != null)
				{
					using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
					{
						IsDeleting = true;
						try
						{
							if (!IsRefreshingByDataRefreshBus)
							{
								RunDeleteCheckers();
							}

							foreach (var parentCollection in ParentCollections.ToList())
							{
								if (((IList)ParentCollections).Contains(parentCollection))
								{
									if (!IsDeletingForDataRefresh && (HasChanges || IsInDatabase))
									{
										((IBusinessObjectCollectionInternals)parentCollection).HasChangesFromDelete = true;
									}

									if (!((IBusinessObjectCollectionInternals)parentCollection).MastersAreDeleted)
									{
										if (parentCollection.Contains(this))
										{
											if (IsDeletingForDataRefresh)
											{
												parentCollection.RemoveForDataRefresh(this);
											}
											else
											{
												parentCollection.Remove(this);
											}
										}

										if (IsDeletingForDataRefresh && parentCollection is IDependentBusinessObjectCollection parentDependentCollection)
										{
											parentDependentCollection.Master.RaiseUpdatedByDataRefreshIncludingChildren();
										}
									}
								}
							}

							foreach (IBusinessObjectStrategy boStrategy in Strategies)
							{
								boStrategy.OnDelete(this);
							}

							EnsureLightValidationIsIgnoredForConcurrency();

							if (!IsDeletingForDataRefresh)
							{
								BeforeSuccessfulDelete();
							}

							EnsureBlobFieldForConcurrencyCheck();
							DeleteRow();
							WasDeleted = true;
							HasChanges = true;

							if (Factory != null && Row != null)
							{
								Factory.NotifyDeleted(this);
							}
						}
						finally
						{
							IsDeleting = false;
						}
					}

					OnNotificationsChanged(false);
				}
			}
			finally
			{
				IsDeletingForDataRefresh = wasDeletingForDataRefresh;
			}
		}

		public virtual bool IsDeleted => IsDataRowDeleted;

		internal bool IsDataRowDeleted
		{
			get { return WasDeleted || (IsDataRowInDataTable && !IsDataInRowAccessibleForDelete); }
		}

		protected virtual bool IsDataInRowAccessibleForDelete
		{
			get { return ZDataUtils.IsDataInRowAccessible(Row); }
		}

		internal void OnSaveRollbackInternal()
		{
			if (WasDeleted)
			{
				WasDeleted = false;
			}

			HasChanges = false;
		}

		protected internal virtual void OnSaveRollback()
		{
			DoStrategyAction((IBusinessObjectStrategy strategy) => { strategy.OnSaveRollback(this); });
		}

		void IBusinessObjectInternals.MarkAsDeleted()
		{
			WasDeleted = true;
		}

		#endregion

		#region Reload & Refresh

		public void Refresh()
		{
			ClearAllNotifications();
			RefreshBinding();
		}

		/// <summary>
		/// Reloads the data in an existing BusinessObject, if it is in the database.
		/// Throws an exception if it is not in the database
		/// </summary>
		public void Reload()
		{
			Reload(true);
		}

		/// <summary>
		/// Reloads the data in an existing BusinessObject, if it is in the database.
		/// Does not throw an exception if it is not in the database
		/// </summary>
		public void ReloadSafe()
		{
			Reload(false);
		}

		void Reload(bool throwExceptionIfNotInDatabase)
		{
			using (PerformanceStatisticsCollector.StartMonitoring("BusinessObject.Reload", GetType()))
			{
#if DEBUG
				IsReloading = true;
				try
				{
#endif
					if (this is IBusinessObjectReload reloader)
					{
						reloader.Reload(Factory);
					}
					else
					{
						Factory.Reload(this, throwExceptionIfNotInDatabase);
					}
					ReloadCore();
					OnReloaded();
					HasChanges = false;
					Refresh();
#if DEBUG
				}
				finally
				{
					IsReloading = false;
				}
#endif
			}
		}

		public event EventHandler Reloaded;

		void OnReloaded()
		{
			Reloaded?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void ReloadCore()
		{
		}

		#endregion

		#region Getting / Setting Property Value

		#region Non-Persistent Property Value

		protected bool SetNonPersistentPropertyValue<T>(ZPropertyInfo info, ref T? nonPersistentValue, T value)
			where T : struct, IZType
		{
			return SetNonPersistentPropertyValue(info, ref nonPersistentValue, value, true);
		}

		protected bool SetNonPersistentPropertyValue<T>(ZPropertyInfo info, ref T nonPersistentValue, T value)
			where T : IZType
		{
			return SetNonPersistentPropertyValue(info, ref nonPersistentValue, value, true);
		}

		protected bool SetNonPersistentPropertyValue<T>(ZPropertyInfo info, ref T? nonPersistentValue, T value, bool setValueOnlyIfDifferentToGetter)
			where T : struct, IZType
		{
			return SetNonPersistentPropertyValueCore(info, ref nonPersistentValue, value, setValueOnlyIfDifferentToGetter);
		}

		protected bool SetNonPersistentPropertyValue<T>(ZPropertyInfo info, ref T nonPersistentValue, T value, bool setValueOnlyIfDifferentToGetter)
			where T : IZType
		{
			return SetNonPersistentPropertyValueCore(info, ref nonPersistentValue, value, setValueOnlyIfDifferentToGetter);
		}

		bool SetNonPersistentPropertyValueCore<TFieldToSet, TIZType>(ZPropertyInfo info, ref TFieldToSet nonPersistentValue, TIZType value, bool setValueOnlyIfDifferentToGetter)
			where TIZType : IZType
		{
			IZType oldValue;
			IZType newValue;

			if (ShouldSetNonPersistentPropertyValue(info, value, setValueOnlyIfDifferentToGetter, out oldValue, out newValue))
			{
				nonPersistentValue = (TFieldToSet)newValue;
				AfterNonPersistentPropertyValueSet(info, oldValue);

				return true;
			}

			return false;
		}

		bool ShouldSetNonPersistentPropertyValue<TIZType>(ZPropertyInfo info, TIZType value, bool setValueOnlyIfDifferentToGetter, out IZType oldValue, out IZType newValue)
			where TIZType : IZType
		{
			newValue = value;
			oldValue = info.Value;

			if (newValue is ZString)
			{
				var stringValue = ((ZString)newValue).TrimEnd(' ');
				CheckMaximumLength(info, stringValue);
				newValue = stringValue;
			}

			return !setValueOnlyIfDifferentToGetter || PropertyValueHasChanged(info, newValue);
		}

		void AfterNonPersistentPropertyValueSet(ZPropertyInfo info, IZType oldValue)
		{
			HasChanges = true;
			PropertyValueSet(info, oldValue);
		}

		#endregion

		public event ZPropertyValueChangedEventHandler PropertyValueChanged;

		protected bool SetPropertyValue(ZPropertyInfo info, IZType value)
		{
			return SetPropertyValue(info, value, true);
		}

		internal bool SetPropertyValue(ZPropertyInfo info, IZType value, bool setValueOnlyIfDifferentToGetter)
		{
			bool result = false;
			if (IsPropertySupported(info))
			{
				IZType newValue = value;
				if (newValue is ZString)
				{
					var maxLength = info.MaxLength;
					if (maxLength > -1)
					{
						newValue = ((ZString)newValue).Left(maxLength);
					}
				}
				else if (newValue is ZDecimal)
				{
					var schemaColumn = (SchemaDecimalColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(info.Name, info.BizObj.TableName);
					if (schemaColumn != null)
					{
						ZDecimal v = (ZDecimal)newValue;
						if (v.DecimalPlaces > schemaColumn.Scale)
						{
							newValue = v.Round(schemaColumn.Scale);
						}
					}
				}

				if (!setValueOnlyIfDifferentToGetter || PropertyValueHasChanged(info, newValue))
				{
					IZType oldValue = info.Value;

					if (!IsFactoryReadOnlyOrNull())
					{
						using (HandleClusterKeyChange(info, oldValue, value))
						using (Factory == null ? DisposableAction.NoAction : ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
						{
							object valueForDataLayer = ((IZTypeInternals)newValue).GetValueForLogicalDataLayer(info.IsNullable);
							if (Factory != null)
							{
								valueForDataLayer = Factory.InternValue(info, valueForDataLayer);
							}
							Row[info.Name] = valueForDataLayer;
							HasChanges = result = true;
						}

						PropertyValueSet(info, oldValue);
					}
				}
			}

			return result;
		}

		void PropertyValueSet(ZPropertyInfo info, IZType oldValue)
		{
			PropertyChangeSubscription.NotifyPropertyChanged(info, oldValue);
			info.RefreshBinding(oldValue);
			if (!IsCopying)
			{
				IValueSetStrategy valueSetStrategy = GetValueSetStrategy();
				if (valueSetStrategy != null)
				{
					valueSetStrategy.ValueSet(info, oldValue);
				}
			}

			PropertyValueChanged?.Invoke(this, new ZPropertyValueChangedEventArgs(info, oldValue));
		}

		protected virtual IValueSetStrategy GetValueSetStrategy()
		{
			return null;
		}

		public virtual bool IsPropertySupported(ZPropertyInfo propertyInfo)
		{
			return true;
		}

		public bool IsValidationEnabled(ZPropertyInfo propertyInfo)
		{
			return IsValidationEnabledCore(propertyInfo);
		}

		protected virtual bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return true;
		}

		protected bool SetPropertyValue(ZPropertyInfo info, SQLComparisonOperator @operator)
		{
			if (IsPropertySupported(info) && !IsFactoryReadOnlyOrNull() && ((SQLComparisonOperator)Row[info.Name]) != @operator)
			{
				Row[info.Name] = @operator;
				HasChanges = true;
				info.RefreshBinding();
				return true;
			}

			return false;
		}

		bool IsFactoryReadOnlyOrNull()
		{
			return Factory == null || ((IBusinessObjectFactoryInternals)Factory).ReadOnly;
		}

		bool PropertyValueHasChanged(ZPropertyInfo info, IZType value)
		{
			var result = GetCurrentValueToCompareToForChanges(info);
			var valueToCompare = value;

			var isOffsetWithChangedTimezone = false;

			if (valueToCompare is ZDateTime && valueToCompare.IsValid && info.IsDbColumnSmallDateTime)
			{
				var dateTimeValue = (ZDateTime)valueToCompare;
				var resultAsDateTime = (ZDateTime)result;
				var shouldCompareToSmallDateTime = resultAsDateTime.IsValid && resultAsDateTime.IsValidSmallDateTime && IsInDatabase && resultAsDateTime.Second == 0 && resultAsDateTime.Millisecond == 0;
				result = shouldCompareToSmallDateTime ? resultAsDateTime.ToSmallDateTimeFloor() : result;
				valueToCompare = shouldCompareToSmallDateTime ? dateTimeValue.ToSmallDateTimeFloor() : valueToCompare;
			}
			else if (valueToCompare is ZDateTimeOffset && valueToCompare.IsValid && result.IsValid)
			{
				var dateTimeOffsetValue = (ZDateTimeOffset)valueToCompare;
				var resultAsDateTimeOffset = (ZDateTimeOffset)result;

				isOffsetWithChangedTimezone = dateTimeOffsetValue.Offset != resultAsDateTimeOffset.Offset;
			}

			return result == null
				? value != null
				: !result.Equals(valueToCompare) || isOffsetWithChangedTimezone;
		}

		protected virtual IZType GetCurrentValueToCompareToForChanges(ZPropertyInfo info)
		{
			return info.Value;
		}

		public static void CheckMaximumLength(ZPropertyInfo info, ZString value)
		{
			var infoMaxLength = info.MaxLength;
			if (infoMaxLength > -1 && value.Length > infoMaxLength)
			{
				var description = GetMaximumLengthErrorDescription(info.Name, info.MaxLength, value, info.Value.ToString());
				if (throwWhenMaxPropertyLengthExceeded > 0)
				{
					throw new MaxLengthExceededException(description, infoMaxLength);
				}
				else
				{
					ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "BusinessObject.CheckMaximumLength ['{0}'] ['{1}']", info.Name, GetMethodNameCallingZStringSetter()), description);
				}
#if DEBUG
				throw new MaxLengthExceededException(description, infoMaxLength);
#endif
			}
		}

		public static IDisposable ThrowWhenMaxPropertyLengthExceeded()
		{
			throwWhenMaxPropertyLengthExceeded++;
			return new DisposableAction(() => throwWhenMaxPropertyLengthExceeded--);
		}

		[ThreadStatic]
		static int throwWhenMaxPropertyLengthExceeded;

		public static string GetMaximumLengthErrorDescription(string name, int maxLength, string newValue, string oldValue)
		{
			return FormattableString.Invariant(
				$"The maximum length of '{name}' has been exceeded.\n The maximum length of this property is {maxLength} characters, but {newValue.Length} were entered. New value: {AbridgeErrorMessageIfTooLong(newValue)}. Old value: {oldValue}");
		}

		static string AbridgeErrorMessageIfTooLong(ZString message)
		{
			if (message.Length <= 6000)
			{
				return message;
			}
			else
			{
				return string.Format(CultureInfo.InvariantCulture, (NoResString)"{0}\r\n... content removed for the sake of brevity ...\r\n{1}", message.SubstringSafe(0, 4000), message.SubstringSafe(message.Length - 1000));
			}
		}

		static string GetMethodNameCallingZStringSetter()
		{
			var checkMaxMethodFound = false;

			foreach (var line in Environment.StackTrace.SplitByLine())
			{
				if (checkMaxMethodFound && !line.Contains(nameof(ZString)) && !line.Contains("set_"))
				{
					return line.Substring(0, line.IndexOf("(", StringComparison.OrdinalIgnoreCase)).Trim();
				}

				if (line.Contains(nameof(CheckMaximumLength)))
				{
					checkMaxMethodFound = true;
				}
			}

			return ZString.Empty;
		}

		#endregion

		#region Concurrency Handling Policy

		/// <summary>
		/// Sets the concurrency policy on all persistent properties on this BusinessObject.
		/// </summary>
		public void SetConcurrencyPolicyOnProperties(ConcurrencyPolicy concurrencyPolicy)
		{
			foreach (ZPropertyInfo info in ZPropertyInfoHash)
			{
				if (info.IsPersistent)
				{
					ConcurrencyInfo.SetConcurrencyPolicy(this.Row, info.Name, concurrencyPolicy);
				}
			}
		}

		public void SetConcurrencyPolicyOnTable(ConcurrencyPolicy concurrencyPolicy)
		{
			Table.ExtendedProperties[typeof(ConcurrencyPolicy)] = concurrencyPolicy;
		}

		public void OnConcurrencyException(IEnumerable<IPropertyRecord> propertyRecords)
		{
			OnConcurrencyExceptionCore(propertyRecords);
		}

		protected virtual void OnConcurrencyExceptionCore(IEnumerable<IPropertyRecord> propertyRecords)
		{
		}

		public void OnConcurrencyExceptionAfterMerge(IEnumerable<IPropertyRecord> propertyRecords)
		{
			OnConcurrencyExceptionAfterMergeCore(propertyRecords);
		}

		protected virtual void OnConcurrencyExceptionAfterMergeCore(IEnumerable<IPropertyRecord> propertyRecords)
		{
		}

		#endregion

		#region MatchesFilter

		public bool MatchesFilter(ZQuery filter, string identifier = "")
		{
			return MatchesFilterCore(filter, Row, Table, TableName, identifier);
		}

		protected virtual bool MatchesFilterCore(ZQuery filter, DataRow row, DataTable table, string tableName, string identifier)
		{
			bool result;

			if (filter == null || filter.IsEmpty)
			{
				result = true;
			}
			else if (filter.IsDBOnlyQuery)
			{
				result = MatchesFilterForDBOnlyQuery(filter, tableName);
			}
			else
			{
				result = MatchesFilterForNonDBOnlyQuery(filter, row, table, identifier);
			}

			return result;
		}

		bool MatchesFilterForDBOnlyQuery(ZQuery filter, string tableName)
		{
			bool result;

			var rowFactory = Factory.RowFactory;
			if (rowFactory.IsDbOnlyQueryCached(tableName, filter))
			{
				result = HasRowWithSamePK(rowFactory.GetCachedDbOnlyQueryResult(tableName, filter));
			}
			else
			{
				result = HasRowWithSamePK(rowFactory.GetCachedNarrowDbOnlyQueryResult(tableName, filter));

				if (!result)
				{
					var compositeParts = filter.GetCompositeParts();
					result = compositeParts.Length > 1 && compositeParts.All(compositePart => HasRowWithSamePK(rowFactory.GetCachedNarrowDbOnlyQueryResult(tableName, compositePart)));
				}

				if (!result)
				{
					if (PKSchemaColumn.IsNonPersistent)
					{
						throw new InvalidOperationException("Cannot match db-only filter with business object of type " + GetType().FullName + " with non-persistent PK schema column.");
					}

					try
					{
						var matchFilter = new ZQuery(filter);
						matchFilter.AddToFilter(PKSchemaColumn, PK);

						if (rowFactory.IsDbOnlyQueryCached(tableName, matchFilter))
						{
							result = HasRowWithSamePK(rowFactory.GetCachedDbOnlyQueryResult(tableName, matchFilter));
						}
						else
						{
							result = Factory.LoadTop1(TypeForMatchesFilter, matchFilter) != null;
						}
					}
					catch (SqlException ex) when (ex.Number == 8623)
					{
						result = Factory.Load(TypeForMatchesFilter, filter)?.Any(x => x.PK == PK) ?? false;
					}
				}
			}

			return result;
		}

		bool MatchesFilterForNonDBOnlyQuery(ZQuery filter, DataRow row, DataTable table, string identifier)
		{
			bool result;

			var matchFilter = !filter.IgnoreActiveFilter ? new ZQuery(filter, GetActiveFilter(GetType())) : filter;
			if (matchFilter.IsEmpty)
			{
				result = true;
			}
			else if (IsDeleted || row.RowState == DataRowState.Detached || matchFilter.IsNoResultQuery)
			{
				result = false;
			}
			else
			{
				RowFilterComparer rfc;

				if (Factory == null || DoNotCacheRowFilterComparerAttribute.HasAttribute(GetType()))
				{
					rfc = new RowFilterComparer(table, matchFilter);
				}
				else
				{
					var (key, literalTextAdo) = GetMatchFilterKeyAndLiteralTextADO(matchFilter, identifier);

					if (!key.IsNullOrEmpty())
					{
						var cache = Factory.GetCachedValue(RowFilterComparerCacheKey, () => new LRUCache<string, RowFilterComparer>(RowFilterCacheLimit));
						if (!cache.TryGetValue(key, out rfc))
						{
							rfc = new RowFilterComparer(table, matchFilter, literalTextAdo ?? matchFilter.LiteralTextADO);
							cache.Add(key, rfc);
						}
					}
					else
					{
						rfc = new RowFilterComparer(table, matchFilter, literalTextAdo ?? matchFilter.LiteralTextADO);
					}
				}

				result = rfc.IsMatch(row);
			}

			return result;

			(string Key, string LiteralTextADO) GetMatchFilterKeyAndLiteralTextADO(ZQuery matchFilter, string identifier)
			{
				var key = string.Empty;
				string literalTextAdo = null;

				if (!identifier.IsNullOrEmpty())
				{
					var matchFilterLiteralTextAdoCache = Factory.GetCachedValue(MatchFilterKeyCacheKey, () => new LRUCache<string, (string Key, bool IsQuery)>(MatchFilterKeyCacheLimit));
					if (!matchFilterLiteralTextAdoCache.TryGetValue(identifier, out var result))
					{
						literalTextAdo = matchFilter.LiteralTextADO;

						var useQueryInCache = literalTextAdo.Length < RowFilterCacheMaxKeySize;
						key = useQueryInCache ? literalTextAdo : identifier;

						matchFilterLiteralTextAdoCache.Add(identifier, (key, useQueryInCache));
					}
					else
					{
						key = result.Key;
						literalTextAdo = result.IsQuery ? key : null;
					}
				}
				else
				{
					literalTextAdo = matchFilter.LiteralTextADO;
					key = literalTextAdo.Length < RowFilterCacheMaxKeySize ? literalTextAdo : null;
				}

				return (key, literalTextAdo);
			}
		}

		#region Row Filter Cache Constants

#if DEBUG
		public
#endif
		const string RowFilterComparerCacheKey = "b9551206-788c-4312-82ce-d8fb1b955f4d";
		const int RowFilterCacheMaxKeySize = 4096;
		const int RowFilterCacheLimit = 28;

		const string MatchFilterKeyCacheKey = "BFD69346-5A4F-48BA-9137-1B19612EDBE2";
		const int MatchFilterKeyCacheLimit = 28;

		#endregion

		protected virtual Type TypeForMatchesFilter => GetType();

		bool HasRowWithSamePK(IEnumerable<DataRow> rows)
		{
			return rows != null && rows.Any(row => (Guid)row[PKSchemaColumn.Name] == PK.ToGuid());
		}

		#endregion

		public static class AddInfoConstants
		{
			public const char Separator = '*';
			public const char SpecialCharRepresentingStar = '¤';
			public const char CodeValueSeparator = '=';
		}

		#region IAccessBusinessObject Members

		/// <summary>
		/// The property value for the specified PropertyName.
		/// </summary>
		object IAccessBusinessObject.this[string propertyName]
		{
			get { return this[propertyName]; }
		}

		/// <summary>
		/// Is the property ReadOnly?
		/// </summary>
		/// <param name="propertyName">The name of the property.</param>
		bool IAccessBusinessObject.IsPropertyReadOnly(string propertyName)
		{
			return !IsDeleted && CargoWise.ComponentModel.MetaData.GetReadOnly(this, TypeDescriptor.GetProperties(this)[propertyName]);
		}

		#endregion

		#region IBusinessObjectInternals Members

		internal bool IsInPreSaveValidation
		{
			[DebuggerStepThrough]
			get { return preSaveValidationDepthCount > 0; }
		}

		/// <summary>
		/// By default, BusinessObjects are subscribed to DataRefresh on construction
		/// Override this property to bypass DataRefresh subscription on construction
		/// You can still manually force a DataRefresh publish
		/// </summary>
		bool IBusinessObjectInternals.SubscribeToDataRefreshOnInstantiation
		{
			get { return SubscribeToDataRefreshOnInstantiationCore; }
		}

		protected virtual bool SubscribeToDataRefreshOnInstantiationCore
		{
			get { return true; }
		}

		bool IBusinessObjectInternals.IsInPreSaveValidation
		{
			[DebuggerStepThrough]
			get { return IsInPreSaveValidation; }
		}

		bool IBusinessObjectInternals.IsCopying
		{
			get { return this.IsCopying; }
			set { this.IsCopying = value; }
		}

		public virtual ZPropertyInfoHashtable ZPropertyInfoHash
		{
			[DebuggerStepThrough]
			get { return fZPropertyInfoHash; }
		}

		DataRow IBusinessObjectInternals.Row
		{
			[DebuggerStepThrough]
			get { return Row; }
		}

		bool IBusinessObjectInternals.AcceptChangesDelayedUntilJustBeforeSavingToDatabase
		{
			get { return GetBool(Flags.AcceptChangesDelayed); }
			set { SetBool(Flags.AcceptChangesDelayed, value); }
		}

		object IBusinessObjectInternals.GetValueFromRowSafely(ZPropertyInfo property)
		{
			return ((IBusinessObjectInternals)this).GetValueFromRowSafely(property, DataRowVersion.Default);
		}

		object IBusinessObjectInternals.GetValueFromRowSafely(ZPropertyInfo property, DataRowVersion version)
		{
			DataColumn column = Table.Columns[property.Name];
			return GetValueFromRowSafely(column, version);
		}

		object IBusinessObjectInternals.GetValueFromRowSafely(SchemaColumn schemaColumn, DataRowVersion version)
		{
			DataColumn column = Table.Columns[schemaColumn.Name];
			return GetValueFromRowSafely(column, version);
		}

		object IBusinessObjectInternals.GetValueFromRowSafely(string columnName, DataRowVersion version)
		{
			DataColumn column = Table.Columns[columnName];
			return GetValueFromRowSafely(column, version);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected object GetValueFromRowSafely(SchemaColumn schemaColumn)
		{
			if (schemaColumn == null)
			{
				throw new ArgumentNullException(nameof(schemaColumn));
			}
			if (!(this.Table is ZDataTable zTable))
			{
				return GetValueFromRowSafely(Table.Columns[schemaColumn.Name], DataRowVersion.Default);
			}
			else
			{
				var column = zTable.GetDataColumnFromSchemaColumn(schemaColumn);
				return GetValueFromRowSafely(column, DataRowVersion.Default);
			}
		}

		object IBusinessObjectInternals.GetValueFromRowSafely(DataColumn column, DataRowVersion version)
		{
			return GetValueFromRowSafely(column, version);
		}

		object GetValueFromRowSafely(DataColumn column, DataRowVersion version)
		{
#if DEBUG
			if (Factory != null)
			{
				Factory.OnAccessingPersistentValueForTesting(this, column);
			}
#endif

			object result;
			var versionToUse = (version == DataRowVersion.Original && !Row.HasVersion(DataRowVersion.Original)) ? DataRowVersion.Default : version;

			try
			{
				result = Row[column, versionToUse];
			}
			catch (DeletedRowInaccessibleException ex)
			{
				result = GetColumnOriginalValue(column.ColumnName);
				ReportRowDeletedError(column.ColumnName, ex, versionToUse, version, result);
			}
			catch (RowNotInTableException ex)
			{
				result = GetColumnOriginalValue(column.ColumnName);
				ReportRowDeletedError(column.ColumnName, ex, versionToUse, version, result);
			}
			catch (VersionNotFoundException ex)
			{
				result = GetColumnOriginalValue(column.ColumnName);
				ReportRowDeletedError(column.ColumnName, ex, versionToUse, version, result);
			}
			catch (IndexOutOfRangeException ex)
			{
				result = GetColumnOriginalValue(column.ColumnName);
				ReportRowDeletedError(column.ColumnName, ex, versionToUse, version, result);
			}

			return result;
		}

		object IBusinessObjectInternals.GetColumnOriginalValue(string columnName)
		{
			return GetColumnOriginalValue(columnName);
		}

		object GetColumnOriginalValue(string columnName)
		{
			return Row.HasVersion(DataRowVersion.Original) ? Row[columnName, DataRowVersion.Original] : null;
		}

		IDisposable IBusinessObjectInternals.SuppressReportRowDeletedError()
		{
			suppressReportRowDeletedErrorCounter++;
			return new DisposableAction(() => suppressReportRowDeletedErrorCounter--);
		}

		int suppressReportRowDeletedErrorCounter;

		#region SuppressResourceStringsCheckRegion

		void ReportRowDeletedError(string columnName, Exception ex = null, DataRowVersion versionToUse = DataRowVersion.Default, DataRowVersion version = DataRowVersion.Default, object propertyValue = null)
		{
			ReportRowError(columnName, ex, versionToUse, version, "Developer Error: Should not be accessing a property on a deleted business object", propertyValue);
		}

		void ReportRowError(string columnName, Exception ex, DataRowVersion versionToUse = DataRowVersion.Default, DataRowVersion version = DataRowVersion.Default, string message = "", object propertyValue = null)
		{
			if (!IsDeleting && suppressReportRowDeletedErrorCounter == 0)
			{
				StackTrace trace = new StackTrace();
				if (!IsCalledFromPropertyDescriptor(trace))
				{
					var errorBuilder = BuildRowDeletedReport(columnName, ex, versionToUse, version, message, propertyValue);
					ErrorReporter.ReportOnce(GetReportKeyForRowDeletedError(trace.ToString(), columnName), errorBuilder.ToString(), ex);
				}
			}
		}

		protected StringBuilder BuildRowDeletedReport(string columnName, Exception ex, DataRowVersion versionToUse, DataRowVersion version, string message, object propertyValue)
		{
			var rowDeletedReport = BuildRowDeletedReport(columnName, ex, versionToUse, version, message);
			if (ShouldIncludePropertyValueInRowDeletedError(columnName))
			{
				rowDeletedReport.Append("Property value: ").AppendLine(propertyValue?.ToString() ?? string.Empty);
			}
			return rowDeletedReport;
		}

		protected virtual bool ShouldIncludePropertyValueInRowDeletedError(string propertyName) => false;

		protected virtual StringBuilder BuildRowDeletedReport(string columnName, Exception ex, DataRowVersion versionToUse, DataRowVersion version, string message)
		{
			var suspendedIndex = CollectionListChangedSuspender.GetInstance(Factory)?.DelayListChangedEventsIndex.ToString(CultureInfo.InvariantCulture);
			return new StringBuilder()
				.AppendLine(message)
				.Append("TableName: ").AppendLine(Row.Table != null ? Row.Table.TableName : "<Table is null>")
				.Append("Property name: ").AppendLine(columnName)
				.Append("Business Object Type: ").AppendLine(GetType().FullName)
				.Append("DataRowState: ").AppendLine(Row.RowState.ToString())
				.Append("IsDataRowInDataTable: ").AppendLine(IsDataRowInDataTable.ToString())
				.Append("Version: ").AppendLine(version.ToString())
				.Append("VersionToUse: ").AppendLine(versionToUse.ToString())
				.Append("PK: ").AppendLine(PK.ToString())
				.Append("CollectionListChangedSuspender index on factory: ").AppendLine(suspendedIndex ?? "N/A");
		}

		#endregion

		string GetReportKeyForRowDeletedError(string traceString, string columnName)
		{
			int index = Math.Max(traceString.IndexOf(".get_" + columnName + "()", System.StringComparison.Ordinal),
				Math.Max(traceString.LastIndexOf("GetValueFromRowSafely", System.StringComparison.Ordinal),
					traceString.LastIndexOf("GetPropertyValueByName", System.StringComparison.Ordinal)));

			StringBuilder sb = new StringBuilder(traceString.Length - (index >= 0 ? index : 0) + Row.Table.TableName.Length + (index >= 0 ? 0 : columnName.Length + 1));
			sb.Append(this.GetType().FullName);
			if (index < 0)
			{
				index = 0;
				sb.Append('.');
				sb.Append(columnName);
			}
			sb.Append(traceString.Substring(index));

			sb.Replace(".get_", ".");
			sb.Replace("CargoWise.", "CW.");
			sb.Replace("Enterprise.", "E.");
			sb.Replace(".Business.", ".B.");
			sb.Replace("EntityFramework.", "EF.");
			sb.Replace("ZArchitecture.", "ZA.");
			sb.Replace("<Call>", "");
			sb.Replace("</Call>", "");
			sb.Replace("<Call />", "");
			sb.Replace(" at ", @"");
			sb.Replace("  ", "");

			return sb.ToString(0, Math.Min(125, sb.Length));
		}

		static bool IsCalledFromPropertyDescriptor(StackTrace trace)
		{
			foreach (StackFrame frame in trace.GetFrames())
			{
				var method = frame.GetMethod();
				var type = method.DeclaringType;

				if (type != null && (typeof(PropertyDescriptor).IsAssignableFrom(type) || type.Name.Equals("BusinessObjectLoggingStrategy", StringComparison.InvariantCulture) && method.Name.Equals("UpdateEditAndCreateLogFields", StringComparison.InvariantCulture)))
				{
					return true;
				}
			}

			return false;
		}

		BusinessObjectCollection[] IBusinessObjectInternals.ParentCollections
		{
			get { return ParentCollections.ToArray(); }
		}

		bool IBusinessObjectInternals.DoChildrenHaveChanges()
		{
			return DoChildrenHaveChanges();
		}

		void IBusinessObjectInternals.EnsureBlobField(SchemaColumn schemaColumn)
		{
			EnsureBlobField(schemaColumn);
		}

		#endregion

		#region ILinkable Members

		ZGuid ILinkable.LinkPK
		{
			get { return PK; }
		}

		string ILinkable.LinkTableName
		{
			get { return TableName; }
		}

		string ILinkable.LinkTablePrefix
		{
			get { return TablePrefix; }
		}

		bool ILinkable.LinkIsInDatabase
		{
			get { return IsInDatabase; }
		}

		#endregion

		#region FetchStrategy

		public IBusinessObjectFetchStrategy FetchStrategy => GetFetchStrategy();

		protected virtual IBusinessObjectFetchStrategy GetFetchStrategy()
		{
			return new BusinessObjectFetchStrategy(this);
		}

		#endregion

		#region Validation

		#region Pre-Save Validation

		void IBusiness.RunPreSaveValidationFetch(bool executeHints)
		{
			if (IsDeleted)
			{
				return;
			}

			if (ShouldValidateOnSave)
			{
				FetchStrategy.FetchForValidate();
			}

			int index = 0;

			while (index < GetChildren().Length)
			{
				IBusiness child = GetChild(index);
				child.RunPreSaveValidationFetch(false);
				index++;
			}

			if (executeHints && Factory != null)
			{
				Factory.ExecuteAllFetchHints();
			}
		}

		/// <summary>
		/// Validation of the <see cref="BusinessObject"/> with collecting and executing fetch hints.
		/// </summary>
		public void RunPreSaveValidationWithFetchHints()
		{
			((IBusiness)this).RunPreSaveValidationFetch(false);
			RunPreSaveValidation();
		}

		/// <summary>
		/// Validation to run before saving the BusinessObject.
		/// </summary>
		public void RunPreSaveValidation()
		{
			RunPreSaveValidationInternal(true);
		}

		/// <summary>
		/// Validation to run before saving the BusinessObject, in specialist cases where you want to ignore a Bizo's child shapes. Usually paired with similar calls validating specific children of the Bizo
		/// </summary>
		public void RunPreSaveValidationExcludingChildren()
		{
			RunPreSaveValidationInternal(false);
		}

		internal void RunPreSaveValidationInternal(bool validateChildren)
		{
			if (!IsNull)
			{
				var lastNotificationChangedCount = NotificationChangeCount;
				var shouldValidateOnSave = this.ShouldValidateOnSave;
				var hasBegunValidation = false;
				try
				{
					using (GetValidationDataSuspender())
					{
						Factory?.BeginValidation(this);
						preSaveValidationDepthCount++;
						hasBegunValidation = true;

						if (shouldValidateOnSave)
						{
							RunPreSaveValidationCore();
							if (LightValidationEnabled && !LightValidationIsValid && !HasNotificationsNotIncludingChildren(NotificationType.Error) && !HasNotificationsNotIncludingChildren(NotificationType.MessageError))
							{
								LightValidationIsValid = true;
							}
						}
						else if (HasNotificationsNotIncludingChildren(NotificationType.Error) || HasNotificationsNotIncludingChildren(NotificationType.MessageError))
						{
							ValidatePropertiesWithErrorsOrMessageErrors();
							if (HasNotificationsNotIncludingChildren(NotificationType.Error) || HasNotificationsNotIncludingChildren(NotificationType.MessageError))
							{
								LightValidationIsValid = false;
							}
						}

						if (validateChildren)
						{
							RegisterCustomBusinessObjectAsChild();
							RunPreSaveValidationOnChildren();
						}
					}
				}
				catch (Exception e)
				{
					if (!hasBegunValidation)
					{
						ErrorReporter.ReportOnce("Unexpected exception in RunPreSaveValidationInternal", e);
					}
					throw;
				}
				finally
				{
					if (hasBegunValidation)
					{
						preSaveValidationDepthCount--;
						Factory?.EndValidation(this);
					}

					if (fValidationFromController != null)
					{
						fValidationFromController.ValidateEntityOnSaving(this);
					}

					if (shouldValidateOnSave)
					{
						if (lastNotificationChangedCount != NotificationChangeCount)
						{
							OnNotificationsChanged(true);
						}
					}
				}
			}
		}

		protected virtual void RegisterCustomBusinessObjectAsChild()
		{
		}

		protected virtual IDisposable GetValidationDataSuspender()
		{
			return new DisposableObject();
		}

		// Often not possible to fire appropriate validation from property setters (plugins etc)
		// so we simply validate all infos eith errors/msg errors to see if they can be cleared.
		void ValidatePropertiesWithErrorsOrMessageErrors()
		{
			foreach (ZPropertyInfo info in ZPropertyInfoHash)
			{
				if (info.HasErrors() || info.HasMessageErrors())
				{
					((IBusinessObjectInternals)info.BizObj).Validate(info);
				}
			}
		}

		protected virtual void RunPreSaveValidationCore()
		{
		}

		int lastNotificationChangeCount;
		[SuppressThreadStaticFieldMessage]
		internal static int NotificationChangeCount;

		void RunPreSaveValidationOnChildren()
		{
			try
			{
				lastNotificationChangeCount = NotificationChangeCount;
				preSaveValidationDepthCount++;

				int index = 0;

				while (index < GetChildren().Length)
				{
					IBusiness child = GetChild(index);
					BusinessObject childBizO = child as BusinessObject;
					bool childIsDeleted = (childBizO != null && childBizO.IsDeleted);

					if (!childIsDeleted)
					{
						using (child is BusinessObject bizO ? bizO.TrackRunPreSaveValidationAsChild() : null)
						{
							child.RunPreSaveValidation();
						}
					}

					index++;
				}
			}
			finally
			{
				preSaveValidationDepthCount--;
				if (lastNotificationChangeCount != NotificationChangeCount)
				{
					OnNotificationsChanged(true);
					lastNotificationChangeCount = NotificationChangeCount;
				}
			}
		}

		IDisposable TrackRunPreSaveValidationAsChild()
		{
			IsRunningPreSaveValidationAsChild = true;
			return new DisposableAction(() => IsRunningPreSaveValidationAsChild = false);
		}

		protected bool IsRunningPreSaveValidationAsChild;

		#endregion

		#region Validating when tabbing through fields

		/// <summary>
		/// Validate a property for the specified PropertyName.
		/// </summary>
		/// <param name="propertyName">The name of the property to Validate.</param>
		void IBusinessObjectInternals.Validate(string propertyName)
		{
			if (ZPropertyInfoHash.ContainsKey(propertyName))
			{
				((IBusinessObjectInternals)this).Validate(ZPropertyInfoHash[propertyName]);
			}
		}

		/// <summary>
		/// Validate a property for the specified PropertyName.
		/// </summary>
		/// <param name="info">The ZPropertyInfo of property to Validate.</param>
		void IBusinessObjectInternals.Validate(ZPropertyInfo info)
		{
			var wrappedInfo = info as ZWrappedPropertyInfo;
			var isWrappedInfoForSubProperty = wrappedInfo != null && wrappedInfo.Name.IndexOf('+') > 0;

			var curInfo = info as ZWrappedPropertyInfo;
			while (curInfo?.InnerInfo is ZWrappedPropertyInfo innerInfo)
			{
				curInfo = innerInfo;
			}
			var infoToValidate = curInfo?.InnerInfo ?? info;

			if (info != null && !info.BizObj.IsNull && infoToValidate != null && !infoToValidate.BizObj.IsNull)
			{
				var oldNotifications = (wrappedInfo != null && !isWrappedInfoForSubProperty) ? infoToValidate.Notifications.ToList() : null;

				if (infoToValidate.BizObj is IObsoleteValidation)
				{
					CallObsoleteValidationForProperty(infoToValidate);
				}
				else
				{
					var validateMethodFound = CallValidationForProperty(infoToValidate);
					if (!validateMethodFound)
					{
						CallObsoleteValidationForProperty(infoToValidate);
					}
				}

				if (wrappedInfo != null)
				{
					var newNotifications = !isWrappedInfoForSubProperty ? infoToValidate.Notifications.ToList() : null;
					if (isWrappedInfoForSubProperty || HasDifferentNotifications(oldNotifications, newNotifications))
					{
						wrappedInfo.RefreshBinding();
					}
				}
			}
		}

		bool HasDifferentNotifications(IReadOnlyList<INotification> oldNotifications, IReadOnlyList<INotification> newNotifications)
		{
			if ((oldNotifications == null || newNotifications == null) && !ReferenceEquals(oldNotifications, newNotifications))
			{
				return true;
			}

			return !newNotifications.HasSameNotificationsIgnoringOrder(oldNotifications);
		}

		bool CallValidationForProperty(ZPropertyInfo info)
		{
			MethodInfo validationMethod = GetValidateMethod(info);
			bool methodFound = (validationMethod != null);

			if (methodFound)
			{
				validationMethod.Invoke(info.BizObj.ValidationInternal, null);
			}

			return methodFound;
		}

		bool CallObsoleteValidationForProperty(ZPropertyInfo info)
		{
			MethodInfo validationMethod = GetObsoleteValidateMethod(info);
			bool methodFound = (validationMethod != null);

			if (methodFound)
			{
				validationMethod.Invoke(info.BizObj, null);
			}

			return methodFound;
		}

		MethodInfo GetValidateMethod(ZPropertyInfo info)
		{
			return (info.BizObj.ValidationInternal != null) ? info.BizObj.ValidationInternal.GetType().GetMethod("Validate" + info.Name) : null;
		}

		MethodInfo GetObsoleteValidateMethod(ZPropertyInfo info)
		{
			return info.BizObj.GetType().GetMethod("Validate" + info.Name, Array.Empty<Type>());
		}

		bool IBusinessObjectInternals.HasValidateMethod(ZPropertyInfo info)
		{
			return (info.BizObj is IObsoleteValidation) ? (GetObsoleteValidateMethod(info) != null) : (GetValidateMethod(info) != null);
		}

		#endregion

		#region BizOBj.ValidationInternal Object

		internal ZValidation ValidationInternal
		{
			get
			{
				if (this is IObsoleteValidation)
				{
					throw new NotSupportedException("The BusinessObject [" + GetType().FullName + "] is using obsolete validation and thus BusinessObject.Validation cannot be used.");
				}

				return (TypedValidationInfo != null) ? (ZValidation)TypedValidationInfo.GetValue(this, null) : null;
			}
		}

		PropertyInfo TypedValidationInfo
		{
			get
			{
				if (typedValidationInfo == null && !hasNoValidation)
				{
					Type currentType = GetType();
					PropertyInfo validationProperty = null;

					while (validationProperty == null)
					{
						if (currentType == typeof(BusinessObject))
						{
							hasNoValidation = true;
							break; // found no validation object
						}

						validationProperty = currentType.GetProperty("Validation", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
						currentType = currentType.BaseType;
					}

					typedValidationInfo = validationProperty;
				}

				return typedValidationInfo;
			}
		}

		public bool ShowWarningIfCancelled { get; set; }

		#endregion

		#endregion

		#region Lookups

		protected virtual bool IsLookupsCachedInBase
		{
			get { return true; }
		}

		#endregion

		#region LoadChildEditableObjects

		public class LoadChildEditableObjectsProgressEventArgs : EventArgs
		{
			public LoadChildEditableObjectsProgressEventArgs(string status, int percentage, bool isFinished)
			{
				fStatus = status;
				fPercentage = percentage;
				fIsFinished = isFinished;
			}

			public string Status
			{
				get { return fStatus; }
			}
			readonly string fStatus;

			public int Percentage
			{
				get { return fPercentage; }
			}
			readonly int fPercentage;

			public bool IsFinished
			{
				get { return fIsFinished; }
			}
			readonly bool fIsFinished;
		}

		public void LoadChildEditableObjects()
		{
			LoadChildEditableObjects(true, true);
		}

		public void LoadChildEditableObjectsForChild(IBusiness[] childList, EventHandler<LoadChildEditableObjectsProgressEventArgs> progressUpdate = null, Func<BusinessObject, BusinessObject[]> getAdditionalChildren = null, Action<BusinessObject> addAdditionalFetchHints = null)
		{
			LoadChildEditableObjects(true, true, childList, progressUpdate, getAdditionalChildren, addAdditionalFetchHints);
		}

		public void FetchForLoadChildEditableObjectsIfNeeded()
		{
			if (!GetBool(Flags.FetchForLoadChildEditableObjectsCalled))
			{
				SetBool(Flags.FetchForLoadChildEditableObjectsCalled, true);
				FetchStrategy.FetchForLoadChildEditableObjects();
			}
		}

		IBusiness[] GetAllChildren() => GetChildren().Union(OtherChildrenToLoad()).Distinct().ToArray();

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		IEnumerable<BusinessObject> LoadChildEditableObjects(bool runFetch, bool loadChild, IBusiness[] childList = null, EventHandler<LoadChildEditableObjectsProgressEventArgs> progressUpdate = null, Func<BusinessObject, BusinessObject[]> getAdditionalChildren = null, Action<BusinessObject> addAdditionalFetchHints = null)
		{
			if (runFetch)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				addAdditionalFetchHints?.Invoke(this);
			}

			if (childList == null)
			{
				foreach (PropertyDescriptor property in ChildEditableProperties)
				{
					property.GetValue(this);
				}
				childList = GetAllChildren();
			}
			List<BusinessObject> additionalChildren = null;
			if (getAdditionalChildren != null)
			{
				additionalChildren = getAdditionalChildren(this)?.ToList();
			}
			var childrenToLoad = new List<BusinessObject>(childList.Length);
			while (childList.Length > 0)
			{
				foreach (var businessEntity in childList)
				{
					var businessObject = businessEntity as BusinessObject;
					var collection = businessEntity as IBusinessObjectCollection;
					if (businessObject != null)
					{
						businessObject.FetchForLoadChildEditableObjectsIfNeeded();
						addAdditionalFetchHints?.Invoke(businessObject);
						childrenToLoad.Add(businessObject);
						if (additionalChildren != null && additionalChildren.Contains(businessObject))
						{
							additionalChildren.Remove(businessObject);
						}
					}
					else if (collection != null)
					{
						foreach (BusinessObject bizo in collection)
						{
							bizo.FetchForLoadChildEditableObjectsIfNeeded();
							addAdditionalFetchHints?.Invoke(bizo);
							childrenToLoad.Add(bizo);
							if (additionalChildren != null && additionalChildren.Contains(bizo))
							{
								additionalChildren.Remove(bizo);
							}
						}
					}
				}
				if (additionalChildren == null)
				{
					break;
				}
				else
				{
					childList = additionalChildren.ToArray();
				}
			}

			if (loadChild)
			{
				var level = 0;
				var showProgress = progressUpdate != null;
				while (childrenToLoad.Count > 0)
				{
					level++;
					var result = new List<BusinessObject>(childrenToLoad.Count);
					var total = childrenToLoad.Count;
					int progressUpdateMark = Math.Max(1, (total / 20));
					for (var count = 0; count < total; count++)
					{
						if (showProgress && count % progressUpdateMark == 0)
						{
							progressUpdate(this, new LoadChildEditableObjectsProgressEventArgs(Res.GetString("{A7569601-C293-41DF-AB32-69ECEFC6B1C5}", "Loading level {0} related data {1} of {2}.", level, count, total), count * 100 / total, false));
						}
						result.AddRange(childrenToLoad[count].LoadChildEditableObjects(false, false, getAdditionalChildren: getAdditionalChildren, addAdditionalFetchHints: addAdditionalFetchHints));
					}
					childrenToLoad = result;
				}
				if (showProgress)
				{
					progressUpdate(this, new LoadChildEditableObjectsProgressEventArgs(Res.GetString("{46C45DF1-BCCB-4317-A751-460BC8FC8F6A}", "Finish loading related data."), 100, true));
				}
				return Array.Empty<BusinessObject>();
			}
			else
			{
				return childrenToLoad;
			}
		}

		protected virtual IBusiness[] OtherChildrenToLoad()
		{
			return Array.Empty<IBusiness>();
		}

		PropertyDescriptor[] ChildEditableProperties
		{
			get
			{
				PropertyDescriptor[] result = childEditableProperties[GetType()];
				if (result == null)
				{
					List<PropertyDescriptor> list = new List<PropertyDescriptor>();
					foreach (PropertyDescriptor property in GetProperties())
					{
						if ((typeof(IBusiness).IsAssignableFrom(property.PropertyType) || typeof(ICollection).IsAssignableFrom(property.PropertyType)) &&
							ChildEditableAttribute.GetValue(property))
						{
							list.Add(property);
						}
					}
					result = list.ToArray();
					childEditableProperties.Add(GetType(), result);
				}
				return result;
			}
		}

		#endregion

		#region GetRelatedBizO

		public static BusinessObject GetRelatedBizO(ZPropertyInfo propInfo)
		{
			BusinessObject result = null;
			if (propInfo.BizObj.ZPropertyInfoHash.ContainsKey(propInfo.Name))
			{
				string relatedBizObjName = RelatedBusinessObjectAttribute.GetRelatedBizObjName(propInfo.BizObj.ZPropertyInfoHash[propInfo.Name].PropertyDescriptor);
				if (relatedBizObjName != null)
				{
					PropertyDescriptor relatedProperty = TypeDescriptor.GetProperties(propInfo.BizObj)[relatedBizObjName];
					if (relatedProperty != null)
					{
						result = (BusinessObject)relatedProperty.GetValue(propInfo.BizObj);
					}
				}
			}
			return result;
		}

		#endregion

		#region GetObsoletePropertyInfoMetaData / SetObsoletePropertyInfoMetaData

		internal object GetObsoletePropertyInfoMetaData(string metaDataTypeId, string propertyName)
		{
			object result = null;
			if (obsoleteMetaData == null || !obsoleteMetaData.TryGetValue(new MetaDataAndPropertyName(metaDataTypeId, propertyName), out result))
			{
				MetaDataType type = MetaDataType.GetMetaDataType(metaDataTypeId);
				result = type.DefaultValue;
			}
			return result;
		}

		internal void SetObsoletePropertyInfoMetaData(string metaDataTypeId, string propertyName, object value)
		{
			MetaDataAndPropertyName key = new MetaDataAndPropertyName(metaDataTypeId, propertyName);
			MetaDataType type = MetaDataType.GetMetaDataType(metaDataTypeId);
			if (object.Equals(value, type.DefaultValue) && obsoleteMetaData != null)
			{
				if (obsoleteMetaData.Remove(key))
				{
					OnElementChanged();
				}
			}
			else
			{
				if (obsoleteMetaData == null)
				{
					obsoleteMetaData = new Dictionary<MetaDataAndPropertyName, object>();
				}
				object currentValue;
				if (!obsoleteMetaData.TryGetValue(key, out currentValue))
				{
					currentValue = type.DefaultValue;
				}
				if (!object.Equals(value, currentValue))
				{
					obsoleteMetaData[key] = value;
					OnElementChanged();
				}
			}
		}

		class MetaDataAndPropertyName
		{
			public MetaDataAndPropertyName(string metaDataTypeId, string propertyName)
			{
				this.MetaDataTypeId = metaDataTypeId;
				this.PropertyName = propertyName;
			}

			readonly string MetaDataTypeId;
			readonly string PropertyName;

			public override bool Equals(object obj)
			{
				MetaDataAndPropertyName rhs = obj as MetaDataAndPropertyName;
				return
					rhs != null &&
					MetaDataTypeId == rhs.MetaDataTypeId &&
					PropertyName == rhs.PropertyName;
			}

			public override int GetHashCode()
			{
				return PropertyName.GetHashCode();
			}
		}

		#endregion

		void IBusinessObjectState.ClearHasChangesIncludingChildren()
		{
			ClearHasChanges();
			ResetHasChangesFromDeleteInCollections();
			foreach (IBusinessObjectState child in GetChildren())
			{
				child.ClearHasChangesIncludingChildren();
			}
		}

		public ZPropertyInfo FindPropertyInfo(string propertyName)
		{
			ZPropertyInfo result = null;

			if (ZPropertyInfoHash.ContainsKey(propertyName))
			{
				result = ZPropertyInfoHash[propertyName];
			}

			return result;
		}

		public ZDateTime InstantiationTime { get; private set; }

		IDisposable HandleClusterKeyChange(ZPropertyInfo info, IZType oldValue, IZType newValue)
		{
			IDisposable result = null;
			if (oldValue is ZInt oldClusterKey && oldClusterKey > 0
				&& newValue is ZInt newClusterKey && newClusterKey > 0
				&& this is IClusterKeyMasterEntity master)
			{
				var clusterKeyPropertyName = master.ClusterKeyPty.Name;
				if (info.Name.Equals(clusterKeyPropertyName, StringComparison.InvariantCulture))
				{
					var childrenWithClusterKey = GetAllChildrenWithClusterKey();
					result = new DisposableAction(() =>
					{
						foreach (var childEntity in childrenWithClusterKey)
						{
							childEntity.ClusterKeyPty.Value = newClusterKey;
						}
					});
				}
			}
			return result;
		}

		IClusterKeyEntity[] GetAllChildrenWithClusterKey()
		{
			LoadChildEditableObjects();
			var checkedBizObjs = new HashSet<BusinessObject>();
			checkedBizObjs.Add(this);
			return GetAllChildrenWithClusterKey(checkedBizObjs, this).ToArray();
		}

		IEnumerable<IClusterKeyEntity> GetAllChildrenWithClusterKey(HashSet<BusinessObject> checkedBizObjs, BusinessObject parentBizo)
		{
			foreach (var child in parentBizo.GetAllChildren())
			{
				if (child is BusinessObject childBizo)
				{
					foreach (var childEntity in GetChildrenWithClusterKey(checkedBizObjs, childBizo))
					{
						yield return childEntity;
					}
				}
				else if (child is IBusinessObjectCollection collection)
				{
					foreach (BusinessObject bizo in collection)
					{
						foreach (var childEntity in GetChildrenWithClusterKey(checkedBizObjs, bizo))
						{
							yield return childEntity;
						}
					}
				}
			}
		}

		IEnumerable<IClusterKeyEntity> GetChildrenWithClusterKey(HashSet<BusinessObject> checkedBizObjs, BusinessObject bizo)
		{
			if (!checkedBizObjs.Contains(bizo))
			{
				checkedBizObjs.Add(bizo);

				if (bizo is IClusterKeyEntity clusterKeyEntity)
				{
					yield return clusterKeyEntity;
				}
				foreach (var childEntity in GetAllChildrenWithClusterKey(checkedBizObjs, bizo))
				{
					yield return childEntity;
				}
			}
		}

		#region Strategies

		internal IEnumerable<IBusinessObjectStrategy> Strategies
		{
			get
			{
				IBusinessObjectStrategy[] defaultStrategies = this.DefaultStrategies;
				if (defaultStrategies != null)
				{
					foreach (IBusinessObjectStrategy strategy in defaultStrategies)
					{
						yield return strategy;
					}
				}

				IBusinessObjectStrategy[] overriddenStrategies = GetStrategies();
				if (overriddenStrategies != null)
				{
					foreach (IBusinessObjectStrategy strategy in overriddenStrategies)
					{
						yield return strategy;
					}
				}
			}
		}

		protected virtual IBusinessObjectStrategy[] GetStrategies()
		{
			return null;
		}

		IBusinessObjectStrategy[] DefaultStrategies
		{
			get
			{
				if (!GetBool(Flags.DefaultStrategiesRetrieved))
				{
					BusinessObjectStrategiesCache strategiesCache = Factory.ServiceContainer.GetService<BusinessObjectStrategiesCache>();
					if (strategiesCache == null)
					{
						strategiesCache = new BusinessObjectStrategiesCache();
						Factory.ServiceContainer.AddService(strategiesCache);
					}

					defaultStrategies = strategiesCache.GetStrategy(TableName);
					SetBool(Flags.DefaultStrategiesRetrieved, true);
				}

				return defaultStrategies;
			}
		}

		sealed class BusinessObjectStrategiesCache : IService
		{
			public IBusinessObjectStrategy[] GetStrategy(string tableName)
			{
				object result = strategiesForTables[tableName];
				if (result == null)
				{
					ObjectHandle handle = (ObjectHandle)AllStrategies[tableName];
					result = handle == null ? null : handle.GetObject();

					ArrayList strategies = new ArrayList();
					if (result is ICollection)
					{
						strategies.AddRange((ICollection)result);
					}
					else if (result != null)
					{
						strategies.Add(result);
					}

					if (!string.IsNullOrEmpty(tableName))
					{
						IBusinessObjectStrategy[] genericStrategies = GetStrategy(string.Empty);
						foreach (IBusinessObjectStrategy strategy in genericStrategies)
						{
							strategies.Add(strategy);
						}
					}

					result = strategies.ToArray(typeof(IBusinessObjectStrategy));
					strategiesForTables[tableName] = result;
				}

				return result as IBusinessObjectStrategy[];
			}

			readonly Hashtable strategiesForTables = new Hashtable();

			Hashtable AllStrategies
			{
				get
				{
					if (!allStrategiesRetrieved)
					{
						allStrategies = ObjectFactory.Contains("BusinessObjectStrategies") ? (Hashtable)ObjectFactory.Get("BusinessObjectStrategies") : null;
						allStrategiesRetrieved = true;
					}
					return allStrategies;
				}
			}

			Hashtable allStrategies;

			bool allStrategiesRetrieved;
		}

		void DoStrategyAction(SavingStrategyAction action)
		{
			foreach (IBusinessObjectStrategy strategy in Strategies)
			{
				action(strategy);
			}
		}

		delegate void SavingStrategyAction(IBusinessObjectStrategy strategy);

		#endregion

		#region RefreshBinding

		public void RefreshBinding()
		{
			if (!IsDeleted)
			{
				OnElementChanged();
			}
		}

		/// <summary>
		/// Calls RefreshBinding on this object and all its registered-editable children.
		/// </summary>
		public void RefreshBindingIncludingChildren()
		{
			RefreshBinding();

			if (children != null)
			{
				foreach (IBusiness child in GetChildren())
				{
					if (child != null)
					{
						child.RefreshBindingIncludingChildren();
					}
				}
			}
		}

		#endregion

		#region ICodeDescription Members

		object ICodeDescription.PK
		{
			get { return PK; }
		}

		string ICodeDescription.Code
		{
			get { return CodePropertyAttribute.CodeFromBusinessObject(this).Trim(); }
		}

		string ICodeDescription.Description
		{
			get { return DescriptionPropertyAttribute.DescriptionFromBusinessObject(this).Trim(); }
		}

		#endregion

		#region INeedTable Members

		ZDataTable INeedTable.Table
		{
			get { return (ZDataTable)this.Row.Table; }
		}

		#endregion

		#region HumanReadableName

		public ZString HumanReadableName
		{
			get
			{
				using (((IBusinessObjectInternals)this).SuppressReportRowDeletedError())
				{
					return HumanReadableNameCore;
				}
			}
		}

		internal ZString HumanReadableNameForPlural  // Don't make this public. Some people (e.g. $\dev\Enterprise\Architecture\Web\Business\Base\WebFilterBusinessObjectFactory.cs) use their own versions of BusinessObjectFactory and manualy add their own rows to tables (e.g. row.Table.Rows.Add(row)), which explodes if this guy is public.
		{
			get { return HumanReadableNameForPluralCore; }
		}

		internal static string HumanReadableNameWhenNoTable
		{
			get { return Res.GetString("f9405425-8328-411d-9821-9592b3be78e7", "record"); }
		}

		protected virtual ZString HumanReadableNameCore
		{
			get { return Table == null ? HumanReadableNameWhenNoTable : DataBoundResourceStrings.GetStringForTable(GetType()); }
		}

		/// <summary>
		/// If your name does not follow default basic grammar rules, override this.  e.g. Box-->Boxes; Child-->Children.
		/// </summary>
		protected virtual ZString HumanReadableNameForPluralCore
		{
			get { return HumanReadableName + (NoResString)"s"; } // I have notified those whom are responsible for this.
		}

		#endregion

		#region HumanReadableShortcutName

		public ZString HumanReadableShortcutName
		{
			get
			{
				using (((IBusinessObjectInternals)this).SuppressReportRowDeletedError())
				{
					return HumanReadableShortcutNameCore;
				}
			}
		}

		protected virtual ZString HumanReadableShortcutNameCore
		{
			get
			{
				var shortcutName = CalculateShortcutName();

				return string.IsNullOrEmpty(shortcutName) ? HumanReadableName : shortcutName;
			}
		}

		protected ZString CalculateShortcutName()
		{
			var shortcutName = string.Empty;
			var codeDescription = (ICodeDescription)this;

			if (codeDescription != null)
			{
				try
				{
					shortcutName = codeDescription.Code;
				}
				catch (NoCodePropertyException)
				{
				}

				try
				{
					if (!string.IsNullOrEmpty(codeDescription.Description) && shortcutName != codeDescription.Description)
					{
						shortcutName += (string.IsNullOrEmpty(shortcutName) ? "" : " - ") + codeDescription.Description;
					}
				}
				catch (NoCodePropertyException)
				{
				}
			}

			return shortcutName;
		}

		#endregion

		#region HumanReadableItemCode

		public ZString HumanReadableItemCode
		{
			get
			{
				using (((IBusinessObjectInternals)this).SuppressReportRowDeletedError())
				{
					return HumanReadableItemCodeCore;
				}
			}
		}

		protected virtual ZString HumanReadableItemCodeCore
		{
			get
			{
				return CalculateItemCode();
			}
		}

		protected ZString CalculateItemCode()
		{
			var codeDescription = this as ICodeDescription;

			if (codeDescription != null)
			{
				try
				{
					return codeDescription.Code;
				}
				catch (NoCodePropertyException)
				{
				}
			}

			return string.Empty;
		}

		#endregion

		#region Implementation

		protected void ActionWhenDataIsChangedAndIsNotCopying<T>(T oldValue, T newValue, Action<T, T> action)
			where T : IZType
		{
			if (!IsCopying && !oldValue.Equals(newValue))
			{
				action(oldValue, newValue);
			}
		}

		IBusiness[] IBusiness.Children
		{
			get { return children; }
		}

		IBusiness[] GetChildren()
		{
			return children;
		}

		void AddChild(IBusiness child)
		{
			ShrinkChildren(1);
			children[children.Length - 1] = child;
		}

		void RemoveChild(IBusiness child)
		{
			bool found = false;

			for (int i = children.Length - 1; i >= 0; i--)
			{
				if (children[i] == child)
				{
					children[i] = null;
					found = true;
					break;
				}
			}

			if (found)
			{
				ShrinkChildren(-1);
			}
		}

		void ShrinkChildren(int addendum)
		{
			var newArray = new IBusiness[children.Length + addendum];

			int newIndex = 0;
			for (int i = 0; i < children.Length; i++)
			{
				var entry = children[i];

				if (entry != null)
				{
					newArray[newIndex++] = entry;
				}
			}

			children = newArray;
		}

		bool ContainsChild(IBusiness child)
		{
			foreach (var each in children)
			{
				if (each == child)
				{
					return true;
				}
			}

			return false;
		}

		IBusiness GetChild(int index)
		{
			if (index > children.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			return children[index];
		}

		protected virtual int GetDefaultAccessedZInfoPropertiesCapacity()
		{
			return 0;
		}

		void InitialiseBusinessObject()
		{
			fZPropertyInfoHash = CreateZPropertyInfoHashtable();

#if DEBUG
			if (Factory != null)
			{
				SetupTraceData();
			}
#endif

			SetPKAndDefaults();
		}

		protected virtual ZPropertyInfoHashtable CreateZPropertyInfoHashtable()
		{
			return new ZPropertyInfoHashtable(this);
		}

		bool PropertyNameIsPK(string propertyName)
		{
			return propertyName == (TablePrefix + "_PK");
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetValidationFromController(IValidateForController validationFromController)
		{
			fValidationFromController = validationFromController;
		}

		#region IsNewAndAllPropertiesDuplicatesOfExistingObject

		protected bool IsNewAndAllPropertiesDuplicatesOfExistingObject
		{
			get { return (!IsInDatabase && !IsDeleted) && IsDuplicateOfMeInDatabase(true); }
		}

		protected bool IsNewAndAllPropertiesExceptAuditColumnsAreDuplicatesOfExistingBusinessObject
		{
			get { return (!IsInDatabase && !IsDeleted) && IsDuplicateOfMeInDatabase(false); }
		}

		[SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		bool IsDuplicateOfMeInDatabase(bool includeAuditColumns)
		{
			ZQuery filter = new ZQuery();

			var columns = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumns(TableName);
			for (int i = columns.Count - 1; i >= 0; i--)
			{
				var column = columns[i];
				if (column.IsPKColumn)
				{
					filter.AddToFilter(column, SQLComparisonOperator.NotEqual, PK);
				}
				else if (includeAuditColumns || !IsAuditColumn(column))
				{
					var value = (IZType)this[column];
					if (column.IsNullable && value.IsEmpty)
					{
						var subFilter = new ZQuery(column, value);
						subFilter.AddToFilter(JoinCondition.Or, column, SQLComparisonOperator.Equal, null);
						filter.AddToFilter(subFilter);
					}
					else
					{
						filter.AddToFilter(column, value);
					}
				}
			}

			return Factory.LoadTop1(GetType(), filter) != null;
		}

		bool IsAuditColumn(SchemaColumn column)
		{
			return column.Name.EndsWith("_SystemCreateTimeUtc", StringComparison.OrdinalIgnoreCase)
				|| column.Name.EndsWith("_SystemCreateUser", StringComparison.OrdinalIgnoreCase)
				|| column.Name.EndsWith("_SystemLastEditTimeUtc", StringComparison.OrdinalIgnoreCase)
				|| column.Name.EndsWith("_SystemLastEditUser", StringComparison.OrdinalIgnoreCase);
		}

		public virtual bool ShouldSkipUpdateAuditColumns { get; }

		#endregion

		#region Get ZPropertyInfo

		/// <summary>
		/// The ZPropertyInfo object for the specified PropertyName.
		/// </summary>
		/// <param name="propertyName">The property name on which to retrieve the Info object.</param>
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected internal ZPropertyInfo GetZPropertyInfo(string propertyName)
		{
			ZPropertyInfo result;

			PropertyDescriptor descriptor = TryReallyHardToGetPropertyDescriptor(propertyName);

			Type propertyType = descriptor.PropertyType;

			if (propertyType == typeof(ZString) || propertyType == typeof(string))
			{
				result = new ZPropertyInfoString(this, propertyName, descriptor);
			}
			else if (propertyType == typeof(ZDecimal) || propertyType == typeof(decimal))
			{
				result = new ZPropertyInfoDecimal(this, propertyName, descriptor);
			}
			else if (propertyType == typeof(ZGuid) || propertyType == typeof(Guid))
			{
				result = new ZPropertyInfoGuid(this, propertyName, descriptor);
			}
			else if (propertyType == typeof(ZBool) || propertyType == typeof(bool))
			{
				result = new ZPropertyInfoBool(this, propertyName, descriptor);
			}
			else if (propertyType == typeof(ZDateTime) || propertyType == typeof(DateTime))
			{
				result = new ZPropertyInfoDateTime(this, propertyName, descriptor);
			}
			else if (propertyType == typeof(ZDateTimeOffset) || propertyType == typeof(DateTimeOffset))
			{
				result = new ZPropertyInfoDateTimeOffset(this, propertyName, descriptor);
			}
			else if (propertyType == typeof(ZDate))
			{
				result = new ZPropertyInfoDate(this, propertyName, descriptor);
			}
			else if (propertyType == typeof(ZLong) || propertyType == typeof(long))
			{
				result = new ZPropertyInfoLong(this, propertyName, descriptor);
			}
			else if (propertyType == typeof(ZInt) || propertyType == typeof(int))
			{
				result = new ZPropertyInfoInt(this, propertyName, descriptor);
			}
			else if (propertyType == typeof(ZShort) || propertyType == typeof(short))
			{
				result = new ZPropertyInfoShort(this, propertyName, descriptor);
			}
			else if (propertyType == typeof(ZByte) || propertyType == typeof(byte))
			{
				result = new ZPropertyInfoByte(this, propertyName, descriptor);
			}
			else if (propertyType == typeof(ZBlob) || propertyType == typeof(byte[]))
			{
				result = new ZPropertyInfoBlob(this, propertyName, descriptor);
			}
			else if (propertyType == typeof(ZGeography) || propertyType == typeof(SqlGeography))
			{
				result = new ZPropertyInfoGeography(this, propertyName, descriptor);
			}
			else if (propertyType == typeof(ZTime))
			{
				result = new ZPropertyInfoTime(this, propertyName, descriptor);
			}
			else if (propertyType == typeof(SQLComparisonOperator))
			{
				result = new ZPropertyInfo(this, propertyName, descriptor);
			}
			else if (typeof(IMultilingualString).IsAssignableFrom(propertyType))
			{
				result = new ZPropertyInfo(this, propertyName, descriptor);
			}
			else
			{
				result = null;
#if DEBUG
				throw new NotSupportedException(
					"The property " + propertyName + " [" + propertyType.FullName +
					"] is not a valid ZType (and should not have a ZPropertyInfo property).");
#endif
			}

			return result;
		}

		// when reverting this, remove the 'internal' visibility from BusinessObjectPropertyDescriptorCollection.
		PropertyDescriptor TryReallyHardToGetPropertyDescriptor(string propertyName)
		{
			KPropertyDescriptorCollection allProperties = GetProperties().AllProperties;
			PropertyDescriptor descriptor = allProperties[propertyName];
			if (descriptor == null && this is IDynamicBusinessObject)
			{
				RefreshDynamicBusinessObjectPropertyDescriptorCollection();
				descriptor = allProperties[propertyName];
			}

			if (descriptor == null)
			{
				string propertyNames = "";
				bool foundAfterThreadYield = false;
				bool foundAfterClearingLRUCaches = false;
				bool foundAfterMakingEntirelyNewPropertyDescriptorCollectionAndUsingThat = false;

				try
				{
					foreach (PropertyDescriptor property in allProperties)
					{
						propertyNames += property.Name + "\n";
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					propertyNames = ex.Message;
				}
				Thread.Sleep(0);
				descriptor = allProperties[propertyName];
				if (descriptor != null)
				{
					foundAfterThreadYield = true;
				}
				else
				{
					ClearLRUCaches();
					allProperties = GetProperties().AllProperties;
					descriptor = allProperties[propertyName];
					if (descriptor != null)
					{
						foundAfterClearingLRUCaches = true;
					}
					else
					{
						descriptor = new BusinessObjectPropertyDescriptorCollection(GetType(), true)[propertyName];
						if (descriptor != null)
						{
							foundAfterMakingEntirelyNewPropertyDescriptorCollectionAndUsingThat = true;
						}
					}
				}

				string message =
					(NoResString)"Could not find property : " + propertyName + (NoResString)" on Type : " + GetType().FullName + (NoResString)"\n" +
					(NoResString)"Property names in GetProperties().AllProperties:\n" +
					(NoResString)"\n" +
					propertyNames + (NoResString)"\n" +
					(NoResString)"foundAfterThreadYield: " + foundAfterThreadYield + (NoResString)"\n" +
					(NoResString)"foundAfterClearingLRUCaches: " + foundAfterClearingLRUCaches + (NoResString)"\n" +
					(NoResString)"foundAfterMakingEntirelyNewPropertyDescriptorCollectionAndUsingThat: " + foundAfterMakingEntirelyNewPropertyDescriptorCollectionAndUsingThat;
				ErrorReporter.ReportOnce("CouldNotFindZPropertyInfoFor_" + propertyName, message);

				if (descriptor == null)
				{
					throw new TryHarderNextTimeException(message);
				}
			}
			return descriptor;
		}

		[Serializable]
		class TryHarderNextTimeException : Exception
		{
			public TryHarderNextTimeException(string message)
				: base(message)
			{
			}

#if NETFRAMEWORK
			protected TryHarderNextTimeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		static void ClearLRUCaches()
		{
			foreach (ILRUCache cache in LRUCache.AllInstances)
			{
				cache.Clear();
			}
		}

		protected internal ZWrappedPropertyInfo GetWrappedZPropertyInfo(string propertyName, ZPropertyInfoGetter innerInfoGetter)
		{
			return new ZWrappedPropertyInfo(propertyName, this, innerInfoGetter);
		}

		protected void RegisterListChangedCalledRefreshBinding(IBindingList element)
		{
			if (element != null)
			{
				element.ListChanged -= new ListChangedEventHandler(element_ListChanged);
				element.ListChanged += new ListChangedEventHandler(element_ListChanged);
			}
		}

		protected void UnRegisterListChangedCalledRefreshBinding(IBindingList element)
		{
			if (element != null)
			{
				element.ListChanged -= new ListChangedEventHandler(element_ListChanged);
			}
		}

		void element_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (!GetBool(Flags.ListChangedCalled))
			{
				try
				{
					SetBool(Flags.ListChangedCalled, true);
					RefreshBinding();
				}
				finally
				{
					SetBool(Flags.ListChangedCalled, false);
				}
			}
		}

		protected internal ZPropertyInfo GetZPropertyInfo(string propertyName, string humanReadableName)
		{
			ZPropertyInfo info = GetZPropertyInfo(propertyName);
			info.HumanReadableName = humanReadableName;
			return info;
		}

		#endregion

		#region HasNotificationChanged

		public event EventHandler<NotificationsChangedEventArgs> NotificationsChanged;

		void HandleChildNotificationChanged(object sender, EventArgs e)
		{
			OnNotificationsChanged(false);
		}

		internal void OnNotificationsChanged(bool raiseElementChanged)
		{
			if (raiseElementChanged && !IsInPreSaveValidation && !((ISingleElementListInternal)this).IsListChangeSuspended && !IsDeleted)
			{
				OnElementChanged();
			}
			OnNotificationsChanged();
		}

		void OnNotificationsChanged()
		{
			NotificationChangeCount++;
			if (NotificationChangeCount == int.MaxValue)
			{
				NotificationChangeCount = 0;
			}

			NotificationsChanged?.Invoke(this, new NotificationsChangedEventArgs(this));
		}

		#endregion

		#region IBusiness Members

		#region ValidationSuspension Management

		sealed class ValidationSuspender : IDisposable
		{
			public ValidationSuspender(BusinessObject bizO)
			{
				this.BizO = bizO;
				bizO.SuspendValidation();
				DisposableLeakListener.Instance.RegisterDisposable(this);
			}

			public void Dispose()
			{
				try
				{
					if (!HasResumed)
					{
						BizO.ResumeValidation();
						HasResumed = true;
					}
				}
				finally
				{
					DisposableLeakListener.Instance.UnRegisterDisposable(this);
				}
			}

			readonly BusinessObject BizO;
			bool HasResumed;
		}

		public IDisposable GetValidationSuspender()
		{
			return new ValidationSuspender(this);
		}

		public void SuspendValidation()
		{
			checked
			{
				validationIndex++;
			}

			foreach (var child in GetChildren())
			{
				child.SuspendValidation();
			}
		}

		public void ResumeValidation()
		{
			if (validationIndex <= 0)
			{
				validationIndex = 0;
				return;
			}

			checked
			{
				validationIndex--;
			}

			foreach (var child in GetChildren())
			{
				child.ResumeValidation();
			}
		}

		public IDisposable ResumeValidationTemporarily()
		{
			int oldValidationIndex = ValidationIndex;
			while (ValidationIndex > 0)
			{
				ResumeValidation();
			}
			return new DisposableAction(delegate
			{
				ValidationIndex = oldValidationIndex;
				while (ValidationIndex < oldValidationIndex)
				{
					SuspendValidation();
				}
			});
		}

		IDisposable IBusinessObjectInternals.ResumeValidationForAllDescendantsTemporarily()
		{
			if (IgnoreValidationSuspended)
			{
				return null;
			}

			IgnoreValidationSuspended = true;
			return new DisposableAction(delegate
			{
				IgnoreValidationSuspended = false;
			});
		}

		public bool IgnoreValidationSuspended
		{
			get { return ignoreValidationSuspended; }
			set
			{
				ignoreValidationSuspended = value;
				foreach (IBusiness child in GetChildren())
				{
					child.IgnoreValidationSuspended = value;
				}
			}
		}
		bool ignoreValidationSuspended;

		public bool IsValidationSuspended
		{
			get { return !IgnoreValidationSuspended && (validationIndex > 0 || (Factory != null && Factory.IsValidationSuspended)); }
		}

		internal int ValidationIndex
		{
			get { return validationIndex; }
			set { validationIndex = value; }
		}

		int validationIndex;

		#endregion

		public IDisposable SuspendSettingHasChanges()
		{
			return SuspendSettingHasChangesCore();
		}

		protected virtual IDisposable SuspendSettingHasChangesCore()
		{
			return new HasChangesSuspender(this);
		}

		/// <summary>
		/// Suspends the setting of the HasChanges property if called before saving begins.
		/// If called during the saving process, it will not suspend the setting of HasChanges.
		/// </summary>
		public IDisposable SuspendSettingHasChangesKeepValidationAndSaving()
		{
			return isSavingStarted ? null : SuspendSettingHasChangesKeepValidationAndSavingCore();
		}

		protected virtual IDisposable SuspendSettingHasChangesKeepValidationAndSavingCore()
		{
			return new HasChangesSuspender(this, true);
		}

		public IDisposable SuspendSettingHasChangesIncludingChildren()
		{
			return SuspendSettingHasChangesIncludingChildrenCore();
		}

		protected virtual IDisposable SuspendSettingHasChangesIncludingChildrenCore()
		{
			if (settingHasChangesWithChildrenIndex++ == 0)
			{
				childrenHasChangesSuspenders = new Dictionary<IBusiness, IDisposable>();
				foreach (var child in children)
				{
					var suspendableChild = child as ICanSuspendSettingHasChanges;
					if (suspendableChild != null)
					{
						childrenHasChangesSuspenders.Add(child, suspendableChild.SuspendSettingHasChanges());
					}
				}
			}

			return new HasChangesSuspender(this, additionalAction: () =>
			{
				if (--settingHasChangesWithChildrenIndex == 0)
				{
					foreach (var disposable in childrenHasChangesSuspenders.Values)
					{
						disposable.Dispose();
					}
					childrenHasChangesSuspenders = null;
				}
			});
		}

		sealed class HasChangesSuspender : IDisposable
		{
			public HasChangesSuspender(BusinessObject bizO, bool keepValidationAndSaving = false, Action additionalAction = null)
			{
				this.BizO = bizO;
				bizO.settingHasChangesIndex++;

				this.keepValidationAndSaving = keepValidationAndSaving;
				if (keepValidationAndSaving)
				{
					bizO.settingHasChangesKeepValidationAndSavingIndex++;
				}

				this.additionalAction = additionalAction;

				DisposableLeakListener.Instance.RegisterDisposable(this);
			}

			public void Dispose()
			{
				if (!isDisposed)
				{
					BizO.settingHasChangesIndex--;
					if (keepValidationAndSaving)
					{
						BizO.settingHasChangesKeepValidationAndSavingIndex--;
					}
					additionalAction?.Invoke();

					isDisposed = true;
				}
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}

			readonly BusinessObject BizO;
			readonly bool keepValidationAndSaving;
			readonly Action additionalAction;
			bool isDisposed;
		}

		public void ClearHasChanges()
		{
			restoreSuspendedHasChanges = false;
			if (HasChangesNotIncludingChildren)
			{
				HasChanges = false;
			}
		}

		public bool IsSettingHasChangesSuspended
		{
			get { return settingHasChangesIndex > 0; }
		}

		int settingHasChangesIndex;
		int settingHasChangesKeepValidationAndSavingIndex;
		bool restoreSuspendedHasChanges;
		bool isSavingStarted;

		int settingHasChangesWithChildrenIndex;
		IDictionary<IBusiness, IDisposable> childrenHasChangesSuspenders;

		bool KeepValidationAndSavingWithSuspendedSettingHasChanges
		{
			get { return settingHasChangesKeepValidationAndSavingIndex > 0; }
		}

		void IBusiness.NotifyRegisteredChildEditable()
		{
			NotifyRegisteredChildEditable();
		}

		protected virtual void NotifyRegisteredChildEditable()
		{
		}

		#endregion

		#region DataRefresh

		/// <summary>
		/// Is this BusinessObject being updated by the DataRefreshBus at the moment?
		/// This includes being changed or deleted by the DataRefreshBus.
		/// </summary>
		public bool IsRefreshingByDataRefreshBus
		{
			get { return Factory != null && Factory.RefreshManager.IsRefreshing(this); }
		}

		void DoForDataRefreshThreadSafe(BusinessObject source, Action doAction, Action enqueueAction)
		{
			if (CanBeHandledByDataFresh(source))
			{
				if (Factory == null || Factory.ThreadSentry.OwnerThread.ThreadID != null && Factory.ThreadSentry.OwnerThread.ThreadID == source.Factory.ThreadSentry.OwnerThread.ThreadID)
				{
					doAction();
				}
				else if (Factory.ThreadSentry.IsPostable)
				{
					enqueueAction();
				}
			}
		}

		internal bool CanBeHandledByDataFresh(BusinessObject source)
		{
			return source.Table.TableName == Table.TableName && source.PK == PK;
		}

		bool IBusiness.CanDeleteForDataRefresh => !IsInDatabase && !IsDeleted;

		void IBusiness.DeleteForDataRefresh()
		{
			DeleteForDataRefresh();
		}

		/// <summary>
		/// When overridden, used to delete related objects that could be missed by <see cref="DataRefreshBus"/>.
		/// For example object that only exists in target <see cref="BusinessObjectFactory"/> and so could not be deleted in source BusinessObjectFactory.
		/// </summary>
		protected virtual void DeleteForDataRefresh()
		{
			InternalOnBeforeDeletedByDataRefresh();

			foreach (var child in children.ToList())
			{
				if (child.CanDeleteForDataRefresh)
				{
					child.DeleteForDataRefresh();
				}
			}

			Delete(true);
			InternalOnDeletedByDataRefresh();
			HasChanges = false;
		}

		internal bool IsDeletingForDataRefresh { get; set; }

		internal void DeleteForDataRefreshThreadSafe(BusinessObject source)
		{
			DoForDataRefreshThreadSafe(source,
				() => DeleteForDataRefresh(),
				() => Factory.EnqueueDelete(source.GetType(), source.PK));
		}

		void InternalOnBeforeDeletedByDataRefresh()
		{
			BeforeDeleteByDataRefresh?.Invoke(this, EventArgs.Empty);
			OnBeforeDeletedByDataRefresh();
		}

		void InternalOnDeletedByDataRefresh()
		{
			DeletedByDataRefresh?.Invoke(this, EventArgs.Empty);
			OnDeletedByDataRefresh();
		}

		public event EventHandler BeforeDeleteByDataRefresh;
		public event EventHandler DeletedByDataRefresh;

		protected virtual void OnDeletedByDataRefresh() { }

		protected virtual void OnBeforeDeletedByDataRefresh() { }

		internal void UpdateForDataRefresh(BusinessObject source)
		{
			DoForDataRefreshThreadSafe(source,
				() => PerformRefresh(source.PK, source.Row.ItemArray),
				() => Factory.EnqueueRefresh(source.GetType(), source.PK, source.Row.ItemArray));
		}

		internal void PerformRefresh(ZGuid primaryKey, object[] source)
		{
			bool IsRowInvalid()
			{
				return Row == null || Table == null ||
					IsDeletingForDataRefresh || IsDeleted ||
					 Row.RowState == DataRowState.Deleted || Row.RowState == DataRowState.Detached;
			}

			if (source == null || IsRowInvalid())
			{
				return;
			}

			using (SuspendSettingHasChanges())
			using (SuspendListChanged())
			{
				string pkName = ZDataUtils.GetPKNameFromTable(Table);
				var columns = Table.Columns;

				InternalOnBeforeUpdatedByDataRefresh();

				for (int i = 0; i < source.Length; i++)
				{
					if (IsRowInvalid())
					{
						return;
					}

					var sourceItem = source[i];

					if (sourceItem == null ||
					sourceItem.Equals(Row[i]) ||
					(sourceItem == DBNull.Value && !columns[i].AllowDBNull))
					{
						continue;
					}

					Row[i] = sourceItem;
				}

				Row.AcceptChanges();
			}

			OnLoadedInternal();
			RefreshBinding();
			InternalOnUpdatedByDataRefresh();
		}

		void InternalOnBeforeUpdatedByDataRefresh()
		{
			if (!IsDeleted)
			{
				OnBeforeUpdatedByDataRefresh();
				BeforeUpdatedByDataRefresh?.Invoke(this, EventArgs.Empty);
			}
		}
		public event EventHandler BeforeUpdatedByDataRefresh;

		void InternalOnUpdatedByDataRefresh()
		{
			if (!IsDeleted)
			{
				UpdatedByDataRefresh?.Invoke(this, EventArgs.Empty);
				OnUpdatedByDataRefresh();
				RaiseUpdatedByDataRefreshIncludingChildren();
			}
		}

		public event EventHandler UpdatedByDataRefresh;

		internal void RaiseUpdatedByDataRefreshIncludingChildren()
		{
			updatedByDataRefreshIncludingChildren?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Called just before the DataRefreshBus refreshes this business object
		/// </summary>
		protected virtual void OnBeforeUpdatedByDataRefresh()
		{
		}

		/// <summary>
		/// Called when the DataRefreshBus refreshes this business object
		/// or any of its children
		/// </summary>
		protected virtual void OnUpdatedByDataRefresh()
		{
		}

		event EventHandler IBusinessObjectState.UpdatedByDataRefreshIncludingChildren
		{
			add
			{
				if (updatedByDataRefreshIncludingChildren == null)
				{
					foreach (IBusiness child in GetChildren())
					{
#pragma warning disable
						child.UpdatedByDataRefreshIncludingChildren += HandleChildUpdatedByDataRefreshIncludingChildren;
#pragma warning restore
					}
				}
				updatedByDataRefreshIncludingChildren += value;
			}
			remove
			{
				updatedByDataRefreshIncludingChildren -= value;
				if (updatedByDataRefreshIncludingChildren == null)
				{
					foreach (IBusiness child in GetChildren())
					{
#pragma warning disable
						child.UpdatedByDataRefreshIncludingChildren -= HandleChildUpdatedByDataRefreshIncludingChildren;
#pragma warning restore
					}
				}
			}
		}

		event EventHandler updatedByDataRefreshIncludingChildren;

		void HandleChildUpdatedByDataRefreshIncludingChildren(object sender, EventArgs e)
		{
			RaiseUpdatedByDataRefreshIncludingChildren();
		}

		#endregion

		#region Debugging - Setting up Trace Data
#if DEBUG

		[SuppressThreadStaticFieldMessage]
		public static long _NextGlobalInstance = 0;
		protected internal string _Instance;

		void SetupTraceData()
		{
			_NextGlobalInstance++;
			Factory._BusinessObjectInstance++;
			_Instance = Factory._Instance.ToString() + "." + Factory._BusinessObjectInstance.ToString() + "." + _NextGlobalInstance.ToString();
		}

#endif
		#endregion

		#endregion

		#region Loader

		/// <summary>
		/// Inherit public newed nested loaders in BusinessObjects to centralise
		/// loading logic and SQL. When you subclass a class with a loader, subclass
		/// the loader as a new nested class called "Loader" and override
		/// GetTypeOfBusinessObjectToLoad().
		/// Always use GetTypeOfBusinessObjectToLoad() rather than typeof() in load
		/// methods. This allows a subclass of the business object to have a subclass of the
		/// loader that loads the correct type for its outer class.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)] // only show this in intellisense if it has been sub-classed
		public abstract class Loader
		{
			protected Loader(BusinessObjectFactory factory)
			{
				Argument.NotNull(factory, "factory");
				this.factory = factory;
			}

			// EXAMPLE:
			//			public Vessel LoadFromCallSign(ZString CallSign)
			//			{
			//				ZQuery Filter = new ZQuery(VesselSchema.VS_CallSign, Callsign);
			//				Filter.AddToFilter(VesselSchema.VS_Company, Company.CurrentCompany.PK);
			//				return (Vessel) Factory.LoadTop1(GetTypeOfBusinessObjectToLoad(), Filter);
			//			}

			internal protected abstract Type GetTypeOfBusinessObjectToLoad();

			internal protected BusinessObjectFactory Factory
			{
				get { return factory; }
			}

			readonly BusinessObjectFactory factory;
		}

		#endregion

		#region Compressed storage for PropertyInfos

		internal ZPropertyInfoStorage PropertyInfoStorage
		{
			get
			{
				if (propertyInfoStorage == null)
				{
					if (Factory != null)
					{
						propertyInfoStorage = Factory.PropertyInfoStorage;
					}
					else
					{
						propertyInfoStorage = new ZPropertyInfoStorage();
					}
				}
				return propertyInfoStorage;
			}
		}

		#endregion

		#region OnValueChanged suspension

		public IDisposable SuspendOnValueChanged(string propertyName)
		{
			if (string.IsNullOrWhiteSpace((propertyName)))
			{
				return null;
			}

			if (propertyOnValueChangedSuspensions == null)
			{
				propertyOnValueChangedSuspensions = new Dictionary<string, int>();
			}

			if (propertyOnValueChangedSuspensions.TryGetValue(propertyName, out var count))
			{
				propertyOnValueChangedSuspensions[propertyName] = count + 1;
			}
			else
			{
				propertyOnValueChangedSuspensions[propertyName] = 1;
			}

			void RemoveOnValueChangeSuspension()
			{
				if (!propertyOnValueChangedSuspensions.TryGetValue(propertyName, out var suspensionCount))
				{
					return;
				}

				if (suspensionCount <= 1)
				{
					propertyOnValueChangedSuspensions.Remove(propertyName);

					if (propertyOnValueChangedSuspensions.Count == 0)
					{
						propertyOnValueChangedSuspensions = null;
					}
				}
				else
				{
					propertyOnValueChangedSuspensions[propertyName] = suspensionCount - 1;
				}
			}

			return new DisposableAction(RemoveOnValueChangeSuspension);
		}

		public bool IsOnValueChangeSuspended(string propertyName) => propertyOnValueChangedSuspensions?.ContainsKey(propertyName) ?? false;
		IDictionary<string, int> propertyOnValueChangedSuspensions;

		#endregion

		#region Flags

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsTopLevel
		{
			get { return GetBool(Flags.TopLevel); }
			set { SetBool(Flags.TopLevel, value); }
		}

		public bool IsDeleting
		{
			get { return GetBool(Flags.Deleting); }
			private set { SetBool(Flags.Deleting, value); }
		}

		internal bool WasDeleted
		{
			get => GetBool(Flags.Deleted);
			set => SetBool(Flags.Deleted, value);
		}

		internal bool IsDataRowInDataTable
		{
			get => !GetBool(Flags.DataRowHasNotBeenAddedToDataTableYet);
			set => SetBool(Flags.DataRowHasNotBeenAddedToDataTableYet, !value);
		}

		protected internal bool IsRemovingFromRelationship
		{
			get { return GetBool(Flags.RemovingFromRelationship); }
			internal set { SetBool(Flags.RemovingFromRelationship, value); }
		}

		#endregion

		#region Custom Comparison

		internal bool EqualsBase(object obj)
		{
			return base.Equals(obj);
		}

		internal int GetHashCodeBase()
		{
			return base.GetHashCode();
		}

		#endregion
		#region ICanDelete Members

		public virtual bool CanDelete
		{
			get { return true; }
		}

		public virtual MultilingualString ReasonForNotAbleToDelete
		{
			get { return (NoResString)string.Empty; }
		}

		public void OnCannotDelete()
		{
			OnCannotDeleteCore();
		}

		protected virtual void OnCannotDeleteCore()
		{
		}

		public virtual MultilingualString GetWarningBeforeBeingDeleted()
		{
			return (NoResString)string.Empty;
		}

		#endregion

		bool IBusinessObjectInternals.IsUnCommittedRow
		{
			get { return Row != null && Row.RowState == DataRowState.Detached; }
		}

		protected MultilingualString GetMultilingual(ZPropertyInfo property)
		{
			return property.CustomizableDataResourceStrings.GetMultilingualString(this, (ZString)this[property.Name]);
		}

		/// <summary>
		/// Used for the balloon window on a ZGrid. Override this method to provide a quick summary of a business object on a row in a ZGrid.
		/// </summary>
		public virtual string QuickViewCard
		{
			get { return null; }
		}

		#region XmlColumnSerialisation

		protected internal virtual IEnumerable<XmlColumnSpecification> XmlSerialisedColumns
		{
			get { yield break; }
		}

		protected internal T GetXmlColumnPropertyValue<T>(ZPropertyInfo info)
			where T : IZType
		{
			return XmlColumnDictionary.GetValue<T>(info.Name);
		}

		protected internal void SetXmlColumnPropertyValue<T>(ZPropertyInfo info, T value)
			where T : IZType
		{
			SetXmlColumnPropertyValue(info, value, null);
		}

		protected internal void SetXmlColumnPropertyValue<T>(ZPropertyInfo info, T value, Action onSet)
			where T : IZType
		{
			IZType oldValue;
			IZType newValue;

			if (ShouldSetNonPersistentPropertyValue(info, value, true, out oldValue, out newValue))
			{
				XmlColumnDictionary.SetValue(value, info.Name);
				AfterNonPersistentPropertyValueSet(info, oldValue);
				onSet?.Invoke();
			}
		}

		internal XmlColumnDictionary XmlColumnDictionary
		{
			get { return xmlColumnDictionary ?? (xmlColumnDictionary = new XmlColumnDictionary()); }
		}

		XmlColumnDictionary xmlColumnDictionary;

		#endregion

		#region ICanDetach Members

		public virtual bool CanDetach
		{
			get { return true; }
		}

		public virtual string ReasonNotToBeAbleToDetach
		{
			get { return string.Empty; }
		}

		public virtual string GetWarningBeforeBeingDetached()
		{
			return string.Empty;
		}

		#endregion

		protected static object GetGeographyColumnValue(DataRow row, string columnName, string geoText)
		{
			return SqlGeography.STGeomFromText(new SqlChars(new SqlString(geoText)), 4326);
		}

		protected static object GetGeographyColumnValue(DataRow row, string columnName, SqlGeography geo)
		{
			return geo;
		}

		public ZString GetAdditionalInfoForZSaveException() => GetAdditionalInfoForZSaveExceptionCore();

		protected virtual ZString GetAdditionalInfoForZSaveExceptionCore() => ZString.Empty;

		static class CachedExtensionHooks
		{
			[ThreadSafe]
			static readonly Lazy<List<IBusinessObjectOnIntializedFactory>> factories = new Lazy<List<IBusinessObjectOnIntializedFactory>>(() =>
			{
				var result = new List<IBusinessObjectOnIntializedFactory>();
				if (ObjectFactory.Contains("BusinessObjectOnInitializedFactories"))
				{
					var hookFactories = ObjectFactory.Get<ICollection>("BusinessObjectOnInitializedFactories");
					foreach (var factory in hookFactories.Cast<IBusinessObjectOnIntializedFactory>())
					{
						result.Add(factory);
					}
				}

				return result;
			}, LazyThreadSafetyMode.PublicationOnly);

			public static IEnumerable<IBusinessObjectOnInitialized> OnInitializedHooks
			{
				get
				{
					foreach (var factory in factories.Value)
					{
						yield return factory.BusinessObjectInitializedHandler;
					}
				}
			}
		}
	}

	#region Typed Exceptions

	[Serializable]
	public class CannotDeleteException : ZCannotSaveException
	{
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception Text")]
		public CannotDeleteException(string reasonCannotDelete)
			: base(reasonCannotDelete, "Cannot Delete...")
		{
		}

#if NETFRAMEWORK
		protected CannotDeleteException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	#endregion
}
