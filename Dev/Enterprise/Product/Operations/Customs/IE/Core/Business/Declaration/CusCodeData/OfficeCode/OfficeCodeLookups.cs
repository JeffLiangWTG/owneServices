using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class OfficeCodeLookups : EuOfficeCodeLookups
	{
		public OfficeCodeLookups(OfficeCode parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList CY_CodeList => Factory.GetCachedValue("86f4c356-3304-4c64-ae7e-f7d5a1f9aff1", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(EuOfficeCodesTypes.Codes.OfficeOfPresentation, EuOfficeCodesTypes.Descriptions.OfficeOfPresentation);
			result.AddPair(EuOfficeCodesTypes.Codes.SupervisingOffice, EuOfficeCodesTypes.Descriptions.SupervisingOffice);
			result.AddPair(EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Descriptions.OfficeOfExit);
			return result;
		});
	}
}
