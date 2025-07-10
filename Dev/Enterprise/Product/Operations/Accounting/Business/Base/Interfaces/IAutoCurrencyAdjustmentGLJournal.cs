namespace Enterprise.Accounting.Business.Base.Interfaces
{
	public interface IAutoCurrencyAdjustmentGLJournal : IGeneralLedger
	{
		bool IsAutoCurrencyAdjustmentGLJournal { get; }
	}
}

#region Test
#if DEBUG
namespace Enterprise.Accounting.Business.Base.Interfaces.Testing
{
	public class TestIAutoCurrencyAdjustmentGLJournal : TestIReverseTransaction, IAutoCurrencyAdjustmentGLJournal
	{
		public bool IsAutoCurrencyAdjustmentGLJournal => true;
	}
}
#endif
#endregion
