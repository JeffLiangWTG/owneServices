//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCACExportTariffValidation
//
//    This class should be used for overriding validation in AutoCACExportTariffValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class CACExportTariffValidation : AutoCACExportTariffValidation
	{
		public CACExportTariffValidation(AutoCACExportTariff parent)
			: base(parent)
		{
		}
	}
}
