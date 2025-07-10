
namespace Enterprise.Accounting.Business.Base.Interfaces
{
	#if DEBUG
	internal
	#endif
	interface IDirectPayment : ICashBook
	{
	}
}

#region Test
#if DEBUG
namespace Enterprise.Accounting.Business.Base.Interfaces.Testing
{
	public class TestIDirectPaymentTransaction : TestICashBookTransaction, IDirectPayment
	{
	}
}
#endif
#endregion
