using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	public abstract class BillLayoutBuilderAbstractTest<TBuilder, T> : ColumnLayoutBuilderAbstractTest<TBuilder, T, CommonBillControlBag>
			where TBuilder : BillLayoutBuilder<T> where T : AsycudaBill
	{
	}
}
