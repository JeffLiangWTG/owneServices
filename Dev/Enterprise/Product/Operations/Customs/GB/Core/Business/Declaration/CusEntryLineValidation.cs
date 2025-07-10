
namespace Enterprise.Customs.GB.Business.Declaration
{
	public class CusEntryLineValidation : EU.Business.Declaration.CusEntryLineValidation
	{
		public CusEntryLineValidation(EU.Business.Declaration.CusEntryLine parent)
			: base(parent)
		{
		}

		protected override void CheckCL_LineNumber()
		{
			base.CheckCL_LineNumber();

			var maxCount = ((JobDeclaration)Parent.Declaration).MaximumEntryLineCount;

			if (Parent.CL_LineNumber > maxCount && Parent.CL_CustomsPostedStatus != Customs.Business.EntryLineStatusList.Codes.Deleted)
			{
				Parent.CL_LineNumberInfo.AddMessageError(System.FormattableString.Invariant($"The maximum number of entry lines permitted is {maxCount}"));
			}
		}

		protected override void CheckCL_CustomsPostedStatus()
		{
			ValidateCL_LineNumber();
		}
	}
}
