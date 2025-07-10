using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportOfficeCodeLookups : OfficeCodeLookups
	{
		public ImportOfficeCodeLookups(OfficeCode parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList CY_CodeList => Factory.GetCachedValue("E91DBA7E-76E0-4142-AB34-5DBDC92867FA", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(EuOfficeCodesTypes.Codes.OfficeOfDischarge, EuOfficeCodesTypes.Descriptions.OfficeOfDischarge);
			result.AddPair(EuOfficeCodesTypes.Codes.OfficeOfPresentation, EuOfficeCodesTypes.Descriptions.OfficeOfPresentation);
			result.AddPair(EuOfficeCodesTypes.Codes.SupervisingOffice, EuOfficeCodesTypes.Descriptions.SupervisingOffice);
			return result;
		});
	}
}
