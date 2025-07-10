using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.CA.Business
{
	public class HCPGAHeaderValidation : CusAddInfoValidation
	{
		public HCPGAHeaderValidation(AutoCusAddInfo parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDangerousGoodsDGSubs();
		}

		HCPGAHeader PGAHeader => (HCPGAHeader)Parent;

		public void ValidateDangerousGoodsDGSubs()
		{
			ValidateCalculatedProperty(PGAHeader.DangerousGoodsDGSubsInfo);
		}

		protected void CheckDangerousGoodsDGSubs()
		{
			ListValidation.ErrorIfInvalidPK(PGAHeader.DangerousGoodsDGSubsInfo);
		}
	}
}
