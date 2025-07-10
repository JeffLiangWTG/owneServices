namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsDepartureCargoDescCollection<T> : EU.NCTS.Business.NctsDepartureCargoDescCollection<T>
		where T : NctsDepartureCargoDesc
	{
		public NctsDepartureCargoDescCollection(NctsDepartureMovementHeader movementHeader) : base(movementHeader)
		{
		}

		public NctsDepartureCargoDescCollection(NctsBill nctsBill) : base(nctsBill)
		{
		}

		protected override void OnAdded(T businessObject)
		{
			base.OnAdded(businessObject);
			if (CopyLastGoodsItemToNewLines)
			{
				businessObject.Bill?.Header?.ApportionedAmountToGuaranteesLiabilityAmount();
			}
		}
	}
}
