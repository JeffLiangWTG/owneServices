using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IN.Business;

public class CusEntryInstructionLookups : Customs.Business.CusEntryInstructionLookups
{
	public CusEntryInstructionLookups(CusEntryInstruction parent)
		: base(parent)
	{
	}

	public new CusEntryInstruction Parent => base.Parent as CusEntryInstruction;

	ZDateTime DateOfValuation => JobDeclaration?.DateOfValuation ?? ZDateTime.Today;

	public CodeDescriptionPairList WeightUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

	public CodeDescriptionPairList PackageUQList => Factory.GetCachedValue("INPackageUQList", delegate
	{
		var typeList = new CodeDescriptionPairList();
		typeList.AddPair(Core.Constants.PkgUnit.Package, nameof(Core.Constants.PkgUnit.Package));
		return typeList;
	});

	public override CodeDescriptionPairList StyleList => Factory.GetCachedValue<DeclarationTypeList>();

	public override CodeDescriptionPairList EntrySubStyleList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.India, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NFEICategoryCode, DateOfValuation);

	public CodeDescriptionPairList CustomsStatusList => Parent.EntryHeader?.Lookups.CH_EntryStatusList ?? new CodeDescriptionPairList();

	public CodeDescriptionPairList MessageStatusList => Parent.EntryHeader?.Lookups.MessageStatusList ?? new CodeDescriptionPairList();
}
