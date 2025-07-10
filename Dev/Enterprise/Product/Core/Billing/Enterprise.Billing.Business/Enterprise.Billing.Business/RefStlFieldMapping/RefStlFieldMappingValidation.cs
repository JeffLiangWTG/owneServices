//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefStlFieldMappingValidation
//
//    This class should be used for overriding validation in AutoRefStlFieldMappingValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Billing.Business
{
	public class RefStlFieldMappingValidation : AutoRefStlFieldMappingValidation
	{
		public RefStlFieldMappingValidation(AutoRefStlFieldMapping parent) : base(parent)
		{
		}
	}
}
