using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	partial class MessageStatusCodeList
	{
		public static string GetMappedCode(BusinessObjectFactory factory, ZString country, ZString code, ZString manifestType)
		{
			var attrValue = ZZRefCusCodeListCombined.Loader.Load(
				factory,
				country,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus,
				ZDateTime.Now,
				new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, code),
				true
			).Where(
				x => x.HasAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, manifestType)
			).OrderBy(
				x => x.ZZD_StartDate
			).FirstOrDefault()?
			.Attributes.Cast<ZZRefCusCodeListAttributeCombined>()
			.FirstOrDefault(x => x.ZZE_ZXE_NKName.EqualsIgnoringCase(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared))?
			.ZZE_Value;
			return !attrValue.HasValue ? Codes.Unknown : (!attrValue.Value.IsEmpty && ZBool.ParseSafe(attrValue.Value, false) ? Codes.Accepted : Codes.Error);
		}

		public static bool HasBeenSentCustoms(ZString code)
		{
			return !(code.IsEmpty || code == Codes.NotSent || code == Codes.Error);
		}
	}
}
