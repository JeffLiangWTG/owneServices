using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class AddInfoCusEntryInstruction : EU.Business.Declaration.AddInfoCusEntryInstruction
	{
		public AddInfoCusEntryInstruction(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
		{
		}

		public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		protected override EUAddInfoLookups GetNewLookups() =>
			Parent is CusEntryInstruction instruction && instruction.IsImport ? new ImportAddInfoCusEntryInstructionLookups(this) : new AddInfoCusEntryInstructionLookups(this);

		public new AddInfoCusEntryInstructionLookups Lookups => (AddInfoCusEntryInstructionLookups)base.Lookups;

		protected override EUAddInfoValidation GetNewValidation()
		{
			if (Parent?.IsImport ?? false)
			{
				return new ImportAddInfoCusEntryInstructionValidation(this);
			}
			else
			{
				return base.GetNewValidation();
			}
		}
	}
}
