using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;
using CargoWise.Common;

namespace CargoWise.Types
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes"), TypeConverter(typeof(ZBlobTypeConverter))]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public struct ZBlob : IZType, IZTypeInternals
	{
		public ZBlob(object value)
		{
			if (value == null)
			{
				this = Empty;
			}
			else
			{
				ZBlobTypeConverter converter = ZBlobTypeConverter.Instance;
				if (converter.CanConvertFrom(value.GetType()))
				{
					object converted = converter.ConvertFrom(null, CultureInfo.CurrentCulture, value);
					this = (ZBlob)converted;
				}
				else
				{
					throw new ZTypeValueException(typeof(ZBlob), value);
				}
			}
		}

		public ZBlob(byte[] value)
		{
			if (value != null)
			{
				fValue_DoNotAccessMeDirectly = value;
				fIsNotEmpty = (fValue_DoNotAccessMeDirectly.Length > 0);
			}
			else
			{
				this = Empty;
			}
		}

		#region Static

		/// <summary>
		/// The empty ZBlob.
		/// </summary>
		public static readonly ZBlob Empty = new ZBlob(Array.Empty<byte>());

		/// <summary>
		/// A new ZBlob from a UTF8-encoded ZString.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "UTF")]
		public static ZBlob FromUTF8(ZString value)
		{
			return new ZBlob(Encoding.UTF8.GetBytes(value));
		}

		/// <summary>
		/// A new ZBlob from an ASCII-encoded ZString.
		/// </summary>
		public static ZBlob FromAscii(ZString value)
		{
			return new ZBlob(Encoding.ASCII.GetBytes(value));
		}

		#endregion

		#region Object Overrides

		public override bool Equals(object obj)
		{
			return (obj is ZBlob blob && blob == this) ||
				(obj is byte[] && new ZBlob(obj) == this);
		}

		public override int GetHashCode()
		{
			return IsEmpty || Length == 0 ? 0 : (Length.GetHashCode() ^ this[0].GetHashCode());
		}

		public override string ToString()
		{
			return fValue.ToString();
		}

		#endregion

		#region Operator Overloads

		/// <summary>
		/// Returns true if two ZBlobs are equal using a *DEEP COMPARE*.
		/// </summary>
		/// <param name="lhs">The left-hand side ZBlob.</param>
		/// <param name="rhs">The right-hand side ZBlob.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs")]
		public static bool operator ==(ZBlob lhs, ZBlob rhs)
		{
			bool result = true;

			if (lhs.fValue.Length != rhs.fValue.Length)
			{
				result = false;
			}
			else
			{
				for (int i = 0; i < lhs.fValue.Length; i++)
				{
					if (lhs.fValue[i] != rhs.fValue[i])
					{
						result = false;
						break;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Returns true if two ZBlobs are not equal using a *DEEP COMPARE*.
		/// </summary>
		/// <param name="lhs">The left-hand side ZBlob.</param>
		/// <param name="rhs">The right-hand side ZBlob.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs")]
		public static bool operator !=(ZBlob lhs, ZBlob rhs)
		{
			return !(lhs == rhs);
		}

		#endregion

		#region Casting

		public static implicit operator ZBlob(byte[] value)
		{
			return new ZBlob(value);
		}

		public static implicit operator byte[](ZBlob value)
		{
			return value.fValue;
		}

		#endregion

		#region ToUTF8/Ascii

		/// <summary>
		/// The ZBlob converted to a UTF8-encoded ZString.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "UTF")]
		public ZString ToUTF8()
		{
			return Encoding.UTF8.GetString(fValue);
		}

		/// <summary>
		/// The ZBlob converted to an ASCII-encoded ZString.
		/// </summary>
		public ZString ToAscii()
		{
			return Encoding.ASCII.GetString(fValue);
		}

		#endregion

		#region IndexOf

		public int IndexOf(byte value)
		{
			return IndexOf(new byte[] { value });
		}

		public int IndexOf(byte[] value)
		{
			Argument.NotNull(value, nameof(value));

			int result = -1;
			int lastPlace = Length - value.Length + 1;
			for (int i = 0; i < lastPlace; i++)
			{
				var sequenceMatched = true;
				for (int j = 0; j < value.Length; j++)
				{
					if (fValue[i + j] != value[j])
					{
						sequenceMatched = false;
						break;
					}
				}
				if (sequenceMatched)
				{
					result = i;
					break;
				}
			}
			return result;
		}

		#endregion

		#region SubStrB
		public ZBlob SubBlobSafe(int startPosition)
		{
			// The next "if" block is to avoid Code Contracts warning only. Whthout the block
			// The warning message is caused by the following code line below:
			// byte[] result = new byte[fValue.Length - startPosition];
			//
			if (startPosition == fValue.Length)
			{
				return new ZBlob(Array.Empty<byte>());
			}
			if (startPosition < 0 || startPosition > fValue.Length)
			{
				startPosition = 0;
			}

			byte[] result = new byte[fValue.Length - startPosition];
			int resultPosition = 0;

			for (int i = startPosition; i < fValue.Length; i++)
			{
				result[resultPosition] = fValue[i];
				++resultPosition;
			}

			return new ZBlob(result);
		}
		#endregion

		#region Wrapping byte[] Stuff

		/// <summary>
		/// The length of the internal byte[].
		/// </summary>
		[XmlIgnore]
		public int Length
		{
			get { return fValue.Length; }
		}

		/// <summary>
		/// Gets the element at the specified index of the internal byte[].
		/// </summary>
		/// <param name="index"></param>
		/// <returns>The element at the specified index of the internal byte[].</returns>
		public byte this[int index]
		{
			get
			{
				if (!(index >= 0 && index < Length))
				{
					throw new ArgumentOutOfRangeException(nameof(index), "Blob index is out of bounds in indexer.");
				}

				return fValue[index];
			}
		}

		#endregion

		#region XmlSerializedValue

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays"), XmlText]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public byte[] XmlSerializedValue
		{
			get { return this; }
			set { this = value; }
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
			get { return typeof(byte[]); }
		}

		[XmlIgnore]
		public bool IsEmpty
		{
			get { return !fIsNotEmpty; }
		}

		[XmlIgnore]
		public bool IsValid
		{
			get { return true; }
		}

		[XmlIgnore]
		public bool IsDefault
		{
			get { return this == (ZBlob)Default; }
		}

		[XmlIgnore]
		public IZType Default
		{
			get { return new ZBlob(); }
		}

		#endregion

		#region IComparable Members

		public int CompareTo(object obj)
		{
			throw new NotSupportedException("IComparable.CompareTo() cannot be used on ZBlobs!");
		}

		#endregion

		#region IZTypeInternals Members

		public object GetValueForLogicalDataLayer(bool isNullable)
		{
			return IsEmpty && isNullable ? DBNull.Value : fValue;
		}

		#endregion

		#region Implementation

		// See ms-help://MS.VSCC.2003/MS.MSDNQTR.2003APR.1033/csref/html/vcrefStructTypes.htm
		// "It is an error to declare a default (parameterless) constructor for a struct. A default
		// constructor is always provided to initialize the struct members to their default values."

		// As a result, .NET will not use our constructors when initialising the struct
		// with no parameter (ie. new ZBlob()), thus we need to set fValue when accessed:

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Avoid extra allocations for performance reasons")]
		readonly byte[] fValue_DoNotAccessMeDirectly;
		readonly bool fIsNotEmpty;

		byte[] fValue
		{
			get
			{
				return fValue_DoNotAccessMeDirectly ?? Empty.fValue;
			}
		}

		#endregion
	}
}
