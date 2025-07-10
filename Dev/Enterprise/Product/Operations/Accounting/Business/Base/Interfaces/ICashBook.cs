
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.Base.Interfaces
{
	public interface ICashBook : IReversing
	{
	}
}

#region Test
#if DEBUG
namespace Enterprise.Accounting.Business.Base.Interfaces.Testing
{
	public class TestICashBookTransaction : TestIReverseTransaction, ICashBook
	{
	}
}
#endif
#endregion