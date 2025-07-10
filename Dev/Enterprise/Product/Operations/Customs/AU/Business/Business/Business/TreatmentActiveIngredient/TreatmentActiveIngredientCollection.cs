using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class TreatmentActiveIngredientCollection : CusCodeDataCollection<TreatmentActiveIngredient>
	{
		public TreatmentActiveIngredientCollection(BusinessObject parent) : base(parent, CusCodeDataTypeList.Codes.EXDOCTreatmentActiveIngredient)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var ingredient = (TreatmentActiveIngredient)child;
			ingredient.CY_Order = new ZShort(NextOrderNumber);
		}

		short NextOrderNumber => Select(x => x.CY_Order).OrderByDescending(x => x).FirstOrDefault() + 1;
	}
}
