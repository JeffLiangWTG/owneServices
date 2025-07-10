using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CA.Business
{
	static class CodeDescriptionExtension
	{
		public static ZString GetCodeDescriptionWithSpace(ZString code, ICodeDescriptionPairList list)
		{
			return code + " " + list.GetDescriptionFromCode(code);
		}

		public static ZString GetCodeDescriptionWithSpace(ZString code, ZZRefCusCodeListCombinedCollection collection)
		{
			collection.Load();
			var description = collection.Cast<ZZRefCusCodeListCombined>().FirstOrDefault(x => x.ZZD_Code == code)?.ZZD_Description ?? ZString.Empty;
			return code + " " + description;
		}
	}
}
