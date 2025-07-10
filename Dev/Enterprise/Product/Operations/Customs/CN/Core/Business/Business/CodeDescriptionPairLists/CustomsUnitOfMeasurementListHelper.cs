using System.Collections.Immutable;

namespace Enterprise.Customs.CN.Business
{
	public static class CustomsUnitOfMeasurementListHelper
	{
		#region Weight Units
		public static class Codes
		{
			public const string Metres = "030";
			public const string SquareMetres = "032";
			public const string CubicMetres = "033";
			public const string Kilograms = "035";
			public const string Grams = "036";
			public const string Quintals = "047";
			public const string Tons = "070";
			public const string LongTones = "071";
			public const string ShortTones = "072";
			public const string SimaDAns = "073";
			public const string SimaJins = "074";
			public const string Catties = "075";
			public const string Pounds = "076";
			public const string Piculs = "077";
			public const string Hundredweights = "078";
			public const string ShortDans = "079";
			public const string Taels = "080";
			public const string Dans = "081";
			public const string Ounces = "083";
			public const string Carats = "084";
		}

		#endregion

		#region List

		static readonly ImmutableArray<string> WeightUOMList = ImmutableArray.Create(
			Codes.Kilograms, Codes.Grams, Codes.Quintals, Codes.Tons,
			Codes.LongTones, Codes.ShortTones, Codes.SimaDAns, Codes.SimaJins, Codes.Catties, Codes.Pounds, Codes.Piculs,
			Codes.Hundredweights, Codes.ShortDans, Codes.Taels, Codes.Dans, Codes.Ounces, Codes.Carats
		);

		#endregion

		public static bool IsWeightUnit(string code)
		{
			return WeightUOMList.Contains(code);
		}
	}
}
