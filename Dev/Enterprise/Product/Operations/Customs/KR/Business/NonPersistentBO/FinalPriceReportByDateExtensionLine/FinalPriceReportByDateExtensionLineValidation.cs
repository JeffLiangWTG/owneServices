using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class FinalPriceReportByDateExtensionLineValidation : AutoFinalPriceReportByDateExtensionLineValidation
	{
		public FinalPriceReportByDateExtensionLineValidation(AutoFinalPriceReportByDateExtensionLine parent) : base(parent)
		{
		}

		protected new FinalPriceReportByDateExtensionLine Parent => (FinalPriceReportByDateExtensionLine)base.Parent;

		protected override void CheckImportDeclarationNumber()
		{
			base.CheckImportDeclarationNumber();
			MandatoryValidation.CheckEntered(Parent.ImportDeclarationNumberInfo);
		}

		protected override void CheckExtensionDate()
		{
			base.CheckExtensionDate();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ExtensionDateInfo);
		}

		protected override void CheckApplicationReason()
		{
			base.CheckApplicationReason();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ApplicationReasonInfo);
		}
	}
}
