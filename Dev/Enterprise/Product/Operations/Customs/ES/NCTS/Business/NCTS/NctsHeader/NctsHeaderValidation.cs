using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsHeaderValidation : EU.NCTS.Business.NctsHeaderValidation
	{
		public NctsHeaderValidation(NctsHeader parent) : base(parent)
		{
		}

		public new NctsHeader Parent => (NctsHeader)base.Parent;

		protected override void CheckBH_CustomsProfile()
		{
			base.CheckBH_CustomsProfile();

			var header = Parent;
			var broker = header.IsArrivalMovement ? header.ArrivalMovementHeader?.CusAgent : header.MovementHeader?.CusAgent;
			CertificateHelper.CheckCustomsProfile(header.BH_CustomsProfileInfo, header.BH_CustomsProfile, broker);
		}
	}
}
