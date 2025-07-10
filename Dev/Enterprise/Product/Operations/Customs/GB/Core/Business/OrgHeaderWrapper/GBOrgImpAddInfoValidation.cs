//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEUOrgImpAddInfoValidation
//
//    This class should be used for overriding validation in AutoEUOrgImpAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.GB.Business
{
	public class GBOrgImpAddInfoValidation : AutoGBOrgImpAddInfoValidation
	{
		public GBOrgImpAddInfoValidation(AutoGBOrgImpAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckZO_VATDeferType()
		{
			base.CheckZO_VATDeferType();

			var parent = (GBOrgImpAddInfo)Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.ZO_VATDeferTypeInfo);

			var eUOrgImpAddInfo = EUOrgImpAddInfo.Get(parent.Organisation, Enterprise.Core.Constants.CountryCodes.UnitedKingdom);
			if (eUOrgImpAddInfo != null)
			{
				if (!parent.ZO_VATDeferType.IsEmpty && eUOrgImpAddInfo.ZO_UseFr3FiscalRepresentation)
				{
					parent.ZO_VATDeferTypeInfo.AddMessageError("This field is mutually exclusive with EU 'Use postponed VAT accounting?' field.");
				}
			}
		}

		protected override void CheckZO_Box44ClientsDucrSourceAttributeField()
		{
			base.CheckZO_Box44ClientsDucrSourceAttributeField();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_Box44ClientsDucrSourceAttributeFieldInfo);
		}
	}
}
