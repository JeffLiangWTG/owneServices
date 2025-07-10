#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class ChargeWithCost
	{
		public bool JR_AC_ReadOnly_ForTestOnly => JR_AC_ReadOnly;

		public void UpdateCostFieldsReadOnly_ForTestOnly()
		{
			UpdateCostFieldsReadOnly();
		}

		public void UpdateCoreFieldsReadOnly_ForTestOnly()
		{
			UpdateCoreFieldsReadOnly();
		}

		public bool JR_Desc_ReadOnly_ForTestOnly => JR_Desc_ReadOnly;

		public ZDecimal SellInvoiceRateWithoutCFX_ForTestOnly
		{
			get { return SellInvoiceRateWithoutCFX; }
			set { SellInvoiceRateWithoutCFX = value; }
		}

		public void EmptyRevenueCalculationDescription_ForTestOnly()
		{
			EmptyRevenueCalculationDescription();
		}

		public void EmptyCostCalculationDescription_ForTestOnly()
		{
			EmptyCostCalculationDescription();
		}

		public void CreateAccrualAndWIP_ForTestOnly()
		{
			CreateAccrualAndWIP();
		}
	}
}

#endif
