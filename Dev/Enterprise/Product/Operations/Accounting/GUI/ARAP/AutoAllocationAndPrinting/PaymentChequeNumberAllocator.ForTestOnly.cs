#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting
{
	public partial class PaymentChequeNumberAllocator
	{
		public ZBool CanContinueWithPrinting_ForTestOnly => CanContinueWithPrinting;
	}
}

#endif
