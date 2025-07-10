using System;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public static class RegistryDataTypes
	{
		public static IRegistryDataType GuidType
		{
			get { return guidType ?? (guidType = new GuidRegistryDataType()); }
		}
		[ThreadStatic]
		static IRegistryDataType guidType;

		public static IRegistryDataType GuidArrayType
		{
			get { return guidArrayType ?? (guidArrayType = new GuidArrayRegistryDataType()); }
		}
		[ThreadStatic]
		static IRegistryDataType guidArrayType;

		public static IRegistryDataType DelimitedStringArrayType => delimitedStringArrayType ?? (delimitedStringArrayType = new DelimitedStringArrayRegistryDataType());
		[ThreadStatic]
		static IRegistryDataType delimitedStringArrayType;

		public static IRegistryDataType JsonStringArrayType => jsonStringArrayType ?? (jsonStringArrayType = new JsonStringArrayRegistryDataType());
		[ThreadStatic]
		static IRegistryDataType jsonStringArrayType;

		public static IRegistryDataType DateTimeType
		{
			get { return dateTimeType ?? (dateTimeType = new DateTimeRegistryDataType()); }
		}
		[ThreadStatic]
		static IRegistryDataType dateTimeType;

		public static IRegistryDataType StringType
		{
			get { return stringType ?? (stringType = new StringRegistryDataType()); }
		}
		[ThreadStatic]
		static IRegistryDataType stringType;

		public static IRegistryDataType IntType
		{
			get { return intType ?? (intType = new IntRegistryDataType()); }
		}
		[ThreadStatic]
		static IRegistryDataType intType;

		public static IRegistryDataType BinType_Deprecated
		{
			get { return binType_Deprecated ?? (binType_Deprecated = new BinaryRegistryDataType()); }
		}
		[ThreadStatic]
		static IRegistryDataType binType_Deprecated;

		public static IRegistryDataType DecimalType
		{
			get { return decimalType ?? (decimalType = new DecimalRegistryDataType()); }
		}
		[ThreadStatic]
		static IRegistryDataType decimalType;

		public static IRegistryDataType BoolType
		{
			get { return boolType ?? (boolType = new BooleanRegistryDataType()); }
		}
		[ThreadStatic]
		static IRegistryDataType boolType;

		public static class Codes
		{
			public const string String = "STR";
			public const string DelimitedStringArray = "SAR";
			public const string Binary = "BIN";
			public const string JsonStringArray = "JSN";
			public const string Guid = "GID";
			public const string GuidArray = "GAR";
			public const string Decimal = "DEC";
			public const string DecimalArray = "DAR";
			public const string Int = "INT";
			public const string Bool = "BOL";
			public const string DateTime = "DT";
		}
	}
}
