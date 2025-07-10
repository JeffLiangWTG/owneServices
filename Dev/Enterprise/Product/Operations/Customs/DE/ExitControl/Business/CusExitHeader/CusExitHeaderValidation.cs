using Enterprise.Customs.DE.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class CusExitHeaderValidation : EU.ExitControl.Business.CusExitHeaderValidation
	{
		public CusExitHeaderValidation(CusExitHeader parent)
			: base(parent)
		{
		}

		protected override void CheckCXH_OA_Carrier()
		{
			base.CheckCXH_OA_Carrier();

			var parent = Parent;
			if (parent.Carrier is OrgAddress carrier)
			{
				var eoriCodeIsMissing = carrier.Header.GetEUEoriDetails().IsEmpty;
				var eoriBranchIsMissing = carrier.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix).IsEmpty;
				if (eoriCodeIsMissing || eoriBranchIsMissing)
				{
					parent.CXH_OA_CarrierInfo.AddMessageError(Res.GetString("381AA07B-FDE4-451D-A075-E0E1D9E894C0", "Carrier is missing {0}", EORIHelper.GetMissingEoriMessage(eoriCodeIsMissing, eoriBranchIsMissing)));
				}
			}
		}
	}
}
