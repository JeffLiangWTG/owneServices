using System.Collections;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business.Declaration;

public class FiscalReferenceLookups : CusSupportingInfoLookups
{
	public FiscalReferenceLookups(FiscalReference parent)
		: base(parent)
	{
	}
	protected new FiscalReference Parent => (FiscalReference)base.Parent;

	public override ICollection CodeList => Factory.GetCachedValue<FiscalReferenceCodeList>();

	public CodeDescriptionPairList HolderIdentificationList => CommonLookups.EORILookup(Parent?.Declaration, new string[] { CommonLookups.Declarant, CommonLookups.Importer }, null);

	public ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> EntryInstructions => Parent?.Declaration?.CustomsEntryInstructionProvider?.CustomsEntryInstructions;
}
