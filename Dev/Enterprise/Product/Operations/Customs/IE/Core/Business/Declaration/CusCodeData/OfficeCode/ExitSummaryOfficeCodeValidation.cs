using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExitSummaryOfficeCodeValidation : CommonExportOfficeCodeValidation
	{
		public ExitSummaryOfficeCodeValidation(OfficeCode parent) : base(parent)
		{
		}

		protected override bool WasDataChanged(JobDeclaration declaration, ZString code)
		{
			return code == EuOfficeCodesTypes.Codes.OfficeOfPresentation ? IsOfficeCodeChanged(declaration.OriginalPresentationOffice) : base.WasDataChanged(declaration, code);
		}
	}
}
