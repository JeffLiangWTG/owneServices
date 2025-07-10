//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoKROrgImpAddInfoValidation
//
//    This class should be used for overriding validation in AutoKROrgImpAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class KROrgImpAddInfoValidation : AutoKROrgImpAddInfoValidation
	{
		public KROrgImpAddInfoValidation(AutoKROrgImpAddInfo parent) : base(parent)
		{
		}
		new AutoKROrgImpAddInfo Parent
		{
			get { return base.Parent; }
		}

		protected override void CheckZO_BankCode()
		{
			base.CheckZO_BankCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_BankCodeInfo, Parent.Lookups.BankTypeList);
		}

		protected override void CheckZO_VATDeferment()
		{
			base.CheckZO_VATDeferment();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_VATDefermentInfo, Parent.Lookups.VATDefermentType);
		}
	}
}

