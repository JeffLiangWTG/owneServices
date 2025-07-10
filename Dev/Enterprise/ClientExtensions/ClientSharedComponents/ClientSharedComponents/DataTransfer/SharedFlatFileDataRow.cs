using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ClientSharedComponents
{
	public abstract class SharedFlatFileDataRow : FlatFileDataRow, ICloneable
	{
		public SharedFlatFileDataRow()
		{
			SetFieldAttributes();
			DataRow = new string[FieldCount];
			PopulateProperties();
		}

		public SharedFlatFileDataRow(string fixedWidthDataLine)
		{
			SetFieldAttributes();
			DataRow = new string[FieldCount];
			PopulateProperties(fixedWidthDataLine);
		}

		public SharedFlatFileDataRow(string[] dataRow)
		{
			SetFieldAttributes();
			this.DataRow = dataRow;
			PopulateProperties();
		}

		public SharedFlatFileDataRow(int fieldCount) : base(fieldCount)
		{
			maxPosition = fieldCount;
			SetFieldAttributes();
			fieldProperties = new List<FlatFileFieldProperty>();
			AddFieldProperties();
			PopulateProperties();
		}

		public SharedFlatFileDataRow(FlatFileDataRow dataRow) : base(dataRow)
		{
			fieldProperties = new List<FlatFileFieldProperty>();
			AddFieldProperties();
			SetFieldAttributes();
			PopulateProperties();
		}

		public IReadOnlyList<string> Fields
		{
			get { return DataRow; }
		}

		#region Overrides
		#region SetField overrides
		public void SetField(FlatFileFieldProperty fieldProperty, ZString value)
		{
			base.SetField(fieldProperty.Name, value.Left(fieldProperty.Length));
		}

		public void SetField(FlatFileFieldProperty fieldProperty, ZDecimal value)
		{
			this.SetField(fieldProperty.Name, value, 2);
		}

		/// <summary>
		/// Store a decimal value as a decimal string.
		/// </summary>
		/// <param name="fieldProperty">Field Position (and length).</param>
		/// <param name="value">Decimal value.</param>
		/// <param name="decimalPlaces">Decimal places used for rounding.</param>
		public void SetField(FlatFileFieldProperty fieldProperty, ZDecimal value, int decimalPlaces)
		{
			if (decimalPlaces < 0 || decimalPlaces > 10)
			{
				throw new ArgumentException("Decimal places must be between 0 and 10 inclusive.", nameof(decimalPlaces));
			}
			if (!ZeroFillAllNumericFields)
			{
				base.SetField(fieldProperty.Name, value, decimalPlaces);
			}
			else
			{
				string format = new string(zero, fieldProperty.Length - decimalPlaces - 1) + "." + new string(zero, decimalPlaces);
				base.SetField(fieldProperty.Name, Utilities.Round(value, decimalPlaces).ToString(format));
			}
		}

		public void SetZDecimalFieldAsInt(FlatFileFieldProperty fieldProperty, ZDecimal value, int decimalPlaces)
		{
			if (decimalPlaces < 0 || decimalPlaces > 10)
			{
				throw new ArgumentException("Decimal places must be between 0 and 10 inclusive.", nameof(decimalPlaces));
			}
			SetField(fieldProperty, ConvertDecimalToInt(value, decimalPlaces));
		}

		public void SetField(FlatFileFieldProperty fieldProperty, ZDateTime value)
		{
			base.SetField(fieldProperty.Name, value, DataDateFormat);
		}

		public void SetField(FlatFileFieldProperty fieldProperty, ZDateTime value, string dataDateTimeFormat)
		{
			base.SetField(fieldProperty.Name, value, dataDateTimeFormat);
		}

		public void SetField(FlatFileFieldProperty fieldProperty, ZInt value)
		{
			SetField(fieldProperty, (int)value);
		}

		public void SetField(FlatFileFieldProperty fieldProperty, int value)
		{
			if (!ZeroFillAllNumericFields)
			{
				base.SetField(fieldProperty.Name, value);
			}
			else
			{
				base.SetField(fieldProperty.Name, value.ToString(new string(zero, fieldProperty.Length)));
			}
		}

		public void SetField(FlatFileFieldProperty fieldProperty, ZBool value)
		{
			base.SetField(fieldProperty.Name, value ? "Y" : "N");
		}

		#endregion

		int ConvertDecimalToInt(ZDecimal decimalValue, int decimalPlaces)
		{
			if (decimalPlaces < 0 || decimalPlaces > 10)
			{
				throw new ArgumentException("Decimal places must be between 0 and 10 inclusive.", nameof(decimalPlaces));
			}
			return Convert.ToInt32(decimalValue * (decimal)(Math.Pow(10, decimalPlaces)));
		}

		decimal ConvertIntToDecimal(ZInt integetValue, int decimalPlaces)
		{
			if (decimalPlaces < 0 || decimalPlaces > 10)
			{
				throw new ArgumentException("Decimal places must be between 0 and 10 inclusive.", nameof(decimalPlaces));
			}
			return Convert.ToDecimal(integetValue / (decimal)(Math.Pow(10, decimalPlaces)));
		}

		#region GetField overrides

		public ZString GetZDateTimeFieldAsString(FlatFileFieldProperty fieldProperty, ZString format)
		{
			if (format.IsEmpty)
			{
				throw new ArgumentException("Format cannot be empty.", nameof(format));
			}
			return GetFieldAsZDateTime(fieldProperty, format).ToString(format);
		}

		public ZString GetField(FlatFileFieldProperty fieldProperty)
		{
			return base.GetField(fieldProperty.Name);
		}

		public ZBool GetFieldAsZBool(FlatFileFieldProperty fieldProperty)
		{
			return new ZBool(base.GetField(fieldProperty.Name).ToUpper() == "Y");
		}

		public ZDateTime GetFieldAsZDateTime(FlatFileFieldProperty fieldProperty)
		{
			return base.GetFieldAsZDateTime(fieldProperty.Name, DataDateFormat);
		}

		public ZDateTime GetFieldAsZDateTime(FlatFileFieldProperty fieldProperty, ZString format)
		{
			return base.GetFieldAsZDateTime(fieldProperty.Name, format);
		}

		public ZDecimal GetFieldAsZDecimal(FlatFileFieldProperty fieldProperty, int decimalPlaces)
		{
			if (decimalPlaces < 0 || decimalPlaces > 10)
			{
				throw new ArgumentException("Decimal places must be between 0 and 10 inclusive.", nameof(decimalPlaces));
			}
			return base.GetFieldAsZDecimal(fieldProperty.Name, decimalPlaces);
		}

		public ZDecimal GetIntFieldAsZDecimal(FlatFileFieldProperty fieldProperty, int decimalPlaces)
		{
			if (decimalPlaces < 0 || decimalPlaces > 10)
			{
				throw new ArgumentException("Decimal places must be between 0 and 10 inclusive.", nameof(decimalPlaces));
			}
			return ConvertIntToDecimal(base.GetFieldAsZInt(fieldProperty.Name), decimalPlaces);
		}

		public ZDecimal GetFieldAsZDecimal(FlatFileFieldProperty fieldProperty)
		{
			return base.GetFieldAsZDecimal(fieldProperty.Name);
		}

		public ZInt GetFieldAsZInt(FlatFileFieldProperty fieldProperty)
		{
			return base.GetFieldAsZInt(fieldProperty.Name);
		}

		#endregion

		#endregion

		public virtual int Length
		{
			get
			{
				int length = 0;
				if (InternalPropertyFieldList.Count == 0)
				{
					foreach (FlatFileFieldProperty fieldProperty in FieldProperties)
					{
						length += fieldProperty.Length;
					}
				}
				else
				{
					InternalPropertyFieldList.ForEach(f => length += f.Attribute.Length);
				}
				return length;
			}
		}

		public virtual string DataDateFormat
		{
			get { return defaultDataFormat; }
		}
		const string defaultDataFormat = "dd/MMM/yyyy";

		public List<FlatFileFieldProperty> FieldProperties
		{
			get { return fieldProperties; }
			protected set { fieldProperties = value; }
		}
		List<FlatFileFieldProperty> fieldProperties;

		protected virtual bool ZeroFillAllNumericFields
		{
			get { return false; }
		}

		protected virtual void AddFieldProperties() { }

		/// <summary>
		/// Populate the internal data array, based on the set properties.
		/// </summary>
		/// <remarks>Call after populating an empty data row for data exporting.</remarks>
		public void PopulateFields()
		{
			InternalPropertyFieldList.ForEach(f => f.Attribute.SetValueAsString((IZType)f.Property.GetValue(this, null), this));
		}

		/// <summary>
		/// Populate the properties based on the internal data array.
		/// </summary>
		void PopulateProperties()
		{
			InternalPropertyFieldList.ForEach(f =>
			{
				try
				{
					f.Property.SetValue(this, f.Attribute.GetValue(this), null);
				}
				catch (ArgumentException ex)
				{
					throw new ArgumentException(string.Format("No set method exists for property ({0}.{1}).", this.GetType(), f.Property.Name), ex);
				}
			});
		}

		/// <summary>
		/// Populate the properties based on the given fixed width line data.
		/// </summary>
		void PopulateProperties(ZString lineData)
		{
			if (IsFixedWidth)
			{
				int position = 0;
				InternalPropertyFieldList.ForEach(f =>
				{
					DataRow[f.Attribute.Position] = lineData.SubstringSafe(position, f.Attribute.Length).Trim();
					position += f.Attribute.Length;
					f.Property.SetValue(this, f.Attribute.GetValue(this), null);
				});
			}
		}

		public IList<PropertyInfoFieldAttributePair> PropertyFieldPairs
		{
			get { return InternalPropertyFieldList; }
		}

		protected List<PropertyInfoFieldAttributePair> InternalPropertyFieldList
		{
			get
			{
				if (propertyInfoFieldAttributes == null)
				{
					SetFieldAttributes();
				}
				return propertyInfoFieldAttributes;
			}
		}

		void SetFieldAttributes()
		{
			propertyInfoFieldAttributes = new List<PropertyInfoFieldAttributePair>();
			Type rowType = GetType();
			foreach (PropertyInfo property in rowType.GetProperties())
			{
				if (property.GetCustomAttributes(typeof(FieldAttribute), true).Length > 1)
				{
					throw new InvalidOperationException(rowType.FullName + "." + property.Name + " has too many 'Attributes' - it should only have one.");
				}

				if (Attribute.IsDefined(property, typeof(FieldAttribute)))
				{
					FieldAttribute attrib = (FieldAttribute)property.GetCustomAttributes(typeof(FieldAttribute), false)[0];
					if (property.PropertyType == typeof(ZDecimal) && attrib.GetType() != typeof(DecimalFieldAttribute) && attrib.GetType() != typeof(DecimalAsIntFieldAttribute) && attrib.GetType() != typeof(DecimalAsStringFieldAttribute))
					{
						throw new InvalidCastException(String.Format(messageFormat, rowType.FullName, property.Name, property.PropertyType, attrib.GetType(), "ZDecimal, DecimalAsInt or DecimalAsString"));
					}
					else if (property.PropertyType == typeof(ZString) && attrib.GetType() != typeof(FieldAttribute))
					{
						throw new InvalidCastException(String.Format(messageFormat, rowType.FullName, property.Name, property.PropertyType, attrib.GetType(), "ZString"));
					}
					else if (property.PropertyType == typeof(ZInt) && attrib.GetType() != typeof(IntFieldAttribute))
					{
						throw new InvalidCastException(String.Format(messageFormat, rowType.FullName, property.Name, property.PropertyType, attrib.GetType(), "ZInt"));
					}
					else if (property.PropertyType == typeof(ZDateTime) && attrib.GetType() != typeof(DateTimeFieldAttribute))
					{
						throw new InvalidCastException(String.Format(messageFormat, rowType.FullName, property.Name, property.PropertyType, attrib.GetType(), "ZDateTime"));
					}
					else if (property.PropertyType == typeof(ZBool) && attrib.GetType() != typeof(BoolFieldAttribute))
					{
						throw new InvalidCastException(String.Format(messageFormat, rowType.FullName, property.Name, property.PropertyType, attrib.GetType(), "ZBool"));
					}

					propertyInfoFieldAttributes.Add(new PropertyInfoFieldAttributePair(property, attrib));
					maxPosition = Math.Max(maxPosition, attrib.Position);
				}
			}
			propertyInfoFieldAttributes.Sort(new PropertyInfoFieldAttributeComparer());
		}
		List<PropertyInfoFieldAttributePair> propertyInfoFieldAttributes;
		const String messageFormat = "Property field attribute type mismatch exception.\r\n\r\nProperty {0}.{1} type {2} and attribute type {3} are mismatched.\r\nExpecting field attribute type {4}.\r\n";

		public override int FieldCount
		{
			get
			{
				if (InternalPropertyFieldList.Count > 0)
				{
					return maxPosition + 1;
				}
				else
				{
					return DataRow.Length;
				}
			}
		}
		int maxPosition;

		public virtual bool IsValid()
		{
			return true;
		}

		public virtual bool IsFixedWidth
		{
			get { return false; }
		}

		public class PropertyInfoFieldAttributePair
		{
			public PropertyInfoFieldAttributePair(PropertyInfo property, FieldAttribute attribute)
			{
				Property = property;
				Attribute = attribute;
			}

			public readonly FieldAttribute Attribute;
			public readonly PropertyInfo Property;
		}

		class PropertyInfoFieldAttributeComparer : IComparer<PropertyInfoFieldAttributePair>
		{
			int IComparer<PropertyInfoFieldAttributePair>.Compare(PropertyInfoFieldAttributePair x, PropertyInfoFieldAttributePair y)
			{
				return x.Attribute.Position.CompareTo(y.Attribute.Position);
			}
		}

		const char zero = '0';

#if DEBUG
		public String this[string fieldName]
		{
			get { return GetField(GetPosition(fieldName)); }
			set { SetField(GetPosition(fieldName), value); }
		}

		int GetPosition(string fieldName)
		{
			int result = 0;
			try
			{
				result = InternalPropertyFieldList.Find(f => f.Property.Name == fieldName).Attribute.Position;
			}
			catch (NullReferenceException ex)
			{
				throw new NullReferenceException("Unable to find field name (" + fieldName + ") for this data row (" + GetType() + ")", ex);
			}
			return result;
		}
#endif
	}
}
