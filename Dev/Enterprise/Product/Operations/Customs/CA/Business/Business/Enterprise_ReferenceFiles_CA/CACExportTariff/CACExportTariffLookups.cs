//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCACExportTariffLookups
//
//    This class should be used for overriding collections in AutoCACExportTariffLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class CACExportTariffLookups : AutoCACExportTariffLookups
	{
		public CACExportTariffLookups(AutoCACExportTariff parent)
			: base(parent)
		{
		}
	}
}
