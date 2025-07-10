
namespace Enterprise.Customs.DE.NCTS.Business
{
	public class GuaranteeTransactionCoordinator : EU.NCTS.Business.GuaranteeTransactionCoordinator
	{
		public GuaranteeTransactionCoordinator(EU.NCTS.Business.NctsDepartureMovementHeader movementHeader) : base(movementHeader)
		{
		}

		protected override bool ShouldCreateTransactionForBondType(string bondType) =>
			bondType switch
			{
				NctsGuaranteeTypeList.Codes._0 => true,
				NctsGuaranteeTypeList.Codes._1 => true,
				_ => false
			};
	}
}
