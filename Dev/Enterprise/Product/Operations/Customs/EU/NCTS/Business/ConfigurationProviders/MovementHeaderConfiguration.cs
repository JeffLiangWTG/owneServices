namespace Enterprise.Customs.EU.NCTS.Business
{
	public class MovementHeaderConfiguration
	{
		public INctsMovementHeaderValidationDecider GetValidationDecider(NctsCommonMovementHeader movementHeader) => GetValidationDeciderCore(movementHeader);

		protected virtual INctsMovementHeaderValidationDecider GetValidationDeciderCore(NctsCommonMovementHeader movementHeader)
		{
			return movementHeader switch
			{
				NctsDepartureMovementHeader { IsPhase5: true } => GetDeparturePhase5ValidationDecider(),
				NctsDepartureMovementHeader { IsPhase5: false } => GetDeparturePhase4ValidationDecider(),
				NctsArrivalMovementHeader { IsPhase5: true } => GetArrivalPhase5ValidationDecider(),
				_ => null
			};
		}

		protected virtual INctsDepartureMovementHeaderPhase4ValidationDecider GetDeparturePhase4ValidationDecider() => new NctsDepartureMovementHeaderPhase4ValidationDecider();

		protected virtual INctsDepartureMovementHeaderPhase5ValidationDecider GetDeparturePhase5ValidationDecider() => new NctsDepartureMovementHeaderPhase5ValidationDecider();

		protected virtual INctsArrivalMovementHeaderPhase5ValidationDecider GetArrivalPhase5ValidationDecider() => new NctsArrivalMovementHeaderPhase5ValidationDecider();

		public ICusGoodsLocationValidationDecider GetGoodsLocationValidationDecider(NctsCommonMovementHeader movementHeader) => GetGoodsLocationValidationDeciderCore(movementHeader);

		protected virtual ICusGoodsLocationValidationDecider GetGoodsLocationValidationDeciderCore(NctsCommonMovementHeader movementHeader)
		{
			ICusGoodsLocationValidationDecider validationDecider = null;

			if (movementHeader is NctsDepartureMovementHeader)
			{
				validationDecider = movementHeader.IsPhase5 ? GetDeparturePhase5CusGoodsLocationValidationDecider() : null;
			}

			if (movementHeader is NctsArrivalMovementHeader)
			{
				validationDecider = movementHeader.IsPhase5 ? GetArrivalPhase5CusGoodsLocationValidationDecider() : null;
			}
			return validationDecider;
		}

		protected virtual ICusGoodsLocationValidationDecider GetDeparturePhase5CusGoodsLocationValidationDecider() => new DeparturePhase5CusGoodsLocationValidationDecider();

		protected virtual ICusGoodsLocationValidationDecider GetArrivalPhase5CusGoodsLocationValidationDecider() => new ArrivalPhase5CusGoodsLocationValidationDecider();
	}
}
