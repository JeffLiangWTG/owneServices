using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class AddInfoCusEntryHeaderLookups : EU.Business.Declaration.AddInfoCusEntryHeaderLookups
	{
		public AddInfoCusEntryHeaderLookups(EU.Business.Declaration.AddInfoCusEntryHeader parent) : base(parent)
		{
		}

		public CodeDescriptionPairList CircuitCodeList => Factory.GetCachedValue<CircuitCodeList>();

		public CodeDescriptionPairList ClearanceResultCodeList => Factory.GetCachedValue<ClearanceResultCodeList>();
	}
}
