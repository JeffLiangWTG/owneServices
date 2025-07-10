
//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEUAddInfoTaxValidation
//
//    This class should be used for overriding validation in AutoEUAddInfoTaxValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class EUAddInfoTaxValidation : AutoEUAddInfoTaxValidation
	{
		public EUAddInfoTaxValidation(AutoEUAddInfoTax parent) : base(parent)
		{
		}

		protected override void CheckG4_Type()
		{
			base.CheckG4_Type();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.G4_TypeInfo);
			ValidateAllTypeAndMopColumnsForAllRowsOfThisType();
		}

		void ValidateAllTypeAndMopColumnsForAllRowsOfThisType()
		{
			var pivot = Parent.Pivot;
			if (pivot != null)
			{
				foreach (CusAddInfo<Tax_CusAddInfoOnlyForPIVOT> brotherTax in pivot.Taxes)
				{
					brotherTax.Data.Validation.ValidateG4_MethodOfPayment();
					brotherTax.Data.Validation.ValidateG4_Type();
				}
			}
		}

		void CheckTaxTypeAndMop(ZPropertyInfo zPropertyInfoForError)
		{
			var cai = Parent.Parent as CusAddInfo;
			if (cai != null)
			{
				var pivot = Parent.Pivot;
				if (pivot != null)
				{
					if (pivot.Taxes.Cast<CusAddInfo<Tax_CusAddInfoOnlyForPIVOT>>()
						.Any(brotherTax =>
							brotherTax.PK != cai.PK
							&& brotherTax.Data.G4_Type == Parent.G4_Type
							&& brotherTax.Data.G4_MethodOfPayment == Parent.G4_MethodOfPayment)
					)
					{
						zPropertyInfoForError.AddMessageError(Res.GetString("460EE869-EBDC-451C-B5E4-6BA99C0C6878", "A row with that tax type and method of payment already exists"));
					}
				}
			}
		}

		protected override void CheckG4_MethodOfPayment()
		{
			base.CheckG4_MethodOfPayment();
			ListValidation.MessageErrorIfInvalidCode(Parent.G4_MethodOfPaymentInfo, Parent.Lookups.MOPList);
			CheckTaxTypeAndMop(Parent.G4_MethodOfPaymentInfo);
			ValidateAllTypeAndMopColumnsForAllRowsOfThisType();
		}

		protected override void CheckG4_RateDuty()
		{
			base.CheckG4_RateDuty();
			ListValidation.MessageErrorIfInvalidCode(Parent.G4_RateDutyInfo, Parent.Lookups.RateDutyList);
		}

		protected new Tax_CusAddInfoOnlyForPIVOT Parent => (Tax_CusAddInfoOnlyForPIVOT)base.Parent;
	}
}
