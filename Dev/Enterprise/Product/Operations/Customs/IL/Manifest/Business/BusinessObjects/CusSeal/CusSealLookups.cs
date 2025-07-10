using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class CusSealLookups : Customs.Business.CusSealLookups
	{
		public CusSealLookups(CusSeal parent) : base(parent)
		{
		}

		public CodeDescriptionPairList UnloadedStates => Factory.GetCachedValue<ILUnloadedStates>();

		public CodeDescriptionPairList SealTypeList
			=> Factory.GetSealTypeList();

		public CodeDescriptionPairList SealingPartyList
			=> Parent.Parent is AsycudaContainer asycudaContainer
				? asycudaContainer.Lookups.SealingPartyList
				: new CodeDescriptionPairList();

		protected new CusSeal Parent => (CusSeal)base.Parent;
	}
}
