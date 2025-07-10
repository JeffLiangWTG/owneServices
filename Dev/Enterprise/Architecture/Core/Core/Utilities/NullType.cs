using System;

namespace Enterprise.ZArchitecture.Environment
{
	public static class NullType
	{
		public static float Single { get { return 0F; } }
		public static Int16 Int16 { get { return 0; } }
		public static int Int32 { get { return 0; } }
		public static DateTime DateTime { get { return new DateTime(1753, 1, 1); } }
		public static Guid Guid { get { return Guid.Empty; } }
		public static string String { get { return ""; } }
		public static double Double { get { return 0D; } }
		public static float Float { get { return 0F; } }
		public static decimal Decimal { get { return 0M; } }
		public static int Int { get { return 0; } }
		public static long Long { get { return 0L; } }
		public static object Object { get { return System.DBNull.Value; } }
		public static bool Boolean { get { return false; } }
		public static Int64 Int64 { get { return 0L; } }
		public static Byte Byte { get { return System.Byte.MinValue; } }

		public static object GetNullObject(Type expectedType)
		{
			if (expectedType.FullName == typeof(DateTime).ToString())
			{
				return NullType.DateTime;
			}
			else if (expectedType.FullName == typeof(decimal).ToString())
			{
				return NullType.Decimal;
			}
			else if (expectedType.FullName == typeof(double).ToString())
			{
				return NullType.Double;
			}
			else if (expectedType.FullName == typeof(float).ToString())
			{
				return NullType.Float;
			}
			else if (expectedType.FullName == typeof(Guid).ToString())
			{
				return NullType.Guid;
			}
			else if (expectedType.FullName == typeof(int).ToString())
			{
				return NullType.Int;
			}
			else if (expectedType.FullName == typeof(long).ToString())
			{
				return NullType.Long;
			}
			else if (expectedType.FullName == typeof(object).ToString())
			{
				return NullType.Object;
			}
			else if (expectedType.FullName == typeof(string).ToString())
			{
				return NullType.String;
			}
			else if (expectedType.FullName == typeof(byte).ToString())
			{
				return NullType.Byte;
			}
			else if (expectedType.FullName == typeof(short).ToString())
			{
				return NullType.Int16;
			}
			else
			{
				throw new NullTypeException(expectedType);
			}
		}
	}

	[Serializable]
	public class NullTypeException : OdysseyException
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public NullTypeException(object errObject)
			: base(string.Format("{0} is not member of NullType", errObject.ToString()))
		{
		}

#if NETFRAMEWORK
		protected NullTypeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
