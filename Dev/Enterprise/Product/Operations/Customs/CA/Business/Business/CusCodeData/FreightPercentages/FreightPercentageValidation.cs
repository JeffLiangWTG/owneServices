using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class FreightPercentageValidation : CusCodeDataValidation
	{
		public FreightPercentageValidation(FreightPercentage parent)
			: base(parent)
		{
		}

		#region CheckCY_Code

		protected override void CheckCY_Code()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CY_CodeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.CY_CodeInfo);
		}

		#endregion

	}
}
