using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsDepartureCargoDescPhase4Validation : EU.NCTS.Business.NctsDepartureCargoDescPhase4Validation
	{
		public NctsDepartureCargoDescPhase4Validation(NctsDepartureCargoDesc parent)
			: base(parent)
		{ }

		protected override void CheckBY_HarmonisedTariff()
		{
			base.CheckBY_HarmonisedTariff();
			var commodityCode = Parent.BY_HarmonisedTariff;
			var header = Parent.Header;
			if (header != null)
			{
				foreach (var guarantee in header.Guarantees)
				{
					if (commodityCode.IsEmpty && guarantee.PW_BondType == EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_HarmonisedTariffInfo);
					}
				}
			}
		}
	}
}
