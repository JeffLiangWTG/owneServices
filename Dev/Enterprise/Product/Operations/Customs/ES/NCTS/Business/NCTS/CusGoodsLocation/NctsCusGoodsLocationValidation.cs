using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsCusGoodsLocationValidation : CusGoodsLocationValidation
	{
		public NctsCusGoodsLocationValidation(CusGoodsLocation parent) : base(parent)
		{
		}

		new NctsCusGoodsLocation Parent => (NctsCusGoodsLocation)base.Parent;

		NctsHeader Header => Parent?.Header;

		protected override void CheckCGL_AdditionalIdentifier()
		{
			base.CheckCGL_AdditionalIdentifier();

			var nctsHeader = Header;
			if (nctsHeader != null && nctsHeader.IsPhase5 && nctsHeader.IsPhaseStatusTNN)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CGL_AdditionalIdentifierInfo, Res.GetString("EAE898A0-4C45-4A9C-B22E-F744715F3AA9", "value in Arrival Notification/Arrival Details/Arrival Goods Location"));
			}
		}
	}
}
