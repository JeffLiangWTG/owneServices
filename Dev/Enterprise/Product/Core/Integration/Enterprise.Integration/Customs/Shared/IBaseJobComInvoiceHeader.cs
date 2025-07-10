using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	static partial class Customs
	{
		static partial class Shared
		{
			public interface IBaseJobComInvoiceHeader : ICommonJobComInvoiceHeader
			{
				#region JZ_Calc_Balance

				ZDecimal JZ_Calc_Balance { get; }
				ZPropertyInfo JZ_Calc_BalanceInfo { get; }

				#endregion

				#region JZ_Calc_BalanceString

				ZString JZ_Calc_BalanceString { get; }
				ZPropertyInfo JZ_Calc_BalanceStringInfo { get; }

				#endregion

				#region JZ_Calc_ChargesExcludedFromITOT

				ZDecimal JZ_Calc_ChargesExcludedFromITOT { get; }
				ZPropertyInfo JZ_Calc_ChargesExcludedFromITOTInfo { get; }

				#endregion

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

				#region JZ_Calc_GroupInvoice

				ZString JZ_Calc_GroupInvoice { get; set; }
				ZPropertyInfo JZ_Calc_GroupInvoiceInfo { get; }

				#endregion

				#region JZ_Calc_LinesEntered

				ZDecimal JZ_Calc_LinesEntered { get; }
				ZPropertyInfo JZ_Calc_LinesEnteredInfo { get; }

				#endregion

				#region JZ_Calc_TNI

				ZDecimal JZ_Calc_TNI { get; }
				ZPropertyInfo JZ_Calc_TNIInfo { get; }

				#endregion

				#region JZ_ITOTIncoTerm

				ZString JZ_ITOTIncoTerm { get; }
				ZPropertyInfo JZ_ITOTIncoTermInfo { get; }

				#endregion

				#region JZ_MessageType

				ZString JZ_MessageType { get; set; }
				ZPropertyInfo JZ_MessageTypeInfo { get; }

				#endregion

				#region JZ_RX_Calc_TNICurrency

				ZGuid JZ_RX_Calc_TNICurrency { get; }
				ZPropertyInfo JZ_RX_Calc_TNICurrencyInfo { get; }

				#endregion

				#region JZ_NetWeight

				ZDecimal JZ_NetWeight { get; set; }

				#endregion

				#region JZ_NetWeightUQ

				ZString JZ_NetWeightUQ { get; set; }

				#endregion

				IBaseJobComInvoiceLine AddNewInvoiceLine();
			}
		}
	}
}
