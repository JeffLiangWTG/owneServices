using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class TreatmentActiveIngredientValidation : CusCodeDataValidation
	{
		public TreatmentActiveIngredientValidation(TreatmentActiveIngredient parent) : base(parent)
		{
		}

		protected new TreatmentActiveIngredient Parent => (TreatmentActiveIngredient)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateProduceType();
		}

		void ValidateProduceType()
		{
			var produceType = new ZString(Parent.QuarantineExDocEstablishmentAndTime?.QuarantineExDocLine?.QL_ProduceType);
			var messageError = Res.GetString("TreatmentActiveIngredientValidation|CheckAllMaximumCollectionsAmounts", "Treatment Active Ingredients may only be present when Produce Type is Horticulture or Grains and Seeds");

			Parent.RemoveRowMessageError(messageError);

			if (!(produceType == EXDOCCommodityCodes.Codes.Horticulture || produceType == EXDOCCommodityCodes.Codes.GrainsAndPlants))
			{
				Parent.AddRowMessageError(messageError);
			}
		}
	}
}
