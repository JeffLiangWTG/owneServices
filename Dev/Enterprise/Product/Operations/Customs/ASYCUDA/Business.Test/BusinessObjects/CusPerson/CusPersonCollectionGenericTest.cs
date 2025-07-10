using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(CusPersonCollection<CusPerson, AsycudaManifestHeader>))]
	sealed class CusPersonCollectionGenericTest : CusPersonCollectionAbstractTest<CusPersonCollection<CusPerson, AsycudaManifestHeader>>
	{
		protected override CusPersonCollection<CusPerson, AsycudaManifestHeader> GetNewCollection(AsycudaManifestHeader header) => new CusPersonCollection<CusPerson, AsycudaManifestHeader>(header);
	}
}
