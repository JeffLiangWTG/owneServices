using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NctsArrivalMovementHeaderValidation : EU.NCTS.Business.NctsArrivalMovementHeaderValidation
	{
		public NctsArrivalMovementHeaderValidation(NctsArrivalMovementHeader parent) : base(parent)
		{
		}
		new NctsArrivalMovementHeader Parent => (NctsArrivalMovementHeader)base.Parent;

		protected override void CheckGoodsLocationDescriptionCore()
		{
			base.CheckGoodsLocationDescriptionCore();
			var movementHeader = Parent;

			if (movementHeader.GoodsLocation is CusGoodsLocation location)
			{
				var info = movementHeader.GoodsLocationDescriptionInfo;
				if (location.CGL_Qualifier.IsEmpty)
				{
					info.AddMessageError(MandatoryValidation.MustBeEnteredMessage(location.CGL_QualifierInfo.Description));
				}
				if (location.CGL_Type.IsEmpty)
				{
					info.AddMessageError(MandatoryValidation.MustBeEnteredMessage(location.CGL_TypeInfo.Description));
				}
			}
		}
	}
}
