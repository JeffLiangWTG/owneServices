
namespace CargoWise.EntityFramework.Testing
{
	public class DummyBizoValidation : AutoDummyBizoValidation
	{
		public DummyBizoValidation(AutoDummyBizo parent)
			: base(parent)
		{
			ConstructorCounter++;
		}

		public static int ConstructorCounter;

		protected override void CheckZ0_Description()
		{
#if DEBUG
			if (Parent is DummyBusinessObject)
			{
				((DummyBusinessObject)Parent).Z0_DescriptionValidationCount++;
			}
#endif
			base.CheckZ0_Description();
			if (Parent.Z0_Description == "Bad")
			{
				Parent.Z0_DescriptionInfo.AddError("Bad!");
			}
			else if (Parent.Z0_Description == "MessageError")
			{
				Parent.Z0_DescriptionInfo.AddMessageError("MessageError!");
			}
		}

		protected override void CheckZ0_VarCharMax()
		{
			base.CheckZ0_VarCharMax();
			if (Parent.Z0_VarCharMax == "Error")
			{
				Parent.Z0_VarCharMaxInfo.AddError("Error!");
			}
		}
	}
}
