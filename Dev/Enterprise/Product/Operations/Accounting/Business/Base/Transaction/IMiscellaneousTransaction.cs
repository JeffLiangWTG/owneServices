using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public interface IMiscellaneousTransaction : IBusiness
	{
		MatchingBase MatchingBizO { get; set; }

		/// <summary>
		/// Optional checkpoint required to unmatch the transaction.
		/// </summary>
		SecurityCheckpoint CheckpointForUnmatch { get; }
	}
}

#region Test
#if DEBUG
namespace Enterprise.Accounting.Business.Base.Interfaces.Testing
{
	using Enterprise.Accounting.Business.Base.Transaction;

	public class TestIMiscellaneousTransaction : TestIReverseTransaction, IMiscellaneousTransaction
	{
		#region IMiscellaneousTransaction Members

		public MatchingBase MatchingBizO { get; set; }

		public SecurityCheckpoint CheckpointForUnmatch { get; set; }

		#endregion
	}
}
#endif
#endregion