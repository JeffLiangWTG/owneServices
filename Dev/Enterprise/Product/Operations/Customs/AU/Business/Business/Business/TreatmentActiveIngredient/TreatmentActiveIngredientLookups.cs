using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class TreatmentActiveIngredientLookups : CusCodeDataLookups
	{
		public TreatmentActiveIngredientLookups(TreatmentActiveIngredient parent) : base(parent)
		{
		}
		public override CodeDescriptionPairList CY_CodeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EXDOCSTreatmentActiveIngredient, ZDateTime.Today);
	}
}
