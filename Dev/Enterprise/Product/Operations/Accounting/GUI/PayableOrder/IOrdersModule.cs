using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PayableOrder
{
	public interface IOrdersModule
	{
		IZForm ShowFormForSplit(AccPayableOrderHeader orderToSplit, AccPayableOrderHeader.CreateOrderType splitType);
	}
}
