using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.Declaration;

public class AddInfoCusEntryInstruction : EU.Business.Declaration.AddInfoCusEntryInstruction
{
	public AddInfoCusEntryInstruction(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
	{
	}

	[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryInstructionLookups.RequestTypeList))]
	public override ZString ZG_RequestType { get => base.ZG_RequestType; set => base.ZG_RequestType = value; }

	[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryInstructionLookups.IndirectTypeList))]
	public override ZString ZG_IndirectType { get => base.ZG_IndirectType; set => base.ZG_IndirectType = value; }

	public new AddInfoCusEntryInstructionLookups Lookups => (AddInfoCusEntryInstructionLookups)base.Lookups;

	protected override EUAddInfoLookups GetNewLookups() => new AddInfoCusEntryInstructionLookups(this);

	public new AddInfoCusEntryInstructionValidation Validation => (AddInfoCusEntryInstructionValidation)base.Validation;

	protected override EUAddInfoValidation GetNewValidation() => new AddInfoCusEntryInstructionValidation(this);
}
