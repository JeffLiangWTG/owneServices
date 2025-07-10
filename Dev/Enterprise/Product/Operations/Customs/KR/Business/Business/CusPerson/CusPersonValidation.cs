namespace Enterprise.Customs.KR.Business
{
	public class CusPersonValidation : Customs.Business.CusPersonValidation
	{
		public CusPersonValidation(CusPerson parent)
			: base(parent)
		{
		}

		public new CusPerson Parent => (CusPerson)base.Parent;

		public void ValidateRelationshipToDeclarant()
		{
			ValidateCalculatedProperty(Parent.RelationshipToDeclarantInfo);
		}

		protected virtual void CheckRelationshipToDeclarant()
		{
		}

		public void ValidateJobCode()
		{
			ValidateCalculatedProperty(Parent.JobCodeInfo);
		}

		protected virtual void CheckJobCode()
		{
		}

		public void ValidateEntryStatus()
		{
			ValidateCalculatedProperty(Parent.EntryStatusInfo);
		}

		protected virtual void CheckEntryStatus()
		{
		}

		public void ValidateResidencyStartDate()
		{
			ValidateCalculatedProperty(Parent.ResidencyStartDateInfo);
		}

		protected virtual void CheckResidencyStartDate()
		{
		}

		public void ValidateResidencyEndDate()
		{
			ValidateCalculatedProperty(Parent.ResidencyEndDateInfo);
		}

		protected virtual void CheckResidencyEndDate()
		{
		}

		public void ValidateNationalityClassCode()
		{
			ValidateCalculatedProperty(Parent.NationalityClassCodeInfo);
		}

		protected virtual void CheckNationalityClassCode()
		{
		}
	}
}
