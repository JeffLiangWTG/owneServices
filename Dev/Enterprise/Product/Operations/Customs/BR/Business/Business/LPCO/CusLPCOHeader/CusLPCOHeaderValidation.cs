using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class CusLPCOHeaderValidation : CusPermitHeaderValidation
	{
		public CusLPCOHeaderValidation(CusLPCOHeader parent) : base(parent)
		{
		}

		protected new CusLPCOHeader Parent => (CusLPCOHeader)base.Parent;

		protected override void CheckCPH_OH_PermitHolder()
		{
			base.CheckCPH_OH_PermitHolder();
			MandatoryValidation.CheckEntered(Parent.CPH_OH_PermitHolderInfo);
		}
	}
}

