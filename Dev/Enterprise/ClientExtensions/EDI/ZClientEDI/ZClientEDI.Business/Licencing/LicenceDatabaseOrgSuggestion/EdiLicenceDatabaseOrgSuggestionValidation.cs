//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiLicenceDatabaseOrgSuggestionValidation
//
//    This class should be used for overriding validation in AutoEdiLicenceDatabaseOrgSuggestionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class EdiLicenceDatabaseOrgSuggestionValidation : AutoEdiLicenceDatabaseOrgSuggestionValidation
	{
		public EdiLicenceDatabaseOrgSuggestionValidation(AutoEdiLicenceDatabaseOrgSuggestion parent) : base(parent)
		{
		}
	}
}
