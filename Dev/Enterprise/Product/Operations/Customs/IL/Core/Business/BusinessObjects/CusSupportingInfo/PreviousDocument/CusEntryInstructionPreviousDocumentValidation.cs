using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	public class CusEntryInstructionPreviousDocumentValidation : PreviousDocumentValidation
	{
		public CusEntryInstructionPreviousDocumentValidation(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
		}
	}
}
