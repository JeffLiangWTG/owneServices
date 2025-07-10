using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NonPersistentJobTypeOptionValidation : AutoNonPersistentJobTypeOptionValidation
	{
		public NonPersistentJobTypeOptionValidation(AutoNonPersistentJobTypeOption parent)
			: base(parent)
		{
		}

		protected override void CheckJobType()
		{
			var targetInfo = Parent.JobTypeInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			ListValidation.ErrorIfInvalidCode(targetInfo);
		}
	}
}
