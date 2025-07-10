using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IBaseJobComInvoiceGroupHeader : ICommonJobComInvoiceHeader
			{
				#region JZ_Calc_CIFAmount

				ZDecimal JZ_Calc_CIFAmount { get; }
				ZPropertyInfo JZ_Calc_CIFAmountInfo { get; }

				#endregion

				#region JZ_Calc_CIFCurrency

				ZGuid JZ_Calc_CIFCurrency { get; }
				ZPropertyInfo JZ_Calc_CIFCurrencyInfo { get; }

				#endregion

				#region JZ_Calc_FOBAmount

				ZDecimal JZ_Calc_FOBAmount { get; }
				ZPropertyInfo JZ_Calc_FOBAmountInfo { get; }

				#endregion

				#region JZ_Calc_FOBCurrency

				ZGuid JZ_Calc_FOBCurrency { get; }
				ZPropertyInfo JZ_Calc_FOBCurrencyInfo { get; }

				#endregion

				#region JZ_Calc_TNI

				ZDecimal JZ_Calc_TNI { get; }
				ZPropertyInfo JZ_Calc_TNIInfo { get; }

				#endregion

				#region JZ_RX_LocalCurrency

				ZGuid JZ_RX_LocalCurrency { get; }
				ZPropertyInfo JZ_RX_LocalCurrencyInfo { get; }

				#endregion
			}
		}
	}
}