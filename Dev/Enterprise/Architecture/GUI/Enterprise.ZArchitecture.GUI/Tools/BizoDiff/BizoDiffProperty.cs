using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.DevTools
{
	public sealed class BizoProperty : NonPersistentBusinessObject, IObsoleteValidation
	{
		public BizoProperty(ZString propertyName, object propertyValue)
		{
			Name = propertyName;
			PropertyValueObject = propertyValue;
		}

		public BizoProperty(ZString propertyName, object propertyValue, object comparePropertyValue)
		{
			Name = propertyName;
			PropertyValueObject = propertyValue;
			ComparePropertyValueObject = comparePropertyValue;
			NeedCompare = true;
			Validation.ValidateAll();
		}

		bool IsKeyField;
		bool IsIgnoreField;
		readonly object PropertyValueObject;
		readonly object ComparePropertyValueObject;

		public event EventHandler IsKeyFieldChanged;
		public event EventHandler IsIgnoreFieldChanged;

		public ZString Name { get; }

		public ZString Value => PropertyValueObject == default ? "[EMPTY]" : PropertyValueObject.ToString();

		public ZBool NeedCompare { get; }

		public ZPropertyInfo ValueInfo
		{
			get { return GetZPropertyInfo(nameof(Value)); }
		}

		public ZBool KeyField
		{
			get
			{
				return IsKeyField;
			}

			set
			{
				IsKeyField = value;
				IsKeyFieldChanged?.Invoke(this, EventArgs.Empty);
				KeyFieldInfo.RefreshBinding();
			}
		}

		public ZBool IgnoreField
		{
			get
			{
				return IsIgnoreField;
			}

			set
			{
				IsIgnoreField = value;
				IsIgnoreFieldChanged?.Invoke(this, EventArgs.Empty);
				IgnoreFieldInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo KeyFieldInfo
		{
			get { return GetZPropertyInfo(nameof(KeyField)); }
		}

		public ZPropertyInfo IgnoreFieldInfo
		{
			get { return GetZPropertyInfo(nameof(IgnoreField)); }
		}

		public ZBool IsSameValue => (PropertyValueObject == null && ComparePropertyValueObject == null)
									|| (PropertyValueObject != null && PropertyValueObject.Equals(ComparePropertyValueObject));

		public BizoPropertyValidation Validation => validation ??= new BizoPropertyValidation(this);
		BizoPropertyValidation validation;
	}
}
