using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public partial class CusLineTariffDetailUserControl : ZUserControl
	{
		public CusLineTariffDetailUserControl()
		{
			InitializeComponent();
			CusLineTariffDetailGrid.ReOrderColumns(orderedColumns);
		}

		readonly string[] orderedColumns = {
			CusLineTariffDetail.Schema.BZ_Type,
			CusLineTariffDetail.Schema.TypeDescription,
			CusLineTariffDetail.Schema.BZ_Tariff,
			CusLineTariffDetail.Schema.ExciseReferenceNumberDescription,
			CusLineTariffDetail.Schema.RateFormula,
			CusLineTariffDetail.Schema.BZ_Qty1,
			CusLineTariffDetail.Schema.BZ_UQ1,
			CusLineTariffDetail.Schema.BZ_Qty2,
			CusLineTariffDetail.Schema.BZ_UQ2,
			CusLineTariffDetail.Schema.ZG_MethodOfPayment,
			CusLineTariffDetail.Schema.PaymentMethodDescription,
		};
	}
}
