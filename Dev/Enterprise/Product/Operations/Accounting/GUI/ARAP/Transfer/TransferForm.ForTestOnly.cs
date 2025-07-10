#if DEBUG

using System;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class TransferForm
	{
		public void OnShown_ForTestOnly(EventArgs e)
		{
			OnShown(e);
		}

		public ZArchitecture.GUI.ZCalcFindBox AH_Calc_FromBeforeTransferBoundCurrencyControl_ForTestOnly
		{
			get { return AH_Calc_FromBeforeTransferBoundCurrencyControl; }
			set { AH_Calc_FromBeforeTransferBoundCurrencyControl = value; }
		}

		public ZArchitecture.GUI.ZCalcFindBox AH_Calc_FromAfterTransferBoundCurrencyControl_ForTestOnly
		{
			get { return AH_Calc_FromAfterTransferBoundCurrencyControl; }
			set { AH_Calc_FromAfterTransferBoundCurrencyControl = value; }
		}

		public ZArchitecture.GUI.ZCalcFindBox AH_Calc_ToBeforeTransferBoundCurrencyControl_ForTestOnly
		{
			get { return AH_Calc_ToBeforeTransferBoundCurrencyControl; }
			set { AH_Calc_ToBeforeTransferBoundCurrencyControl = value; }
		}

		public ZArchitecture.GUI.ZCalcFindBox AH_Calc_ToAfterTransferBoundCurrencyControl_ForTestOnly
		{
			get { return AH_Calc_ToAfterTransferBoundCurrencyControl; }
			set { AH_Calc_ToAfterTransferBoundCurrencyControl = value; }
		}
	}
}

#endif
