using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using CargoWise.Common;
using Microsoft.SqlServer.Types;

namespace CargoWise.Types
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public struct ZDataType
	{
		public override bool Equals(object obj)
		{
			if (obj is ZDataType other)
			{
				return other.IsInteger == IsInteger && other.IsNumeric == IsNumeric;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return IsNumeric.GetHashCode() ^ IsInteger.GetHashCode();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator !=(ZDataType lhs, ZDataType rhs)
		{
			return !(lhs == rhs);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator ==(ZDataType lhs, ZDataType rhs)
		{
			return lhs.IsInteger == rhs.IsInteger && lhs.IsNumeric == rhs.IsNumeric;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "NonNumeric")]
		public static ZDataType NonNumeric
		{
			get { return nonNumeric; }
		}

		public static ZDataType Numeric
		{
			get { return numeric; }
		}

		public static ZDataType Integer
		{
			get { return integer; }
		}

		public bool IsNumeric
		{
			get { return isNumeric; }
		}

		public bool IsInteger
		{
			get { return isInteger; }
		}

		public static IZType ObjectToZType(object value)
		{
			Argument.NotNull(value, nameof(value));

			return ObjectToZType(value, value.GetType());
		}

		public static IZType ObjectToZType(Type targetZType, object value)
		{
			Argument.NotNull(targetZType, nameof(targetZType));

			if (typeof(IZType).IsAssignableFrom(targetZType))
			{
				return (IZType)TypeDescriptor.GetConverter(targetZType).ConvertFrom(value);
			}
			else
			{
				throw new ArgumentException("targetZType was not a ZType: " + targetZType.FullName, nameof(targetZType));
			}
		}

		public static IZType InvariantStringToZType(Type targetZType, string value)
		{
			Argument.NotNull(targetZType, nameof(targetZType));
			if (typeof(IZType).IsAssignableFrom(targetZType))
			{
				return (IZType)TypeDescriptor.GetConverter(targetZType).ConvertFromInvariantString(value);
			}
			else
			{
				throw new ArgumentException("targetZType was not a ZType: " + targetZType.FullName, nameof(targetZType));
			}
		}

		public static IZType ZTypeToEmptyValue(Type targetZType)
		{
			Argument.NotNull(targetZType, nameof(targetZType));
			return ObjectToZType(targetZType, DBNull.Value);
		}

		public static IZType ObjectToZType(System.Data.DataColumn sourceColumn, object value)
		{
			Argument.NotNull(sourceColumn, nameof(sourceColumn));
			Argument.NotNull(value, nameof(value));

			return ObjectToZType(value, sourceColumn.DataType);
		}

		static IZType ObjectToZType(object source, Type sourceType)
		{
			Argument.NotNull(source, nameof(source));
			Argument.NotNull(sourceType, nameof(sourceType));

			if (source is IZType zSource)
			{
				return zSource;
			}

			if (zTypeConversionMap.TryGetValue(sourceType, out var func))
			{
				var returnVal = func(source);
				return returnVal;
			}

			throw new ArgumentException("Unknown data type: " + source.GetType().FullName, nameof(source));
		}

		public static bool IsConvertibleToZType(object value)
		{
			if (value == null)
			{
				return false;
			}

			if (value is IZType)
			{
				return true;
			}

			var valueType = value.GetType();

			return zTypeConversionMap.ContainsKey(valueType);
		}

		static readonly ImmutableDictionary<Type, Func<object, IZType>> zTypeConversionMap = new Dictionary<Type, Func<object, IZType>>
		{
			{ typeof(Guid), val => new ZGuid(val) },
			{ typeof(DateTime), val => new ZDateTime(val) },
			{ typeof(DateTimeOffset), val => new ZDateTimeOffset(val) },
			{ typeof(TimeSpan), val => new ZTime(val) },
			{ typeof(SqlGeography), val => new ZGeography(val) },
			{ typeof(decimal), val => new ZDecimal(val) },
			{ typeof(long), val => new ZLong(val) },
			{ typeof(int), val => new ZInt(val) },
			{ typeof(short), val => new ZShort(val) },
			{ typeof(byte), val => new ZByte(val) },
			{ typeof(byte[]), val => new ZBlob(val) },
			{ typeof(string), val => new ZString(val) },
			{ typeof(bool), val => new ZBool(val) },
		}.ToImmutableDictionary();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static ImmutableDictionary<Type, Type> baseTypes = ImmutableDictionary<Type, Type>.Empty;

		public static Type ZTypeToBaseType(Type zType)
		{
			Argument.NotNull(zType, nameof(zType));

			var lookup = baseTypes;
			if (!lookup.TryGetValue(zType, out var resultType) && typeof(IZType).IsAssignableFrom(zType))
			{
				resultType = ((IZType)Activator.CreateInstance(zType)).BaseDataType;
				Interlocked.CompareExchange(ref baseTypes, lookup.Add(zType, resultType), lookup); // if the swap fails, don't worry it will get cached next time
			}

			return resultType;
		}

		#region Implementation

		static readonly ZDataType nonNumeric;
		static readonly ZDataType numeric = new ZDataType(true, false);
		static readonly ZDataType integer = new ZDataType(true, true);

		readonly bool isNumeric;
		readonly bool isInteger;

		ZDataType(bool isNumeric, bool isInteger)
		{
			this.isNumeric = isNumeric;
			this.isInteger = isInteger;
		}

		#endregion
	}
}
