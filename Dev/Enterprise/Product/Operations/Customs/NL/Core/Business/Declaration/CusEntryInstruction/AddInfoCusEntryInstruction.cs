using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business.Declaration;

public class AddInfoCusEntryInstruction : EU.Business.Declaration.AddInfoCusEntryInstruction
{
	public AddInfoCusEntryInstruction(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
	{
	}

	public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

	[ResourceStringData("AB6D9834-C968-4084-B290-E67044FF9E91", Caption = "Tran. Nature", FullDescription = "[UCC 8/5] Transaction Nature")]
	[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryInstructionLookups.TransNatureList))]
	public override ZString ZG_TransNature
	{
		get => base.ZG_TransNature;
		set => base.ZG_TransNature = value;
	}

	protected override EUAddInfoLookups GetNewLookups() => new AddInfoCusEntryInstructionLookups(this);

	protected override EUAddInfoValidation GetNewValidation() => new AddInfoCusEntryInstructionValidation(this);
}
