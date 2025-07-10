//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCACCasualImpCommoditiesValidation
//
//    This class should be used for overriding validation in AutoCACCasualImpCommoditiesValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class CACCasualImpCommoditiesValidation : AutoCACCasualImpCommoditiesValidation
	{
		public CACCasualImpCommoditiesValidation(AutoCACCasualImpCommodities parent) : base(parent)
		{
		}
	}
}
