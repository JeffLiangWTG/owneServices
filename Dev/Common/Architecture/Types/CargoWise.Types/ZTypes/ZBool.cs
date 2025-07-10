using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using CargoWise.Common;

namespace CargoWise.Types
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes"), TypeConverter(typeof(ZBoolTypeConverter))]
	[DebuggerDisplay("{fValue}")]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public struct ZBool : IZType, IZTypeInternals
	{
		[DebuggerStepThrough]
		public ZBool(object value)
		{
			if (value == null)
			{
				this = False;
			}
			else if (value is bool boolean)
			{
				this = new ZBool(boolean);
			}
			else
			{
				TypeConverter converter = ZBoolTypeConverter.Instance;
				if (converter.CanConvertFrom(value.GetType()))
				{
					var converted = converter.ConvertFrom(value);
					this = (ZBool)converted;
				}
				else
				{
					throw new ZTypeValueException(typeof(ZBool), value);
				}
			}
		}

		[DebuggerStepThrough]
		public ZBool(bool value)
		{
			fValue = value;
		}

		[DebuggerStepThrough]
		public ZBool(char value)
		{
			if (value == 'Y' || value == 'y' || value == '1' || value == 'T' || value == 't')
			{
				fValue = true;
			}
			else if (value == 'N' || value == 'n' || value == ' ' || value == '0' || value == 'F' || value == 'f')
			{
				fValue = false;
			}
			else
			{
				throw new ZTypeValueException(typeof(ZBool), value);
			}
		}

		[DebuggerStepThrough]
		public ZBool(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				this = False;
			}
			else if (value.Length != 1)
			{
				if (value.Equals("YES", StringComparison.OrdinalIgnoreCase) || value.Equals("TRUE", StringComparison.OrdinalIgnoreCase))
				{
					fValue = true;
				}
				else if (value.Equals("NO", StringComparison.OrdinalIgnoreCase) || value.Equals("FALSE", StringComparison.OrdinalIgnoreCase))
				{
					fValue = false;
				}
				else
				{
					throw new ZTypeValueException(typeof(ZBool), value);
				}
			}
			else
			{
				this = new ZBool(value[0]);
			}
		}

		#region True/False Constants

		public static readonly ZBool True = new ZBool(true);
		public static readonly ZBool False = new ZBool(false);

		#endregion

		#region Object Overrides

		public override bool Equals(object obj)
		{
			return (obj is ZBool @bool && @bool == this) ||
					(obj is bool boolean && boolean == fValue);
		}

		public override int GetHashCode()
		{
			return fValue.GetHashCode();
		}

		public override string ToString()
		{
			return fValue ? "Y" : "N";
		}

		#endregion

		#region Casting

		[DebuggerStepThrough]
		public static implicit operator ZBool(bool value)
		{
			return value ? True : False;
		}

		[DebuggerStepThrough]
		public static implicit operator bool(ZBool value)
		{
			return value.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator ==(ZBool lhs, ZBool rhs)
		{
			return lhs.fValue == rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator !=(ZBool lhs, ZBool rhs)
		{
			return lhs.fValue != rhs.fValue;
		}

		#endregion

		#region XmlSerializedValue

		[XmlText]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool XmlSerializedValue
		{
			get { return this; }
			set { this = value; }
		}

		#endregion

		#region IComparable Members

		public int CompareTo(object obj)
		{
			if (obj != null && !(obj is bool))
			{
				obj = new ZBool(obj).fValue;
			}

			return fValue.CompareTo(obj);
		}

		#endregion

		#region IZTypeInternals Members

		public object GetValueForLogicalDataLayer(bool isNullable)
		{
			return (isNullable && !fValue) ? DBNull.Value : fValue;
		}

		#endregion

		#region IZType Members

		[XmlIgnore]
		public ZDataType DataType
		{
			get { return ZDataType.NonNumeric; }
		}

		[XmlIgnore]
		public Type BaseDataType
		{
			get { return typeof(bool); }
		}

		[XmlIgnore]
		public bool IsEmpty
		{
			get { return false; }
		}

		[XmlIgnore]
		public bool IsValid
		{
			get { return true; }
		}

		[XmlIgnore]
		public bool IsDefault
		{
			get { return this == (ZBool)Default; }
		}

		[XmlIgnore]
		public IZType Default
		{
			get { return new ZBool(); }
		}

		#endregion

		#region TryParse / ParseSafe

		public static bool TryParse(string sourceValue, out ZBool typedResult)
		{
			Argument.NotNull(sourceValue, nameof(sourceValue));

			var sourceValueUpper = sourceValue.ToUpperInvariant();

			if (sourceValueUpper == "TRUE" ||
				sourceValueUpper == "YES" ||
				sourceValueUpper == "1" ||
				sourceValueUpper == "T" ||
				sourceValueUpper == "Y")
			{
				typedResult = True;
				return true;
			}
			else if (sourceValueUpper == "FALSE" ||
				sourceValueUpper == "NO" ||
				string.IsNullOrWhiteSpace(sourceValueUpper) ||
				sourceValueUpper == "0" ||
				sourceValueUpper == "F" ||
				sourceValueUpper == "N")
			{
				typedResult = False;
				return true;
			}
			else
			{
				typedResult = False;
				return false;
			}
		}

		public static ZBool ParseSafe(ZString value, ZBool defaultValue)
		{
			Argument.NotNullOrEmpty(value, nameof(value));
			return TryParse(value, out var parsed) ? parsed : defaultValue;
		}
		#endregion

		#region Implementation

		readonly bool fValue;

		#endregion
	}
}
