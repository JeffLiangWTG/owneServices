//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoFeatureControlSetValidation
//
//    This class should be used for overriding validation in AutoFeatureControlSetValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.FeatureControl.Business
{
	public class FeatureControlSetValidation : AutoFeatureControlSetValidation
	{
		public FeatureControlSetValidation(AutoFeatureControlSet parent) : base(parent)
		{
		}
	}
}
