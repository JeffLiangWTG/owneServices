using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class AddInfoCusEntryInstruction : EU.Business.Declaration.AddInfoCusEntryInstruction
	{
		public AddInfoCusEntryInstruction(CusEntryInstruction cusEntryInstruction) : base(cusEntryInstruction.CEI_AddInfoInfo)
		{
		}

		public new AddInfoCusEntryInstructionValidation Validation => (AddInfoCusEntryInstructionValidation)base.Validation;

		protected override EUAddInfoValidation GetNewValidation() => new AddInfoCusEntryInstructionValidation(this);

		public new AddInfoCusEntryInstructionLookups Lookups => (AddInfoCusEntryInstructionLookups)base.Lookups;

		protected override EUAddInfoLookups GetNewLookups() => new AddInfoCusEntryInstructionLookups(this);

		public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryInstructionLookups.PartyConstellationCodeList))]
		public override ZString ZG_PartyConstellation
		{
			get => base.ZG_PartyConstellation;
			set => base.ZG_PartyConstellation = value;
		}
	}
}
