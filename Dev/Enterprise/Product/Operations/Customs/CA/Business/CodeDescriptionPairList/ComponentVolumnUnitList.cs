using System;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public partial class ComponentVolumnUnitList : CodeDescriptionPairList
	{
		public static class Codes
		{
			[BaseUnit(1)]
			public const string Becquerel = "BQL";

			[BaseUnit(1000)]
			public const string Kilobecquerel = "2Q";

			[BaseUnit(1000000)]
			public const string Megabecquerel = "4N";

			[BaseUnit(1000000000)]
			public const string Gigabecquerel = "GBQ";

			[BaseUnit(1000000000000)]
			public const string Terabecquerel = "TBQ";

			[BaseUnit(1000000000000000)]
			public const string Petabecquerel = "PBQ";

			[BaseUnit(36982817978.02659)]
			public const string Curie = "MCU";

			[BaseUnit(0.000001)]
			public const string Microgram = "MC";

			[BaseUnit(0.001)]
			public const string Milligram = "MGM";

			[BaseUnit(1)]
			public const string Gram = "GRM";

			[BaseUnit(1000)]
			public const string Kilogram = "KGM";

			[BaseUnit(1000000)]
			public const string Megagram = "2U";

			[BaseUnit(452.8975799036209)]
			public const string Pounds = "LBR";

			[BaseUnit(1000000)]
			public const string MetricTon = "TNE";

			[BaseUnit(1)]
			public const string Littre = "LTR";

			[BaseUnit(1000000)]
			public const string Metre = "MTR";

			[BaseUnit(1000)]
			public const string Kilovolt = "KVT";

			[BaseUnit(1000000)]
			public const string Megavolt = "B78";

			[BaseUnit(1)]
			public const string Electronvolt = "A53";

			[BaseUnit(1000)]
			public const string Kiloelectronvolt = "B29";

			[BaseUnit(100000)]
			public const string Megaelectronvolt = "B71";

			[BaseUnit(1000000000)]
			public const string Gigaelectronvolt = "A85";

			[BaseUnit(0.000001)]
			public const string Microamperes = "B84";

			[BaseUnit(1)]
			public const string Moles = "C34";

			[BaseUnit(1000)]
			public const string BecquerelGram = "B25";
		}

		public static class Descriptions
		{
			public static MultilingualString Electronvolt { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|A53", "Electronvolt"); } }
			public static MultilingualString Gigaelectronvolt { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|A85", "Gigaelectronvolt"); } }
			public static MultilingualString BecquerelGram { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|B25", "Becquerel/gram"); } }
			public static MultilingualString Kiloelectronvolt { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|B29", "Kiloelectronvolt"); } }
			public static MultilingualString Megaelectronvolt { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|B71", "Megaelectronvolt"); } }
			public static MultilingualString Megavolt { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|B78", "Megavolt"); } }
			public static MultilingualString Microamperes { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|B84", "Microamperes"); } }
			public static MultilingualString Becquerel { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|BQL", "Becquerel"); } }
			public static MultilingualString Moles { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|C34", "Moles"); } }
			public static MultilingualString Gigabecquerel { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|GBQ", "Gigabecquerel"); } }
			public static MultilingualString Gram { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|GRM", "Gram"); } }
			public static MultilingualString Kilogram { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|KGM", "Kilogram"); } }
			public static MultilingualString Kilovolt { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|KVT", "Kilovolt"); } }
			public static MultilingualString Pounds { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|LBR", "Pounds"); } }
			public static MultilingualString Littre { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|LTR", "Littre"); } }
			public static MultilingualString Microgram { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|MC", "Microgram"); } }
			public static MultilingualString Curie { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|MCU", "Curie"); } }
			public static MultilingualString Milligram { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|MGM", "Milligram"); } }
			public static MultilingualString Metre { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|MTR", "Metre"); } }
			public static MultilingualString Petabecquerel { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|PBQ", "Petabecquerel"); } }
			public static MultilingualString Terabecquerel { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|TBQ", "Terabecquerel"); } }
			public static MultilingualString MetricTon { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|TNE", "Metric ton"); } }
			public static MultilingualString Kilobecquerel { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|2Q", "Kilobecquerel"); } }
			public static MultilingualString Megagram { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|2U", "Megagram"); } }
			public static MultilingualString Megabecquerel { get { return ResString.GetMultilingualString("ComponentVolumnUnitList|4N", "Megabecquerel"); } }
		}

		public ComponentVolumnUnitList(string code)
		{
			switch (code)
			{
				case CNSCCategories.Codes.CNS:
					AddPair(Codes.Becquerel, Descriptions.Becquerel);
					AddPair(Codes.Kilobecquerel, Descriptions.Kilobecquerel);
					AddPair(Codes.Megabecquerel, Descriptions.Megabecquerel);
					AddPair(Codes.Gigabecquerel, Descriptions.Gigabecquerel);
					AddPair(Codes.Terabecquerel, Descriptions.Terabecquerel);
					AddPair(Codes.Petabecquerel, Descriptions.Petabecquerel);
					AddPair(Codes.Curie, Descriptions.Curie);
					AddPair(Codes.Microgram, Descriptions.Microgram);
					AddPair(Codes.Milligram, Descriptions.Milligram);
					AddPair(Codes.Gram, Descriptions.Gram);
					AddPair(Codes.Kilogram, Descriptions.Kilogram);
					AddPair(Codes.Megagram, Descriptions.Megagram);
					AddPair(Codes.Pounds, Descriptions.Pounds);
					AddPair(Codes.MetricTon, Descriptions.MetricTon);
					AddPair(Codes.Littre, Descriptions.Littre);
					AddPair(Codes.Metre, Descriptions.Metre);
					AddPair(Codes.BecquerelGram, Descriptions.BecquerelGram);
					break;
				case CNSCCategories.Codes.NS:
				case CNSCCategories.Codes.RD:
					AddPair(Codes.Becquerel, Descriptions.Becquerel);
					AddPair(Codes.Kilobecquerel, Descriptions.Kilobecquerel);
					AddPair(Codes.Megabecquerel, Descriptions.Megabecquerel);
					AddPair(Codes.Gigabecquerel, Descriptions.Gigabecquerel);
					AddPair(Codes.Terabecquerel, Descriptions.Terabecquerel);
					AddPair(Codes.Petabecquerel, Descriptions.Petabecquerel);
					AddPair(Codes.Milligram, Descriptions.Milligram);
					AddPair(Codes.Gram, Descriptions.Gram);
					AddPair(Codes.Kilogram, Descriptions.Kilogram);
					AddPair(Codes.Kilovolt, Descriptions.Kilovolt);
					AddPair(Codes.Megavolt, Descriptions.Megavolt);
					AddPair(Codes.Electronvolt, Descriptions.Electronvolt);
					AddPair(Codes.Kiloelectronvolt, Descriptions.Kiloelectronvolt);
					AddPair(Codes.Megaelectronvolt, Descriptions.Megaelectronvolt);
					AddPair(Codes.Gigaelectronvolt, Descriptions.Gigaelectronvolt);
					AddPair(Codes.Microamperes, Descriptions.Microamperes);
					AddPair(Codes.Moles, Descriptions.Moles);
					AddPair(Codes.BecquerelGram, Descriptions.BecquerelGram);
					break;
			}
		}

		[AttributeUsage(AttributeTargets.Field)]
		internal sealed class BaseUnitAttribute : Attribute
		{
			public BaseUnitAttribute(double value)
			{
				this.Value = value;
			}
			public readonly double Value;
		}
	}
}
