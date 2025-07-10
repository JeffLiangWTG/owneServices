
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.Base.Interfaces
{
	public interface IGeneralLedger : IReversing
	{
	}
}

#region Test
#if DEBUG
namespace Enterprise.Accounting.Business.Base.Interfaces.Testing
{
	public class TestIGeneralLedgerTransaction : TestIReverseTransaction, IGeneralLedger
	{
	}
}
#endif
#endregion