using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportUCC5OfficeCodeLookups : OfficeCodeLookups
	{
		public ImportUCC5OfficeCodeLookups(OfficeCode parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList CY_CodeList => Factory.GetCachedValue("", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(EuOfficeCodesTypes.Codes.OfficeOfDischarge, EuOfficeCodesTypes.Descriptions.OfficeOfDischarge);
			result.AddPair(EuOfficeCodesTypes.Codes.OfficeOfPresentation, ResString.GetMultilingualString("75F6B861-AFD5-4E0F-9E84-02441195364D", "[5/26] Customs office of presentation"));
			result.AddPair(EuOfficeCodesTypes.Codes.SupervisingOffice, ResString.GetMultilingualString("F0F38B87-D9DB-4F42-833D-50994ABEB42A", "[5/27] Supervising customs office"));
			return result;
		});
	}
}
