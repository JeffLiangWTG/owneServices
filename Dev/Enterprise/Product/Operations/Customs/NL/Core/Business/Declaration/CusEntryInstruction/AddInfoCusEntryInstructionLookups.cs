using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business.Declaration;

public class AddInfoCusEntryInstructionLookups : EU.Business.Declaration.AddInfoCusEntryInstructionLookups
{
	public AddInfoCusEntryInstructionLookups(AddInfoCusEntryInstruction parent) : base(parent)
	{
	}

	public CodeDescriptionPairList TransNatureList => EUUniversalLookupsHelper.GetTranNatureList(Factory, Parent.Parent?.JobDeclaration?.GetDefaultDataGroupingCode() ?? Core.Constants.CountryCodes.Netherlands);
}
