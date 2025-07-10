using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Declaration;

public class GuaranteeForEntryInstruction : EU.Business.Declaration.GuaranteeForEntryInstruction
{
	public GuaranteeForEntryInstruction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new GuaranteeForEntryInstructionLookups Lookups => (GuaranteeForEntryInstructionLookups)base.Lookups;

	protected override CusBondDetailLookups GetNewLookups() => new GuaranteeForEntryInstructionLookups(this);

	protected override CusBondDetailValidation GetNewValidation() => new GuaranteeForEntryInstructionValidation(this);

	[MaxLength(5)]
	[List(nameof(Lookups) + "." + nameof(GuaranteeForEntryInstructionLookups.BondTypeList))]
	public override ZString PW_BondType { get => base.PW_BondType; set => base.PW_BondType = value; }

	[MaxLength(35)]
	public override ZString PW_BondNumber { get => base.PW_BondNumber; set => base.PW_BondNumber = value; }

	[MaxLength(35)]
	[List(nameof(Lookups) + "." + nameof(GuaranteeForEntryInstructionLookups.FacilityCollection))]
	public override ZString PW_HolderIdentification { get => base.PW_HolderIdentification; set => base.PW_HolderIdentification = value; }
}
