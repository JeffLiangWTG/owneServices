using CargoWise.Types;

namespace Enterprise.Customs.CN.Business;

internal class EntryRequiresCIQStrategyPackageType : IEntryRequiresCIQStrategy
{
	internal static string PackageTypeRequireCIQMessage => Res.GetString("884DB141-7C1A-464F-8CBB-240AD2569D5D", "the type of package or other packages include wooden packaging");

	public ZString Validate(CusEntryInstruction instruction)
	{
		if (instruction.OtherPackages.ContainsCode(PackageType.Codes.WoodBox)
			|| instruction.OtherPackages.ContainsCode(PackageType.Codes.WoodBarrel)
			|| instruction.OtherPackages.ContainsCode(PackageType.Codes.NaturalWood)
			|| instruction.OtherPackages.ContainsCode(PackageType.Codes.PlantAuxiliaryPadMaterial)
			|| instruction.CEI_PackageUQ == PackageType.Codes.WoodBox
			|| instruction.CEI_PackageUQ == PackageType.Codes.WoodBarrel
			|| instruction.CEI_PackageUQ == PackageType.Codes.NaturalWood
			|| instruction.CEI_PackageUQ == PackageType.Codes.PlantAuxiliaryPadMaterial)
		{
			return PackageTypeRequireCIQMessage;
		}
		return ZString.Empty;
	}
}
