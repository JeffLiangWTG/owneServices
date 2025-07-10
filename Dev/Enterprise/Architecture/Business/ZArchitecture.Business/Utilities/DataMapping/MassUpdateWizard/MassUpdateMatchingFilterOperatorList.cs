using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.ZArchitecture.Business.Res;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class MassUpdateMatchingFilterOperatorList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Equal = "EQU";
			public const string NotEqualTo = "NOT";
			public const string Contains = "CON";
			public const string GreaterThan = "GTH";
			public const string LessThan = "LTH";
		}

		public static class Descriptions
		{
			public static string Equal => Res.GetString("471d1374-84b2-4e0a-8012-8dcfe48b3891", "Equal (=)");
			public static string NotEqualTo => Res.GetString("6d15b73d-de5f-4451-b6a3-aa3cbfe29e2d", "Not Equal To (<>)");
			public static string Contains => Res.GetString("64c35cd7-6272-481f-87c3-5be32fc4a60c", "Contains");
			public static string GreaterThan => Res.GetString("8234e796-eb63-4d41-b453-15c960136d1a", "Greater Than (>)");
			public static string LessThan => Res.GetString("f2960eee-5c5a-488b-8ca2-ba5a5c750b44", "Less Than (<)");
		}

		public MassUpdateMatchingFilterOperatorList(Type propertyType)
		{
			AddPair(Codes.Equal, Descriptions.Equal);
			AddPair(Codes.NotEqualTo, Descriptions.NotEqualTo);
			if (propertyType == typeof(ZString))
			{
				AddPair(Codes.Contains, Descriptions.Contains);
			}
			if (typeof(INumericZType).IsAssignableFrom(propertyType))
			{
				AddPair(Codes.GreaterThan, Descriptions.GreaterThan);
				AddPair(Codes.LessThan, Descriptions.LessThan);
			}
		}
	}
}
