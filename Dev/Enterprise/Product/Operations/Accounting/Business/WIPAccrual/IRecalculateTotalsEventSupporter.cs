using System;
namespace Enterprise.Accounting.Business.WIPAccrual
{
	interface IRecalculateAmountsEventSupporter
	{
		void RaiseRecalculateAmounts();
		event EventHandler RecalculateAmounts;
	}
}
