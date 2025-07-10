using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class ContractRevocation5ULValidation : CusSupportingInfoValidation
	{
		public ContractRevocation5ULValidation(ContractRevocation5UL parent)
			: base(parent)
		{
		}
		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
		}
	}
}
