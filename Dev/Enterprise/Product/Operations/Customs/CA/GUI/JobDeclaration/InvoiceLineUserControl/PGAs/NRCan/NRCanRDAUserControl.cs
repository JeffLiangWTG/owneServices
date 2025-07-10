using System.Collections.Generic;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class NRCanRDAUserControl : ZUserControl
	{
		public NRCanRDAUserControl(bool isOnInvoiceLine = false)
		{
			InitializeComponent();
			InitializeLazyCreate(isOnInvoiceLine);
		}
		protected void InitializeLazyCreate(bool isOnInvoiceLine)
		{
			var list = new List<string>(NRCanPGAHeader.AvailableLPCOFields);
			list.Remove(CusCALPCO.Schema.CLP_ApplicantType);
			list.Remove(CusCALPCO.Schema.LPCOApplicantOrgPK);
			list.Remove(CusCALPCO.Schema.CLP_ApplicantName);
			list.Remove(CusCALPCO.Schema.CLP_OA_Applicant);
			list.Remove(CusCALPCO.Schema.CLP_IsApplicantOverridden);
			LPCOGridUserControl.RemoveExceptAvailableColumns(list);

			if (!isOnInvoiceLine)
			{
				detailsGroupBox.Controls.Remove(CustomsValueInUsdEdit);
				caratWeightCalcEdit.Dispose();
			}
		}
	}
}
