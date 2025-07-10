using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.ZArchitecture.GUI.Testing.TextTemplateFormTest;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class GridTestFieldBusinessObject : NonPersistentBusinessObject, IObsoleteValidation, INotifyPropertyUpdated, IRootTypeProvider, IModuleIDProvider
	{
		public GridTestFieldBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static readonly Overridable<Type[]> RootTypes_Override = new Overridable<Type[]>(null);

		Type[] IRootTypeProvider.RootTypes
		{
			get => RootTypes_Override.Value ?? new Type[] { typeof(DummyBusinessObjectWithIRootTypeProvider), typeof(DummyBusinessObject) };
		}

		BusinessObject[] IRootTypeProvider.Roots
		{
			get => null;
		}

		public event EventHandler<PropertyUpdatedEventArgs> PropertyUpdated = (o, e) => { };

		public ZInt Decimal { get; set; }

		#region FieldName

		public virtual ZString FieldName
		{
			get { return fieldName; }
			set
			{
				fieldName = value;
				HasChanges = true;
				CheckMaximumLength(FieldNameInfo, value);

				if (!IsValidationSuspended)
				{
					ValidateFieldName();
				}
			}
		}

		ZString fieldName;

		public virtual void ValidateFieldName()
		{
			FieldNameInfo.ClearAllNotifications();
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(FieldNameInfo);
		}

		public virtual ZPropertyInfo FieldNameInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(FieldName)); }
		}

		#endregion

		#region FieldValue

		public virtual ZString FieldValue
		{
			get { return fieldValue; }
			set
			{
				fieldValue = value;
				HasChanges = true;
				CheckMaximumLength(FieldValueInfo, value);

				if (!IsValidationSuspended)
				{
					ValidateFieldValue();
				}
			}
		}

		ZString fieldValue;

		public virtual void ValidateFieldValue()
		{
			FieldValueInfo.ClearAllNotifications();
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(FieldValueInfo);
		}

		public virtual ZPropertyInfo FieldValueInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(FieldValue)); }
		}

		#endregion

		#region FieldType

		public virtual ZString FieldType
		{
			get { return fieldType; }
			set
			{
				fieldType = value;
				HasChanges = true;
				CheckMaximumLength(FieldTypeInfo, value);

				if (!IsValidationSuspended)
				{
					ValidateFieldType();
				}

				PropertyUpdated(this, new PropertyUpdatedEventArgs(nameof(FieldType)));
			}
		}

		ZString fieldType;

		public virtual void ValidateFieldType()
		{
			FieldTypeInfo.ClearAllNotifications();
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(FieldTypeInfo);
		}

		public virtual ZPropertyInfo FieldTypeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(FieldType)); }
		}

		#endregion

		#region ModuleId

		public ModuleIdentifier ModuleID { get; set; }

		#endregion

		public ICollection TestList
		{
			get
			{
				if (testList == null)
				{
					var dummies = new DummyBusinessObjectCollection(Factory);
					dummies.Load();

					testList = dummies;
				}
				return testList;
			}

			set { testList = value; }
		}

		ICollection testList;
	}
}
