using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using CargoWise.Common;

namespace CargoWise.Types
{
	[TypeConverter(typeof(ZGuidTypeConverter))]
	[DebuggerDisplay("{fValue}")]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public struct ZGuid : IZType, IZTypeInternals
	{
		[DebuggerStepThrough]
		public ZGuid(object value)
		{
			if (value == null)
			{
				this = Empty;
			}
			else if (value is Guid guid)
			{
				this = new ZGuid(guid);
			}
			else
			{
				TypeConverter converter = ZGuidTypeConverter.Instance;
				if (converter.CanConvertFrom(value.GetType()))
				{
					var converted = converter.ConvertFrom(value);
					this = (ZGuid)converted;
				}
				else
				{
					throw new ZTypeValueException(typeof(ZGuid), value);
				}
			}
		}

		[DebuggerStepThrough]
		public ZGuid(Guid value)
		{
			if (value == Guid.Empty)
			{
				this = Empty;
			}
			else if (value == ZTypeConstants.InvalidGuid)
			{
				this = Invalid;
			}
			else if (value == ZTypeConstants.MissingGuid)
			{
				this = Missing;
			}
			else
			{
				this = new ZGuid(value, true, true, false);
			}
		}

		ZGuid(Guid value, bool isNotEmpty, bool isValid, bool isMissing)
		{
			fIsNotEmpty = isNotEmpty;
			fIsValid = isValid;
			fIsMissing = isMissing;
			fValue = value;
		}

		#region Static

		/// <summary>
		/// The empty ZGuid.
		/// </summary>
		public static readonly ZGuid Empty;

		/// <summary>
		/// The invalid (but not empty) ZGuid.
		/// </summary>
		public static readonly ZGuid Invalid = new ZGuid(ZTypeConstants.InvalidGuid, true, false, false);

		/// <summary>
		/// The missing ZGuid.
		/// </summary>
		public static readonly ZGuid Missing = new ZGuid(ZTypeConstants.MissingGuid, true, false, true);

		public static ZGuid NewZGuid()
		{
			return new ZGuid(Guid.NewGuid());
		}

		public static bool TryParse(object value, out ZGuid guid)
		{
			guid = Empty;
			bool success = true;
			if (value != null)
			{
				if (success = Guid.TryParse(value.ToString(), out var theGuid))
				{
					guid = new ZGuid(theGuid);
				}
			}
			return success;
		}

		public static ZGuid ParseSafe(string value)
		{
			ZGuid guid;
			if (string.IsNullOrEmpty(value))
			{
				guid = Empty;
			}
			else if (!TryParse(value, out guid))
			{
				guid = Invalid;
			}
			return guid;
		}

		public static bool IsGuid(ZString value)
		{
			if (value.Length != 36)
			{
				return false;
			}

			if (value.Replace("-", ZString.Empty).Length != 32)
			{
				return false;
			}

			return TryParse(value, out var g);
		}

		#endregion

		#region Hashing / Key algorithms

		public string ToStringKey()
		{
			char[] r = new char[8];
			byte[] m = fValue.ToByteArray();

			r[0] = (char)((m[1] << 8) + m[0]);
			r[1] = (char)((m[3] << 8) + m[2]);
			r[2] = (char)((m[5] << 8) + m[4]);
			r[3] = (char)((m[7] << 8) + m[6]);
			r[4] = (char)((m[9] << 8) + m[8]);
			r[5] = (char)((m[11] << 8) + m[10]);
			r[6] = (char)((m[13] << 8) + m[12]);
			r[7] = (char)((m[15] << 8) + m[14]);

			return new string(r);
		}

		#endregion

		#region IsMissing

		[XmlIgnore]
		public bool IsMissing
		{
			get { return fIsMissing; }
		}

		#endregion

		#region Object Overrides

		/// <summary>
		/// Is this instance equal to the specified object?
		/// </summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		[DebuggerStepThrough]
		public override bool Equals(object obj)
		{
			return
				(obj is ZGuid guid && guid == this) ||
				(obj is Guid guid1 && guid1 == fValue);
		}

		/// <summary>
		/// The hash code for this instance.
		/// </summary>
		public override int GetHashCode()
		{
			return fValue.GetHashCode();
		}

		/// <summary>
		/// The default string representation of this instance.
		/// </summary>
		public override string ToString()
		{
			return fValue.ToString();
		}

		#endregion

		#region Operator Overloads

		#region ZGuid + ZGuid

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator ==(ZGuid lhs, ZGuid rhs)
		{
			return lhs.fValue == rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator !=(ZGuid lhs, ZGuid rhs)
		{
			return !(lhs == rhs);
		}

		#endregion

		#region ZGuid + Guid

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator ==(ZGuid lhs, Guid rhs)
		{
			return lhs == new ZGuid(rhs);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator ==(Guid lhs, ZGuid rhs)
		{
			return new ZGuid(lhs) == rhs;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator !=(ZGuid lhs, Guid rhs)
		{
			return !(lhs == new ZGuid(rhs));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator !=(Guid lhs, ZGuid rhs)
		{
			return !(new ZGuid(lhs) == rhs);
		}

		#endregion

		#endregion

		#region Casting

		/// <summary>
		/// Converts the ZGuid to a Guid. WARNING - an exception will be thrown if ToGuid() is invoked on an empty or invalid ZGuid!".
		/// </summary>
		public Guid ToGuid()
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot convert an Empty or Invalid ZGuid to a Guid!");
			}

			return fValue;
		}

		internal Guid ToGuidUnsafe()
		{
			return fValue;
		}

		[DebuggerStepThrough]
		public static implicit operator ZGuid(Guid value)
		{
			return new ZGuid(value);
		}

		#endregion

		#region XmlSerializedValue

		[XmlText]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Guid XmlSerializedValue
		{
			get { return ToGuidUnsafe(); }
			set { this = value; }
		}

		#endregion

		#region IZType Members

		[XmlIgnore]
		public ZDataType DataType
		{
			[DebuggerStepThrough]
			get { return ZDataType.NonNumeric; }
		}

		[XmlIgnore]
		public Type BaseDataType
		{
			get { return typeof(Guid); }
		}

		[XmlIgnore]
		public bool IsEmpty
		{
			[DebuggerStepThrough]
			get { return !fIsNotEmpty; }
		}

		[XmlIgnore]
		public bool IsValid
		{
			[DebuggerStepThrough]
			get { return fIsValid; }
		}

		[XmlIgnore]
		public bool IsDefault
		{
			get { return this == (ZGuid)Default; }
		}

		[XmlIgnore]
		public IZType Default
		{
			get { return Empty; }
		}

		#endregion

		#region IComparable Members

		public int CompareTo(object obj)
		{
			if (obj is IZTypeInternals zTypeInternals)
			{
				obj = zTypeInternals.GetValueForLogicalDataLayer(false);
			}

			if (!(obj is ZGuid || obj is Guid || obj is DBNull))
			{
				if (obj == null)
				{
					return 1;
				}
				else if (!IsValid)
				{
					return -1;
				}
				else
				{
					return 0;
				}
			}
			else
			{
				return obj is DBNull && IsEmpty ? 0 : fValue.CompareTo(obj);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs")]
		public static bool operator <(ZGuid lhs, ZGuid rhs)
		{
			return lhs.CompareTo(rhs) == -1;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs")]
		public static bool operator >(ZGuid lhs, ZGuid rhs)
		{
			return lhs.CompareTo(rhs) == 1;
		}

		#endregion

		#region IZTypeInternals Members

		object IZTypeInternals.GetValueForLogicalDataLayer(bool isNullable)
		{
			return IsEmpty && isNullable ? DBNull.Value : fValue;
		}

		#endregion

		#region Brett's Guid

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Bretts")]
		public static ZGuid BrettsGuid
		{
			get { return new ZGuid("20DD961B-3E62-40E5-B60A-B1312B70F5EE"); }
		}

		#endregion

		#region Implementation

		readonly Guid fValue;
		readonly bool fIsNotEmpty;
		readonly bool fIsValid;
		readonly bool fIsMissing;

		#endregion
	}
}
