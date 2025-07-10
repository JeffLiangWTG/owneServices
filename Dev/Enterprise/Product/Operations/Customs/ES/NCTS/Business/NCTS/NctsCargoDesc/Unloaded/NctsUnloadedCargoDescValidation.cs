using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsUnloadedCargoDescValidation : EU.NCTS.Business.NctsUnloadedCargoDescValidation
	{
		public NctsUnloadedCargoDescValidation(EU.NCTS.Business.NctsUnloadedCargoDesc parent) : base(parent)
		{
		}

		protected new NctsUnloadedCargoDesc Parent => (NctsUnloadedCargoDesc)base.Parent;

		protected override void CheckBY_HarmonisedTariffIsValid()
		{
			if (Parent.BY_HarmonisedTariff.Length == 8)
			{
				var parent = Parent;
				var tariffCollection = TariffViewCollection.GetCachedCollection(parent.Factory, Parent.DataGroupingCode, Constants.TariffTypes.Export, parent.ValuationDate);
				ListValidation.MessageErrorIfInvalidCode(Parent.BY_HarmonisedTariffInfo, tariffCollection);
			}
			else
			{
				base.CheckBY_HarmonisedTariffIsValid();
			}
		}
	}
}
