using System.Collections.Generic;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class NRCanEXPUserControl : ZUserControl
	{
		public NRCanEXPUserControl(bool isOnInvoiceLine = false)
		{
			InitializeComponent();
			InitializeLazyCreate(isOnInvoiceLine);
		}
		protected void InitializeLazyCreate(bool isOnInvoiceLine)
		{
			var list = new List<string>(NRCanPGAHeader.AvailableLPCOFields);
			list.Remove(CusCALPCO.Schema.CLP_RN_NKIssuanceCountryCode);
			list.Remove(CusCALPCO.Schema.CLP_RN_NKOriginCountryCode);
			list.Remove(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation);
			list.Remove(CusCALPCO.Schema.CLP_IsMixedCountryOfOrigin);
			list.Remove(CusCALPCO.Schema.CLP_EndDate);
			list.Remove(CusCALPCO.Schema.CLP_StartDate);
			LPCOGridUserControl.RemoveExceptAvailableColumns(list);

			if (!isOnInvoiceLine)
			{
				netWeightCalcDropEdit.Dispose();
				grossWeightCalcDropEdit.Dispose();
				quantityCalcDropEdit.Dispose();
				UNDGGuidFindBox.Dispose();
				tradeNameTextBox.Dispose();
			}
		}
	}
}
