//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientFeatureRequestValueAndContributionValidation
//
//    This class should be used for overriding validation in AutoClientFeatureRequestValueAndContributionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class ClientFeatureRequestValueAndContributionValidation : AutoClientFeatureRequestValueAndContributionValidation
	{
		public ClientFeatureRequestValueAndContributionValidation(AutoClientFeatureRequestValueAndContribution parent)
			: base(parent)
		{
		}

		protected override void CheckT9_EBV()
		{
			if (Parent.ShouldValidateMandatoryEBV)
			{
				MandatoryValidation.CheckEntered(Parent.T9_EBVInfo);
			}
		}

		protected new ClientFeatureRequestValueAndContribution Parent
		{
			get { return (ClientFeatureRequestValueAndContribution)base.Parent; }
		}
	}
}

