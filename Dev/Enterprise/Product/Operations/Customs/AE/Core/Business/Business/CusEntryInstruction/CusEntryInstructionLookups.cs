using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Universal.AsycudaUniversalReference;

namespace Enterprise.Customs.AE.Business;

public class CusEntryInstructionLookups : Customs.Business.CusEntryInstructionLookups
{
	public CusEntryInstructionLookups(CusEntryInstruction parent)
		: base(parent)
	{
	}

	public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

	public CodeDescriptionPairList DeclarationPurposeList => GetDeclarationPurposeList();

	CodeDescriptionPairList GetDeclarationPurposeList()
	{
		return Parent.JobDeclaration == null ? new CodeDescriptionPairList() : RefCusCodeListTypes.GetCachedList(Factory, Parent.JobDeclaration.GetDefaultDataGroupingCode(), AEConstants.RefCusCodeList.CodeTypes.DelarationPurpose);
	}

	public CodeDescriptionPairList TradeTypeList => Factory.GetCachedValue<TradeTypeList>();
}
