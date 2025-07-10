namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class AlternativeEvidenceValidation : EU.ExitControl.Business.AlternativeEvidenceValidation
	{
		public AlternativeEvidenceValidation(AlternativeEvidence parent)
			: base(parent)
		{
		}

		protected new AlternativeEvidence Parent => (AlternativeEvidence)base.Parent;
	}
}
