using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
#if NETFRAMEWORK
using Enterprise.ZArchitecture.Core;
#endif
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.ZArchitecture.Business.Res;
using ResString = Enterprise.ZArchitecture.Business.ResString;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class MassUpdateWizard : NonPersistentBusinessObject, IObsoleteValidation
	{
		public MassUpdateWizard(IImportCollectionInfo collectionInfo)
			: base(collectionInfo.Collection.Factory)
		{
			this.collectionInfo = collectionInfo;
			LoadFieldDetails();
			UpdateFieldChange();
		}

		public static class Schema
		{
			public const string Field = "Field";
			public const string NewZGuidValue = "NewZGuidValue";
			public const string NewZDateTimeValue = "NewZDateTimeValue";
			public const string NewZDateTimeOffsetValue = "NewZDateTimeOffsetValue";
			public const string NewZDecimalValue = "NewZDecimalValue";
			public const string NewZIntValue = "NewZIntValue";
			public const string NewZShortValue = "NewZShortValue";
			public const string NewZBlobValue = "NewZBlobValue";
			public const string NewZByteValue = "NewZByteValue";
			public const string NewZStringValue = "NewZStringValue";
			public const string NewZBoolValue = "NewZBoolValue";
			public const string NewZGeographyValue = "NewZGeographyValue";
			public const string NewZTimeValue = "NewZTimeValue";
		}

		public IImportCollectionInfo CollectionInfo
		{
			get { return collectionInfo; }
		}
		readonly IImportCollectionInfo collectionInfo;

		#region Properties

		#region Field

		[List("FieldList")]
		public ZString Field
		{
			get { return field; }
			set
			{
				if (field != value)
				{
					ZString oldValue = field;
					CheckMaximumLength(FieldInfo, value);
					field = value;
					UpdateFieldChange();
					FieldInfo.RefreshBinding(oldValue);
					if (!IsValidationSuspended)
					{
						ValidateField();
					}
					NewZStringValueInfo.RefreshBinding();
				}
			}
		}
		ZString field;

		public ZPropertyInfo FieldInfo
		{
			get { return GetZPropertyInfo(Schema.Field); }
		}

		public int Field_MaxLength
		{
			get { return fieldMaxLength; }
		}
		int fieldMaxLength = 68;

		public void ValidateField()
		{
			FieldInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(FieldInfo);
			ListValidation.ErrorIfInvalidCode(FieldInfo, FieldList, ResString.GetMultilingualString("43bbdbeb-d363-42d0-a7e8-ee98ebb389b5", "Field is not valid."));
		}

		public MassUpdateFieldItemList FieldList
		{
			get { return fieldList ?? (fieldList = new MassUpdateFieldItemList()); }
		}
		MassUpdateFieldItemList fieldList;

		#endregion

		public MassUpdateFieldItemList MatchingFieldList
		{
			get { return matchingFieldList ?? (matchingFieldList = new MassUpdateFieldItemList()); }
		}
		MassUpdateFieldItemList matchingFieldList;

		[SuppressWeaklyTypedCollectionMessage]
		public IList FieldValueBindingList
		{
			get { return fieldValueBindingList; }
		}
		IList fieldValueBindingList;

		#region Modifier

		[List("ModifierList")]
		[ReadOnlyMember(nameof(IsModifierReadOnly))]
		[MaxLength(3)]
		public ZString Modifier
		{
			get => modifier;
			set
			{
				CheckMaximumLength(ModifierInfo, value);
				modifier = value;
				ModifierInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateModifier();
				}
			}
		}
		ZString modifier = MassUpdateModifierList.Codes.Overwrite;

		public ZPropertyInfo ModifierInfo => GetZPropertyInfo(nameof(Modifier));

		void ValidateModifier()
		{
			ModifierInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ModifierInfo);
			ListValidation.ErrorIfInvalidCode(ModifierInfo, ModifierList);
		}

		public MassUpdateModifierList ModifierList => Factory == null ? new MassUpdateModifierList() : Factory.GetCachedValue<MassUpdateModifierList>();

		public bool IsModifierReadOnly => Field.IsEmpty || !IsFieldNumeric();

		bool IsFieldNumeric()
		{
			var propertyType = FieldList[Field]?.propertyInfo.PropertyType;
			return propertyType == typeof(ZInt) || propertyType == typeof(ZDecimal);
		}

		void ResetModifier()
		{
			if (!IsFieldNumeric())
			{
				Modifier = MassUpdateModifierList.Codes.Overwrite;
			}
		}

		#endregion

		void ValidateNumericValue(ZPropertyInfo propertyInfo)
		{
			propertyInfo.ClearAllNotifications();
			if (Modifier != MassUpdateModifierList.Codes.Overwrite)
			{
				MandatoryValidation.CheckNotZero(propertyInfo);
			}
		}

		void ValidateNumericValue()
		{
			var propertyType = currentPropertyInfo?.PropertyType;
			if (propertyType == typeof(ZInt))
			{
				ValidateNumericValue(NewZIntValueInfo);
			}
			if (propertyType == typeof(ZDecimal))
			{
				ValidateNumericValue(NewZDecimalValueInfo);
			}
		}

		#region NewZGuidValue
		[List("FieldValueBindingList")]
		public ZGuid NewZGuidValue
		{
			get { return newZGuidValue; }
			set { SetNonPersistentPropertyValue(NewZGuidValueInfo, ref newZGuidValue, value); }
		}
		ZGuid newZGuidValue;

		public ZPropertyInfo NewZGuidValueInfo
		{
			get { return GetZPropertyInfo(Schema.NewZGuidValue); }
		}
		#endregion

		#region NewZDateTimeValue
		public ZDateTime NewZDateTimeValue
		{
			get { return newZDateTimeValue; }
			set { SetNonPersistentPropertyValue(NewZDateTimeValueInfo, ref newZDateTimeValue, value); }
		}
		ZDateTime newZDateTimeValue;

		public ZPropertyInfo NewZDateTimeValueInfo
		{
			get { return GetZPropertyInfo(Schema.NewZDateTimeValue); }
		}
		#endregion

		#region NewZDateTimeOffsetValue
		public ZDateTimeOffset NewZDateTimeOffsetValue
		{
			get { return newZDateTimeOffsetValue; }
			set { SetNonPersistentPropertyValue(NewZDateTimeOffsetValueInfo, ref newZDateTimeOffsetValue, value); }
		}
		ZDateTimeOffset newZDateTimeOffsetValue;

		public ZPropertyInfo NewZDateTimeOffsetValueInfo
		{
			get { return GetZPropertyInfo(Schema.NewZDateTimeOffsetValue); }
		}
		#endregion

		#region NewZDecimalValue
		public ZDecimal NewZDecimalValue
		{
			get => newZDecimalValue;
			set
			{
				SetNonPersistentPropertyValue(NewZDecimalValueInfo, ref newZDecimalValue, value);
				if (!IsValidationSuspended)
				{
					ValidateNumericValue(NewZDecimalValueInfo);
				}
			}
		}
		ZDecimal newZDecimalValue;

		public ZPropertyInfo NewZDecimalValueInfo => GetZPropertyInfo(Schema.NewZDecimalValue);
		#endregion

		#region NewZIntValue
		public ZInt NewZIntValue
		{
			get => newZIntValue;
			set
			{
				SetNonPersistentPropertyValue(NewZIntValueInfo, ref newZIntValue, value);
				if (!IsValidationSuspended)
				{
					ValidateNumericValue(NewZIntValueInfo);
				}
			}
		}
		ZInt newZIntValue;

		public ZPropertyInfo NewZIntValueInfo => GetZPropertyInfo(Schema.NewZIntValue);
		#endregion

		#region NewZShortValue
		public ZShort NewZShortValue
		{
			get { return newZShortValue; }
			set { SetNonPersistentPropertyValue(NewZShortValueInfo, ref newZShortValue, value); }
		}
		ZShort newZShortValue;

		public ZPropertyInfo NewZShortValueInfo
		{
			get { return GetZPropertyInfo(Schema.NewZShortValue); }
		}
		#endregion

		#region NewZBlobValue
		public ZBlob NewZBlobValue
		{
			get { return newZBlobValue; }
			set { SetNonPersistentPropertyValue(NewZBlobValueInfo, ref newZBlobValue, value); }
		}
		ZBlob newZBlobValue;

		public ZPropertyInfo NewZBlobValueInfo
		{
			get { return GetZPropertyInfo(Schema.NewZBlobValue); }
		}
		#endregion

		#region NewZByteValue
		public ZByte NewZByteValue
		{
			get { return newZByteValue; }
			set { SetNonPersistentPropertyValue(NewZByteValueInfo, ref newZByteValue, value); }
		}
		ZByte newZByteValue;

		public ZPropertyInfo NewZByteValueInfo
		{
			get { return GetZPropertyInfo(Schema.NewZByteValue); }
		}
		#endregion

		#region NewZStringValue
		[BusinessObjectMaxLengthTestExclude]
		[List("FieldValueBindingList")]
		public ZString NewZStringValue
		{
			get { return newZStringValue; }
			set { SetNonPersistentPropertyValue(NewZStringValueInfo, ref newZStringValue, value); }
		}
		ZString newZStringValue;

		public ZPropertyInfo NewZStringValueInfo
		{
			get { return GetZPropertyInfo(Schema.NewZStringValue); }
		}

		public int NewZStringValue_MaxLength
		{
			get { return newZStringValueMaxLength; }
		}

		#endregion

		#region NewZBoolValue
		public ZBool NewZBoolValue
		{
			get { return newZBoolValue; }
			set { SetNonPersistentPropertyValue(NewZBoolValueInfo, ref newZBoolValue, value); }
		}
		ZBool newZBoolValue;

		public ZPropertyInfo NewZBoolValueInfo
		{
			get { return GetZPropertyInfo(Schema.NewZBoolValue); }
		}
		#endregion

		#region NewZGeographyValue
		public ZGeography NewZGeographyValue
		{
			get { return newZGeographyValue; }
			set { SetNonPersistentPropertyValue(NewZGeographyValueInfo, ref newZGeographyValue, value); }
		}
		ZGeography newZGeographyValue;

		public ZPropertyInfo NewZGeographyValueInfo
		{
			get { return GetZPropertyInfo(Schema.NewZGeographyValue); }
		}
		#endregion

		#region NewZTimeValue
		public ZTime NewZTimeValue
		{
			get { return newZTimeValue; }
			set { SetNonPersistentPropertyValue(NewZTimeValueInfo, ref newZTimeValue, value); }
		}
		ZTime newZTimeValue;

		public ZPropertyInfo NewZTimeValueInfo
		{
			get { return GetZPropertyInfo(Schema.NewZTimeValue); }
		}
		#endregion

		#endregion

		#region BizObjsToUpdate

		public ImportWizardPreviewLineCollection BizObjsToUpdate
		{
			get { return bizObjsToUpdate; }
		}
		ImportWizardPreviewLineCollection bizObjsToUpdate;

		#endregion

		#region Matching Filters

		[ChildEditable]
		public MassUpdateMatchingFilterCollection MatchingFilters
		{
			get
			{
				if (matchingFilters == null)
				{
					matchingFilters = new MassUpdateMatchingFilterCollection(this);
					RegisterEditableChildObject(matchingFilters);
				}
				return matchingFilters;
			}
		}
		MassUpdateMatchingFilterCollection matchingFilters;

		#endregion

		public ZString FieldMappingName
		{
			get { return currentPropertyInfo == null ? "" : currentPropertyInfo.MappingName; }
		}
		IImportPropertyInfo currentPropertyInfo;

		[SuppressWeaklyTypedCollectionMessage]
		public IList GetBindingList(ZString field)
		{
			IList result = null;
			bindingLists.TryGetValue(field, out result);
			return result;
		}

		public ModuleIdentifier GetModuleId(ZString field)
		{
			ModuleIdentifier result = null;
			moduleIdLists.TryGetValue(field, out result);
			return result;
		}

		public FieldType GetFieldType(ZString field)
		{
			FieldType result;
			if (!fieldTypeLists.TryGetValue(field, out result))
			{
				result = FieldType.Text;
			}
			return result;
		}

		public int GetMaxLength(ZString field)
		{
			int result;
			if (!stringPropertyMaxLengthList.TryGetValue(field, out result))
			{
				result = -1;
			}
			return result;
		}

		public ZString GetBindToField(Type propertyType)
		{
			ZPropertyInfo bindToPropertyInfo = NewZStringValueInfo;
			if (propertyType != null)
			{
				if (ImportWizard.IsType(propertyType, typeof(ZGuid)))
				{
					bindToPropertyInfo = NewZGuidValueInfo;
				}
				else if (ImportWizard.IsType(propertyType, typeof(ZDate)) || ImportWizard.IsType(propertyType, typeof(ZDateTime)))
				{
					bindToPropertyInfo = NewZDateTimeValueInfo;
				}
				else if (ImportWizard.IsType(propertyType, typeof(ZDateTimeOffset)))
				{
					bindToPropertyInfo = NewZDateTimeOffsetValueInfo;
				}
				else if (ImportWizard.IsType(propertyType, typeof(ZTime)))
				{
					bindToPropertyInfo = NewZTimeValueInfo;
				}
				else if (ImportWizard.IsType(propertyType, typeof(ZDecimal)))
				{
					bindToPropertyInfo = NewZDecimalValueInfo;
				}
				else if (ImportWizard.IsType(propertyType, typeof(ZByte)))
				{
					bindToPropertyInfo = NewZByteValueInfo;
				}
				else if (ImportWizard.IsType(propertyType, typeof(ZInt)))
				{
					bindToPropertyInfo = NewZIntValueInfo;
				}
				else if (ImportWizard.IsType(propertyType, typeof(ZShort)))
				{
					bindToPropertyInfo = NewZShortValueInfo;
				}
				else if (ImportWizard.IsType(propertyType, typeof(ZBool)))
				{
					bindToPropertyInfo = NewZBoolValueInfo;
				}
				else if (ImportWizard.IsType(propertyType, typeof(ZGeography)))
				{
					bindToPropertyInfo = NewZGeographyValueInfo;
				}
			}
			return bindToPropertyInfo.Name;
		}

		public void LoadBizObjsToUpdate()
		{
			BizObjsToUpdate.RemoveAll();
			var filterLookup = ConstructFilterLookup();
			foreach (BusinessObject bizObj in collectionInfo.Collection)
			{
				if (IsMatchingFilters(bizObj, filterLookup))
				{
					AddBizObjToUpdate(BizObjsToUpdate, bizObj);
				}
			}
		}

		internal struct MassUpdateMatchingFilterKey
		{
			public MassUpdateMatchingFilterKey(ZString fieldMappingName, ZString @operator)
			{
				this.FieldMappingName = fieldMappingName;
				this.Operator = @operator;
			}

			public ZString FieldMappingName;
			public ZString Operator;
		}

		internal Dictionary<MassUpdateMatchingFilterKey, List<IZType>> ConstructFilterLookup()
		{
			var result = new Dictionary<MassUpdateMatchingFilterKey, List<IZType>>();
			foreach (MassUpdateMatchingFilter filter in MatchingFilters)
			{
				var key = new MassUpdateMatchingFilterKey(filter.FieldMappingName, filter.Operator);
				if (!result.ContainsKey(key))
				{
					result[key] = new List<IZType>();
				}
				result[key].Add(filter.FieldValueInIZType);
			}
			return result;
		}

		public void Update()
		{
			RunPreSaveValidation();
			if (!HasErrors)
			{
				if (currentPropertyInfo != null)
				{
					int totalMatched = 0;
					int totalUpdated = 0;
					int totalNotUpdated = 0;
					string mappingName = currentPropertyInfo.MappingName;
					Type actualPropertyType = currentPropertyInfo.PropertyType;

					using (collectionInfo.Collection.SuspendListChanged())
					{
						var filterLookup = ConstructFilterLookup();

						foreach (BusinessObject bizObj in collectionInfo.Collection.ToArray())
						{
							if (bizObj.HasPossiblyCustomProperty(mappingName) && IsMatchingFilters(bizObj, filterLookup))
							{
								totalMatched++;
								if (!bizObj.GetPossiblyCustomPropertyReadOnly(mappingName))
								{
									object value = GetPropertyValueAfterModifier(bizObj, mappingName);
									if (value.ToString() == bizObj.GetPossiblyCustomProperty(mappingName).ToString() || value.Equals(bizObj.GetPossiblyCustomProperty(mappingName)))
									{
										continue;
									}
									if (currentPropertyInfo.IsMultiControl && value != null)
									{
										if (actualPropertyType == typeof(ZString))
										{
											value = (ZString)value.ToString();
										}
									}
									totalUpdated++;

									if (value is ZString zStringValue)
									{
										int maxLength = bizObj.GetPossiblyCustomPropertyMaxLength(mappingName);
										if (zStringValue.Length > maxLength && maxLength > 0)
										{
											value = zStringValue.Substring(0, maxLength);
										}
									}
									bizObj.SetPossiblyCustomProperty(mappingName, value);
								}
								else
								{
									totalNotUpdated++;
								}
							}
						}
					}

					var message = Res.GetString("a7d33de4-a384-4ed2-8314-041273c43814", "{0} record(s) matched; {1} were updated; {2} were 'Read Only'.", totalMatched, totalUpdated, totalNotUpdated);
					Globals.Message.Show(message, Res.GetString("7e54c4a1-d728-4628-b6d2-f38f94a6361f", "Update Result"), ZMessageBoxButtons.OK, ZMessageBoxIcon.Information);
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("e78ed915-e950-4d90-a967-4b4cf020e97d", "Cannot Update till all errors are fixed."));
			}
		}

		#region Implementation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateField();
			ValidateNumericValue();
		}

		bool IsMatchingFilters(BusinessObject bizObj, Dictionary<MassUpdateMatchingFilterKey, List<IZType>> filterLookup)
		{
			bool result = true;

			foreach (var key in filterLookup.Keys)
			{
				ZString mappingName = key.FieldMappingName;

				if (bizObj.HasPossiblyCustomProperty(mappingName))
				{
					object value = bizObj.GetPossiblyCustomProperty(mappingName);
					var isNumeric = ZDecimal.TryParse(value.ToString(), out ZDecimal numericValue);
					switch (key.Operator)
					{
						case MassUpdateMatchingFilterOperatorList.Codes.Equal:
							result = filterLookup[key].Any(x => x.Equals(value));
							break;
						case MassUpdateMatchingFilterOperatorList.Codes.NotEqualTo:
							result = !filterLookup[key].Any(x => x.Equals(value));
							break;
						case MassUpdateMatchingFilterOperatorList.Codes.Contains:
							result = filterLookup[key].Any(x => x is ZString s && value.ToString().Contains(s, StringComparison.OrdinalIgnoreCase));
							break;
						case MassUpdateMatchingFilterOperatorList.Codes.GreaterThan:
							result = isNumeric && filterLookup[key].Any(x => x is INumericZType targetValue && numericValue.CompareTo(new ZDecimal(targetValue)) > 0);
							break;
						case MassUpdateMatchingFilterOperatorList.Codes.LessThan:
							result = isNumeric && filterLookup[key].Any(x => x is INumericZType targetValue && numericValue.CompareTo(new ZDecimal(targetValue)) < 0);
							break;
					}
					if (!result)
					{
						break;
					}
				}
			}
			return result;
		}

		object GetPropertyValueAfterModifier(BusinessObject bizObj, string propertyName)
		{
			var sourceValue = bizObj.GetPossiblyCustomProperty(propertyName);
			var result = sourceValue;
			if (propertyName == currentPropertyInfo?.MappingName)
			{
				var propertyType = propertyTypeLists[propertyName];
				var targetValue = sourceValue;
				if (currentPropertyInfo.IsMultiControl)
				{
					if (propertyType == currentPropertyInfo.GetExpectedTypeForMultiControl(bizObj))
					{
						targetValue = this[GetBindToField(propertyType)];
					}
				}
				else
				{
					targetValue = this[GetBindToField(propertyType)];
				}

				result = sourceValue is INumericZType numericValue ? CalculateValue(numericValue, Modifier, targetValue) : targetValue;
			}
			return result;
		}

		void AddBizObjToUpdate(ImportWizardPreviewLineCollection bizObjsToUpdate, BusinessObject bizObj)
		{
			ImportWizardPreviewLine bizObjToUpdate = bizObjsToUpdate.AddNew();

			foreach (IImportPropertyInfo property in CollectionInfo.Properties)
			{
				string mappingName = property.MappingName;
				if (bizObj.HasPossiblyCustomProperty(mappingName))
				{
					bizObjToUpdate[property.MappingName] = GetPropertyValueAfterModifier(bizObj, mappingName);
				}

				var fieldTypeColumnName = property.FieldTypeColumnName;
				if (fieldTypeColumnName.IsNullOrEmpty())
				{
					continue;
				}

				AddFieldTypeColumnNameAndValue(bizObj, bizObjToUpdate, fieldTypeColumnName);
			}
		}

		void AddFieldTypeColumnNameAndValue(BusinessObject bizObj, ImportWizardPreviewLine bizObjToUpdate, string fieldTypeColumnName)
		{
			var value = GetFieldType(bizObj, fieldTypeColumnName);

			if (value != null)
			{
				bizObjToUpdate[fieldTypeColumnName] = value.ToString();
			}
			else
			{
				bizObjToUpdate[fieldTypeColumnName] = nameof(FieldType.Text);
			}
		}

		static object GetFieldType(BusinessObject bizObj, string fieldTypeColumnName)
		{
			object value;

			if (fieldTypeColumnName.Contains("."))
			{
				value = ProcessCascadingFieldTypes(bizObj, fieldTypeColumnName.Split('.'));
			}
			else if (fieldTypeColumnName.Contains("+"))
			{
				value = ProcessCascadingFieldTypes(bizObj, fieldTypeColumnName.Split('+'));
			}
			else
			{
				value = bizObj.GetType().GetProperty(fieldTypeColumnName)?.GetValue(bizObj);
			}

			return value;
		}

		static object ProcessCascadingFieldTypes(BusinessObject bizObj, string[] fieldTypes)
		{
			object currentValue = bizObj;

			for (var i = 0; i < fieldTypes.Length; i++)
			{
				if (currentValue == null)
				{
					break;
				}
				currentValue = currentValue.GetType().GetProperty(fieldTypes[i])?.GetValue(currentValue);

				if (i == fieldTypes.Length - 1)
				{
					return currentValue;
				}
			}

			return null;
		}

		void LoadFieldDetails()
		{
			IBindingList list = CollectionInfo.Collection;
			if (list != null)
			{
				stringPropertyMaxLengthList = new Dictionary<string, int>();
				bindingLists = new Dictionary<string, IList>();
				moduleIdLists = new Dictionary<string, ModuleIdentifier>();
				fieldTypeLists = new Dictionary<string, FieldType>();
				propertyTypeLists = new Dictionary<string, Type>();
				BusinessObject bizObj = list.Count > 0 ? list[0] as BusinessObject : null;
				if (bizObj != null)
				{
					foreach (IImportPropertyInfo property in CollectionInfo.Properties)
					{
						Type propertyType = property.IsMultiControl ? property.GetExpectedTypeForMultiControl(bizObj) : property.PropertyType;
						string mappingName = property.MappingName;
						MatchingFieldList.Add(property);
						IList bindToList = property.GetBindToList(bizObj);
						if (bindToList != null)
						{
							bindingLists.Add(mappingName, bindToList);
						}
						ModuleIdentifier moduleID = property.GetModuleID(bizObj);
						if (moduleID != null)
						{
							moduleIdLists.Add(mappingName, moduleID);
						}

						if (!property.IsReadOnly)
						{
							propertyTypeLists.Add(mappingName, propertyType);
							FieldList.Add(property);
						}

						int maxLength = -1;
						if (typeof(ZString).IsAssignableFrom(propertyType))
						{
							if (bizObj.HasPossiblyCustomProperty(mappingName))
							{
								maxLength = bizObj.GetPossiblyCustomPropertyMaxLength(mappingName);
							}
							stringPropertyMaxLengthList.Add(mappingName, maxLength);
						}
						fieldTypeLists.Add(mappingName, CalculateFieldType(bindToList, moduleID, propertyType, maxLength));
					}
					bizObjsToUpdate = new ImportWizardPreviewLineCollection(CollectionInfo.Properties, bindingLists);
					bizObjsToUpdate.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(bizObjsToUpdate);
					if (FieldList.Count > 0)
					{
						fieldMaxLength = FieldList.Cast<ICodeDescription>().Max(x => x.Code.Length);
					}
					fieldMaxLength = Math.Max(fieldMaxLength, 68);
				}
			}
		}
		Dictionary<string, ModuleIdentifier> moduleIdLists;
		Dictionary<string, IList> bindingLists;
		Dictionary<string, int> stringPropertyMaxLengthList;
		public Dictionary<string, Type> propertyTypeLists;
		Dictionary<string, FieldType> fieldTypeLists;

		FieldType CalculateFieldType(IList bindingList, ModuleIdentifier moduleID, Type propertyType, int maxLength)
		{
			FieldType result = FieldType.Text;
			if (ImportWizard.IsType(propertyType, typeof(ZString)))
			{
				if (bindingList == null)
				{
					result = maxLength > 128 ? FieldType.TextMultiLine : FieldType.Text;
				}
				else if (moduleID != null && moduleID != ModuleIDs.NotAssigned)
				{
					result = FieldType.TextCodeFindBox;
				}
				else
				{
					result = FieldType.TextDropEdit;
				}
			}
			else if (ImportWizard.IsType(propertyType, typeof(ZGuid)))
			{
				if (moduleID != null && moduleID != ModuleIDs.NotAssigned)
				{
					result = FieldType.Guid;
				}
				else
				{
					result = FieldType.GuidDropEdit;
				}
			}
			else if (ImportWizard.IsType(propertyType, typeof(ZDate)))
			{
				result = FieldType.Date;
			}
			else if (ImportWizard.IsType(propertyType, typeof(ZDateTime)))
			{
				result = FieldType.DateTime;
			}
			else if (ImportWizard.IsType(propertyType, typeof(ZDateTimeOffset)))
			{
				result = FieldType.DateTimeOffset;
			}
			else if (ImportWizard.IsType(propertyType, typeof(ZDecimal)))
			{
				result = FieldType.Decimal;
			}
			else if (ImportWizard.IsType(propertyType, typeof(ZByte)))
			{
				result = FieldType.Byte;
			}
			else if (ImportWizard.IsType(propertyType, typeof(ZInt)) || ImportWizard.IsType(propertyType, typeof(ZShort)))
			{
				result = FieldType.Integer;
			}
			else if (ImportWizard.IsType(propertyType, typeof(ZBool)))
			{
				result = FieldType.Boolean;
			}
			else if (ImportWizard.IsType(propertyType, typeof(ZGeography)))
			{
				result = FieldType.Geography;
			}
			else if (ImportWizard.IsType(propertyType, typeof(ZTime)))
			{
				result = FieldType.Time;
			}
			return result;
		}

		void UpdateFieldChange()
		{
			MassUpdateFieldItem fieldItem = FieldList[Field];
			currentPropertyInfo = fieldItem?.propertyInfo;
			fieldValueBindingList = null;
			newZStringValueMaxLength = -1;
			if (currentPropertyInfo != null)
			{
				fieldValueBindingList = GetBindingList(currentPropertyInfo.MappingName);
				if (ImportWizard.IsType(currentPropertyInfo.PropertyType, typeof(ZString)))
				{
					var maxLength = GetMaxLength(currentPropertyInfo.MappingName);
					newZStringValueMaxLength = maxLength > 0 ? maxLength : -1;

					if (NewZStringValue.Length > newZStringValueMaxLength)
					{
						NewZStringValue = NewZStringValue.Left(newZStringValueMaxLength);
					}
				}
				ResetModifier();
			}
		}
		int newZStringValueMaxLength;

		INumericZType CalculateValue(INumericZType inputValue, string modifier, object operand)
		{
			var result = inputValue;
			switch (inputValue)
			{
				case ZInt @int:
					result = CalculateValue((ZDecimal)@int, modifier, operand).Round(0).ToZInt();
					break;
				case ZDecimal @decimal:
					result = CalculateValue(@decimal, modifier, operand);
					break;
			}
			return result;
		}

		ZDecimal CalculateValue(ZDecimal inputValue, string modifier, object operand)
		{
			var result = inputValue;
			if (ZDecimal.TryParse(operand.ToString(), out ZDecimal op))
			{
				switch (modifier)
				{
					case MassUpdateModifierList.Codes.Add:
						result += op;
						break;
					case MassUpdateModifierList.Codes.Subtract:
						result -= op;
						break;
					case MassUpdateModifierList.Codes.Multiply:
						result *= op;
						break;
					case MassUpdateModifierList.Codes.Divide:
						if (op != 0)
						{
							result /= op;
						}
						break;
					case MassUpdateModifierList.Codes.Overwrite:
						result = op;
						break;
				}
			}
			return result;
		}

		#endregion
	}
}
