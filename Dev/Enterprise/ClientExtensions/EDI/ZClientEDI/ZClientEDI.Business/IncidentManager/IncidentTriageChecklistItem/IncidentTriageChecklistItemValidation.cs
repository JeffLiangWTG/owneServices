//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoIncidentTriageChecklistItemValidation
//
//    This class should be used for overriding validation in AutoIncidentTriageChecklistItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentTriageChecklistItemValidation : AutoIncidentTriageChecklistItemValidation
	{
		public IncidentTriageChecklistItemValidation(AutoIncidentTriageChecklistItem parent) : base(parent)
		{
		}

		new IncidentTriageChecklistItem Parent
		{
			get { return (IncidentTriageChecklistItem)base.Parent; }
		}

		protected override void CheckIMC_SupportDescription()
		{
			base.CheckIMC_SupportDescription();
			MandatoryValidation.CheckEntered(Parent.IMC_SupportDescriptionInfo);
		}

		protected override void CheckIMC_Category()
		{
			base.CheckIMC_Category();
			ListValidation.ErrorIfInvalidCode(Parent.IMC_CategoryInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePublishedDescriptionText();
		}

		public void ValidatePublishedDescriptionText()
		{
			ValidateCalculatedProperty(Parent.PublishedDescriptionTextInfo);
		}

		protected void CheckPublishedDescriptionText()
		{
			if (Parent.IMC_IsPublished)
			{
				MandatoryValidation.CheckEntered(Parent.PublishedDescriptionTextInfo);
			}
		}
	}
}
