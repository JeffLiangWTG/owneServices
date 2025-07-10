using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaGAddInfoCusEntryInstructionValidation : AddInfoCusEntryInstructionValidation
	{
		public DeltaGAddInfoCusEntryInstructionValidation(AddInfoCusEntryInstruction parent) : base(parent)
		{
		}

		protected override void CheckZG_TransNature()
		{
			base.CheckZG_TransNature();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_TransNatureInfo);
		}
	}
}
