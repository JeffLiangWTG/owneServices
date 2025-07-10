using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class AddInfoCusEntryInstruction : EU.Business.Declaration.AddInfoCusEntryInstruction
	{
		public AddInfoCusEntryInstruction(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
		{
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryInstructionLookups.ValuationBypassCodeList))]
		public override ZString ZG_BypassCode
		{
			get => base.ZG_BypassCode; set => base.ZG_BypassCode = value;
		}

		[ResourceStringData("070F339A-8CD1-476A-B760-333B69308E9B", Caption = "[24] Tran. Nature", FullDescription = "The nature of the transaction.")]
		[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryInstructionLookups.TransNatureList))]
		public override ZString ZG_TransNature
		{
			get => base.ZG_TransNature;
			set => base.ZG_TransNature = value;
		}

		protected override EUAddInfoLookups GetNewLookups()
		{
			return ((JobDeclaration)Parent.JobDeclaration)?.ApplicationExtender.GetAddInfoCusEntryInstructionLookups(this) ?? new DeltaGAddInfoCusEntryInstructionLookups(this);
		}

		protected override EUAddInfoValidation GetNewValidation()
		{
			return ((JobDeclaration)Parent.JobDeclaration)?.ApplicationExtender.GetAddInfoCusEntryInstructionValidation(this) ?? new DeltaGAddInfoCusEntryInstructionValidation(this);
		}
	}
}
