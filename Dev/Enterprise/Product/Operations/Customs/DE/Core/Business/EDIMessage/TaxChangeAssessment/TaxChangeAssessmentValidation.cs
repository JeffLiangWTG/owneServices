using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business
{
	public class TaxChangeAssessmentValidation : EDIMessageValidation
	{
		public TaxChangeAssessmentValidation(TaxChangeAssessment parent) : base(parent)
		{
		}

		protected new TaxChangeAssessment Parent => (TaxChangeAssessment)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateEntryStatus();
		}

		public void ValidateEntryStatus()
		{
			ValidateCalculatedProperty(Parent.EntryStatusInfo);
		}

		protected void CheckEntryStatus()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.EntryStatusInfo);
		}
	}
}
