using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class FinalPriceReportByDateExtensionHeaderValidation : AutoFinalPriceReportByDateExtensionHeaderValidation
	{
		public FinalPriceReportByDateExtensionHeaderValidation(AutoFinalPriceReportByDateExtensionHeader parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateHasExtensionLines();
		}

		public void ValidateHasExtensionLines()
		{
			ValidateCalculatedProperty(Parent.HasExtensionLinesInfo);
		}
		protected void CheckHasExtensionLines()
		{
			if (!Parent.HasExtensionLines)
			{
				Parent.HasExtensionLinesInfo.AddError(Res.GetString("7228C848-5ECD-4F33-9A4E-B0E0022A9865", "You must enter at least one line."));
			}
		}

		protected override void CheckCustomsOffice()
		{
			base.CheckCustomsOffice();
			MandatoryValidation.CheckEntered(Parent.CustomsOfficeInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.CustomsOfficeInfo);
		}

		protected override void CheckGB_Branch()
		{
			base.CheckGB_Branch();
			if (Parent.GB_Branch.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.GB_BranchInfo);
			}
			else
			{
				var branch = Parent.Branch;
				if (branch == null)
				{
					Parent.GB_BranchInfo.AddMessageError(Res.GetString("84C88143-0E34-42A7-A8B1-19184A210767", "Please enter a valid branch."));
				}
			}
		}

		new FinalPriceReportByDateExtensionHeader Parent => (FinalPriceReportByDateExtensionHeader)base.Parent;
	}
}
