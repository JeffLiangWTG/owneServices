using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaContainerLookups : ASYCUDA.Business.AsycudaContainerLookups
	{
		public AsycudaContainerLookups(ASYCUDA.Business.AsycudaContainer parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList EmptyFullList => Factory.GetCachedValue<ILEmptyFullIndicatorList>();

		public CodeDescriptionPairList UnloadedStates => Factory.GetCachedValue<ILUnloadedStates>();

		protected override CodeDescriptionPairList GetSealTypeListCore()
			=> Factory.GetSealTypeList();
	}
}
