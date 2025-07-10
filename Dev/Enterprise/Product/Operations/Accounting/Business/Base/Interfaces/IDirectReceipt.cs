
namespace Enterprise.Accounting.Business.Base.Interfaces
{
	interface IDirectReceipt : ICashBook
	{
	}
}

#region Test
#if DEBUG
namespace Enterprise.Accounting.Business.Base.Interfaces.Testing
{
	public class TestIDirectReceiptTransaction : TestICashBookTransaction, IDirectReceipt
	{
	}
}
#endif
#endregion