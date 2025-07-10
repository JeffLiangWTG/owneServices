using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportAddInfoCusEntryInstructionLookups : AddInfoCusEntryInstructionLookups
	{
		public ImportAddInfoCusEntryInstructionLookups(AddInfoCusEntryInstruction parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList ProcessingProcedureCode => Factory.GetCachedValue<ProcessingProcedureCodeList>();
	}
}
