using System;
using System.Collections;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using ResString = Enterprise.ZArchitecture.Business.ResString;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class MassUpdateMatchingFilter : NonPersistentBusinessObject, IObsoleteValidation, IModuleIDProvider
	{
		public MassUpdateMatchingFilter(MassUpdateWizard wizard)
			: base(wizard.Factory)
		{
			this.wizard = wizard;
			UpdateFieldChange();
		}

		public static class Schema
		{
			public const string Field = "Field";
			public const string Operator = "Operator";
			public const string FieldValue = "FieldValue";
			public const string FieldValueType = "FieldValueType";
		}

		#region Properties

		#region Field

		[List("FieldList")]
		[MaxLength(200)]
		public ZString Field
		{
			get { return field; }
			set
			{
				ZString oldValue = field;
				CheckMaximumLength(FieldInfo, value);
				field = value;
				if (oldValue != field)
				{
					UpdateFieldChange();
					FieldInfo.RefreshBinding(oldValue);
				}
				else
				{
					FieldInfo.RefreshBinding();
				}
				if (!IsValidationSuspended)
				{
					ValidateField();
				}
			}
		}
		ZString field;

		public ZPropertyInfo FieldInfo
		{
			get { return GetZPropertyInfo(Schema.Field); }
		}

		public void ValidateField()
		{
			FieldInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(FieldInfo);
			ListValidation.ErrorIfInvalidCode(FieldInfo, FieldList, ResString.GetMultilingualString("d2ca3f27-a00d-4a4a-9d51-8864f78d15b3", "Matching Field"));
		}

		public MassUpdateFieldItemList FieldList
		{
			get { return wizard.MatchingFieldList; }
		}

		#endregion

		#region Operator

		[List("OperatorList")]
		[MaxLength(3)]
		public ZString Operator
		{
			get { return fOperator; }
			set
			{
				CheckMaximumLength(OperatorInfo, value);
				fOperator = value;
				OperatorInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateOperator();
				}
			}
		}
		ZString fOperator = MassUpdateMatchingFilterOperatorList.Codes.Equal;

		public ZPropertyInfo OperatorInfo
		{
			get { return GetZPropertyInfo(Schema.Operator); }
		}

		public void ValidateOperator()
		{
			OperatorInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(OperatorInfo);
			ListValidation.ErrorIfInvalidCode(OperatorInfo, OperatorList);
		}

		public MassUpdateMatchingFilterOperatorList OperatorList
		{
			get
			{
				var propertyType = FieldList[Field]?.propertyInfo.PropertyType ?? typeof(object);
				return Factory == null
					? new MassUpdateMatchingFilterOperatorList(propertyType)
					: Factory.GetCachedValue(propertyType, () => new MassUpdateMatchingFilterOperatorList(propertyType));
			}
		}

		#endregion

		#region FieldValue

		public IZType FieldValueInIZType
		{
			get
			{
				if (fieldValueInIZType == null)
				{
					fieldValueInIZType = FieldValue;
					if (currentPropertyInfo != null)
					{
						Type propertyType = currentPropertyInfo.PropertyType;
						try
						{
							fieldValueInIZType = ZDataType.ObjectToZType(propertyType, FieldValue);
						}
						catch (FormatException)
						{
							fieldValueInIZType = ZDataType.ZTypeToEmptyValue(propertyType);
						}
						catch (ZTypeValueException)
						{
							fieldValueInIZType = ZDataType.ZTypeToEmptyValue(propertyType);
						}
						catch (OverflowException)
						{
							fieldValueInIZType = ZDataType.ZTypeToEmptyValue(propertyType);
						}
					}
				}
				return fieldValueInIZType;
			}
		}
		IZType fieldValueInIZType;

		[BusinessObjectMaxLengthTestExclude]
		public ZString FieldValue
		{
			get { return fieldValue; }
			set
			{
				if (fieldValue != value)
				{
					ZString oldValue = fieldValue;
					CheckMaximumLength(FieldValueInfo, value);
					fieldValue = value;
					fieldValueInIZType = null;
					FieldValueInfo.RefreshBinding(oldValue);
				}
			}
		}
		ZString fieldValue;

		public ZPropertyInfo FieldValueInfo
		{
			get { return GetZPropertyInfo(Schema.FieldValue); }
		}
		int fieldValueMaxLength;

		public int FieldValue_MaxLength
		{
			get { return fieldValueMaxLength; }
		}

		#endregion

		#region FieldValueType

		public ZString FieldValueType
		{
			get
			{
				if (!fieldValueType.HasValue)
				{
					CalculateFieldValuteType();
				}
				return fieldValueType.Value;
			}
		}
		ZString? fieldValueType;

		public ZPropertyInfo FieldValueTypeInfo
		{
			get { return GetZPropertyInfo(Schema.FieldValueType); }
		}

		#endregion

		#region moduleID

		public ModuleIdentifier ModuleID => wizard.GetModuleId(FieldMappingName) ?? ModuleIDs.NotAssigned;

		#endregion

		#endregion

		[SuppressWeaklyTypedCollectionMessage]
		public IList FieldValueBindingList
		{
			get { return fieldValueBindingList; }
		}
		IList fieldValueBindingList;

		public ZString FieldMappingName
		{
			get { return currentPropertyInfo == null ? "" : currentPropertyInfo.MappingName; }
		}
		IImportPropertyInfo currentPropertyInfo;

		#region Implementation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateField();
			ValidateOperator();
		}

		void CalculateFieldValuteType()
		{
			FieldType result = FieldType.Text;
			if (currentPropertyInfo != null)
			{
				result = wizard.GetFieldType(currentPropertyInfo.MappingName);
			}
			fieldValueType = result.ToString();
		}

		void UpdateFieldChange()
		{
			fieldValue = ZString.Empty;
			fieldValueInIZType = null;
			fieldValueMaxLength = -1;
			fieldValueBindingList = null;
			MassUpdateFieldItem fieldItem = FieldList[Field];
			currentPropertyInfo = fieldItem == null ? null : fieldItem.propertyInfo;
			if (currentPropertyInfo != null)
			{
				var maxLength = currentPropertyInfo == null ? -1 : wizard.GetMaxLength(currentPropertyInfo.MappingName);
				fieldValueMaxLength = maxLength > 0 ? maxLength : -1;
				fieldValueBindingList = wizard.GetBindingList(currentPropertyInfo.MappingName);
			}

			CalculateFieldValuteType();
		}
		readonly MassUpdateWizard wizard;

		#endregion
	}
}
