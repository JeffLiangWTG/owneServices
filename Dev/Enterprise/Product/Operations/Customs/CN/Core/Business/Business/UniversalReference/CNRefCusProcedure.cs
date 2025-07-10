using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public static class CNRefCusProcedure
	{
		#region Codes

		public static class Codes
		{
			public const string _0110 = "0110";
			public const string _0130 = "0130";
			public const string _0139 = "0139";
			public const string _0200 = "0200";
			public const string _0214 = "0214";
			public const string _0243 = "0243";
			public const string _0245 = "0245";
			public const string _0255 = "0255";
			public const string _0258 = "0258";
			public const string _0265 = "0265";
			public const string _0300 = "0300";
			public const string _0314 = "0314";
			public const string _0320 = "0320";
			public const string _0345 = "0345";
			public const string _0400 = "0400";
			public const string _0420 = "0420";
			public const string _0444 = "0444";
			public const string _0445 = "0445";
			public const string _0446 = "0446";
			public const string _0456 = "0456";
			public const string _0466 = "0466";
			public const string _0500 = "0500";
			public const string _0513 = "0513";
			public const string _0544 = "0544";
			public const string _0545 = "0545";
			public const string _0615 = "0615";
			public const string _0642 = "0642";
			public const string _0644 = "0644";
			public const string _0654 = "0654";
			public const string _0657 = "0657";
			public const string _0664 = "0664";
			public const string _0700 = "0700";
			public const string _0715 = "0715";
			public const string _0744 = "0744";
			public const string _0815 = "0815";
			public const string _0844 = "0844";
			public const string _0845 = "0845";
			public const string _0864 = "0864";
			public const string _0865 = "0865";
			public const string _1039 = "1039";
			public const string _1139 = "1139";
			public const string _1200 = "1200";
			public const string _1210 = "1210";
			public const string _1215 = "1215";
			public const string _1233 = "1233";
			public const string _1234 = "1234";
			public const string _1239 = "1239";
			public const string _1300 = "1300";
			public const string _1371 = "1371";
			public const string _1427 = "1427";
			public const string _1500 = "1500";
			public const string _1523 = "1523";
			public const string _1616 = "1616";
			public const string _1741 = "1741";
			public const string _1831 = "1831";
			public const string _2025 = "2025";
			public const string _2210 = "2210";
			public const string _2225 = "2225";
			public const string _2439 = "2439";
			public const string _2600 = "2600";
			public const string _2700 = "2700";
			public const string _2939 = "2939";
			public const string _3010 = "3010";
			public const string _3039 = "3039";
			public const string _3422 = "3422";
			public const string _3100 = "3100";
			public const string _3339 = "3339";
			public const string _3410 = "3410";
			public const string _3511 = "3511";
			public const string _3611 = "3611";
			public const string _3612 = "3612";
			public const string _3910 = "3910";
			public const string _3939 = "3939";
			public const string _4019 = "4019";
			public const string _4039 = "4039";
			public const string _4200 = "4200";
			public const string _4239 = "4239";
			public const string _4400 = "4400";
			public const string _4500 = "4500";
			public const string _4539 = "4539";
			public const string _4561 = "4561";
			public const string _4600 = "4600";
			public const string _5000 = "5000";
			public const string _5010 = "5010";
			public const string _5014 = "5014";
			public const string _5015 = "5015";
			public const string _5033 = "5033";
			public const string _5034 = "5034";
			public const string _5100 = "5100";
			public const string _5200 = "5200";
			public const string _5300 = "5300";
			public const string _5335 = "5335";
			public const string _5361 = "5361";
			public const string _6033 = "6033";
			public const string _9600 = "9600";
			public const string _9610 = "9610";
			public const string _9639 = "9639";
			public const string _9700 = "9700";
			public const string _9739 = "9739";
			public const string _9800 = "9800";
			public const string _9839 = "9839";
			public const string _9900 = "9900";
		}

		#endregion

		public static RefCusProcedure GetRefCusProcedure(BusinessObjectFactory factory, ZString procedureCode, ZDateTime dateOfValuation)
		{
			return procedureCode.IsEmpty ? null : new RefCusProcedure.Loader(factory).LoadTop1FromCodeAndCountry(procedureCode, ZString.Empty, Core.Constants.CountryCodes.China, dateOfValuation);
		}

		public static CodeDescriptionPairList GetRefCusProcedureList(BusinessObjectFactory factory)
		{
			var countryCode = MasterFiles.Business.GlbCompany.CurrentCompany.Country.Code;
			var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}_RefCusProcedures_ProcedureCode", countryCode);
			return factory.GetCachedValue(cacheKey, () =>
			{
				var result = new CodeDescriptionPairList();
				var procedures = new RefCusProcedure.Loader(factory).LoadForZzzDataGrouping(countryCode);
				foreach (var procedure in procedures)
				{
					result.AddPair(procedure.ZZ6_ProcedureCode, procedure.ZZ6_Description);
				}
				result.Sort();
				return result;
			});
		}
	}
}
