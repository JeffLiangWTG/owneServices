using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.GB.Business
{
	public class EntrySubStyleCodeList : ZZRefCusCodeListCombinedCollection, ICodeDescriptionPairList
	{
		public EntrySubStyleCodeList(BusinessObjectFactory factory) : base(factory)
		{ }

		protected override ZQuery CreateRelationshipFilter()
		{
			return ZZRefCusCodeListCombined.Loader.GetFilter(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub, ZDateTime.Today, (ZQuery)null, false);
		}

		public bool ContainsCode(object code)
		{
			return this.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == code.ToString());
		}

		public string GetDescriptionFromCode(string code)
		{
			return this.Cast<ZZRefCusCodeListCombined>()
				.FirstOrDefault(x => x.ZZD_Code == code)
				?.ZZD_Description;
		}

		public static class Codes
		{
			public const string A = "A";
			public const string B = "B";
			public const string C = "C";
			public const string D = "D";
			public const string E = "E";
			public const string F = "F";
			public const string J = "J";
			public const string K = "K";
			public const string Q = "Q";
			public const string X = "X";
			public const string Y = "Y";
			public const string Z = "Z";
		}
	}
}
