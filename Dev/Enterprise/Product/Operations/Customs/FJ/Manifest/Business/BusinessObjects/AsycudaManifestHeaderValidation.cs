using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FJ.Manifest.Business
{
	public class AsycudaManifestHeaderValidation : ASYCUDA.Business.AsycudaManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(ASYCUDA.Business.AsycudaManifestHeader parent) : base(parent)
		{
		}

		protected override void CheckAMA_RN_NKConveyanceNationalityCore()
		{
			if (Parent.IsSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_RN_NKConveyanceNationalityInfo);
			}
		}

		protected override void CheckAMA_OA_ShippingAgent()
		{
			base.CheckAMA_OA_ShippingAgent();
			var shippingAgent = Parent.ShippingAgentOrg;

			if (shippingAgent != null)
			{
				var shippingAgentFJ_CCD = shippingAgent.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientCode, CountryCodes.Fiji);
				if (shippingAgentFJ_CCD.IsEmpty)
				{
					Parent.AMA_OA_ShippingAgentInfo.AddMessageError(Res.GetString("B29BAE0E-7028-4C4B-879B-E67E360F9B8A", "A Shipping Agent code cannot be determined. This organization does not contain a CCD code for FJ."));
				}
			}
		}
	}
}
