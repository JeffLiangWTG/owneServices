using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	public abstract class ManifestLayoutBuilderAbstractTest<TBuilder, T> : ColumnLayoutBuilderAbstractTest<TBuilder, T, CommonManifestControlBag>
			where TBuilder : ManifestLayoutBuilder<T> where T : AsycudaManifestHeader
	{
	}
}
