//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoIncidentDiagnosticCriteriaValidation
//
//    This class should be used for overriding validation in AutoIncidentDiagnosticCriteriaValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	using CargoWise.EntityFramework;
	using Res = ZClientEDI.Business.Res;

	public class IncidentDiagnosticCriteriaValidation : AutoIncidentDiagnosticCriteriaValidation
	{
		public IncidentDiagnosticCriteriaValidation(AutoIncidentDiagnosticCriteria parent) : base(parent)
		{
		}

		new IncidentDiagnosticCriteria Parent
		{
			get { return (IncidentDiagnosticCriteria)base.Parent; }
		}

		protected override void CheckIMD_Type()
		{
			base.CheckIMD_Type();
			MandatoryValidation.CheckEntered(Parent.IMD_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.IMD_TypeInfo);
		}

		protected override void CheckIMD_Question()
		{
			base.CheckIMD_Question();
			if (Parent.IMD_QuestionInfo.Value.IsEmpty && Parent.IMD_InternalSupportNoteInfo.Value.IsEmpty)
			{
				Parent.IMD_QuestionInfo.AddError(Res.GetString("fe930791-27b8-4209-ad03-ba35c69b400f", "Please enter either a client question or an internal support note, or both."));
			}
		}

		protected override void CheckIMD_InternalSupportNote()
		{
			base.CheckIMD_InternalSupportNote();
			if (Parent.IMD_InternalSupportNoteInfo.Value.IsEmpty && Parent.IMD_QuestionInfo.Value.IsEmpty)
			{
				Parent.IMD_InternalSupportNoteInfo.AddError(Res.GetString("f8fdab5f-eb17-453c-bb35-39251df02645", "Please enter either a client question or an internal support note, or both."));
			}
		}

		protected override void CheckIMD_Keywords()
		{
			base.CheckIMD_Keywords();
			MandatoryValidation.CheckEntered(Parent.IMD_KeywordsInfo);
		}

		protected override void CheckIMD_Description()
		{
			base.CheckIMD_Description();
			MandatoryValidation.CheckEntered(Parent.IMD_DescriptionInfo);
		}
	}
}


