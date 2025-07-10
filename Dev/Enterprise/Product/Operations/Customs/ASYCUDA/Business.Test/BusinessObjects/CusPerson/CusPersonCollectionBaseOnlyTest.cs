using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(CusPersonCollection))]
	sealed class CusPersonCollectionBaseOnlyTest : CusPersonCollectionAbstractTest<CusPersonCollection>
	{
		protected override CusPersonCollection GetNewCollection(AsycudaManifestHeader header) => new CusPersonCollection(header);
	}
}
