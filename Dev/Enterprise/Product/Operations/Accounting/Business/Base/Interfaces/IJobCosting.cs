
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.Base.Interfaces
{
	public interface IJobCosting : IReversing
	{
	}
}

#if DEBUG
namespace Enterprise.Accounting.Business.Base.Interfaces.Testing
{
	public class TestIJobCostingTransaction : TestIReverseTransaction, IJobCosting
	{
	}
}
#endif
