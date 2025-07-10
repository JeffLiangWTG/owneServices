
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public partial class ProfessionalServicesQuoteValidation : IncidentMainValidation
	{
		public ProfessionalServicesQuoteValidation(ProfessionalServicesQuote parent)
			: base(parent)
		{
		}

		protected override void CheckIM_GS_NKAssignedToCurrent()
		{
			// Do nothing.
		}

		protected override void CheckIM_GS_NKCustServiceContact()
		{
			base.CheckIM_GS_NKCustServiceContact();
			MandatoryValidation.CheckEntered(Parent.IM_GS_NKCustServiceContactInfo);
		}
	}
}

