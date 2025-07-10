using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class AddInfoCusEntryInstruction : AddInfo
	{
		public AddInfoCusEntryInstruction(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
		{
		}

		public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		public new AddInfoCusEntryInstructionLookups Lookups => (AddInfoCusEntryInstructionLookups)base.Lookups;

		public new AddInfoCusEntryInstructionValidation Validation => (AddInfoCusEntryInstructionValidation)base.Validation;

		protected override EUAddInfoLookups GetNewLookups() => new AddInfoCusEntryInstructionLookups(this);

		protected override EUAddInfoValidation GetNewValidation() => new AddInfoCusEntryInstructionValidation(this);

		[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryInstructionLookups.IdentificationOfGoodsCodeList))]
		public override ZString ZG_IdOfGoodCode { get => base.ZG_IdOfGoodCode; set => base.ZG_IdOfGoodCode = value; }

		[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryInstructionLookups.ProcessingProcedureCode))]
		public override ZString ZG_ProcessingProcedureCode { get => base.ZG_ProcessingProcedureCode; set => base.ZG_ProcessingProcedureCode = value; }

		[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryInstructionLookups.YesNoList))]
		public override ZString ZG_Article86_3_UCC { get => base.ZG_Article86_3_UCC; set => base.ZG_Article86_3_UCC = value; }
	}
}
