using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CO.Manifest.Business
{
	public class AsycudaBillValidationForMasterChild : ASYCUDA.Business.AsycudaBillValidationForMasterChild
	{
		public AsycudaBillValidationForMasterChild(ASYCUDA.Business.AsycudaBill parent) : base(parent)
		{
		}

		protected override void CheckABL_BillIssueDate()
		{
			base.CheckABL_BillIssueDate();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_BillIssueDateInfo);
		}

		protected override void CheckABL_RL_NKPortOfDischarge()
		{
			base.CheckABL_RL_NKPortOfDischarge();

			if (!Parent.ABL_RL_NKPortOfDischarge.IsEmpty)
			{
				var uNLOCO = Parent.Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, Parent.ABL_RL_NKPortOfDischarge));

				if (uNLOCO != null && uNLOCO.RL_RW.IsEmpty)
				{
					Parent.ABL_RL_NKPortOfDischargeInfo.AddMessageError(ResString.GetMultilingualString("D0D3F733-84D3-4FFC-B084-E293EEC0A14C", "The selected Discharge Port should have State entered"));
				}
			}
		}
	}
}
