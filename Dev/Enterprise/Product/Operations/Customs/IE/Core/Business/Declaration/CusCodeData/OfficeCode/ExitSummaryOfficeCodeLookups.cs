using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExitSummaryOfficeCodeLookups : OfficeCodeLookups
	{
		public ExitSummaryOfficeCodeLookups(OfficeCode parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList CY_CodeList => Factory.GetCachedValue("122DB9B6-132E-45AB-BAE8-A5803B3B5B53", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Descriptions.OfficeOfExit);
			return result;
		});
	}
}
