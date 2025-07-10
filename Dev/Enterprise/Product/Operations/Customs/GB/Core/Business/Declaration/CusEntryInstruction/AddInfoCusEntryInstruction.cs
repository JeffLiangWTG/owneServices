using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class AddInfoCusEntryInstruction : EU.Business.Declaration.AddInfoCusEntryInstruction
	{
		public AddInfoCusEntryInstruction(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
		{
		}

		protected override EU.Business.EUAddInfoValidation GetNewValidation()
		{
			return new AddInfoCusEntryInstructionValidation(this);
		}
	}
}
