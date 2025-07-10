using System.Collections.Generic;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class ECCCWasteUserControl : ZUserControl
	{
		public ECCCWasteUserControl(bool isOnInvoiceLine)
		{
			InitializeComponent();
			InitializeLazyCreate(isOnInvoiceLine);
		}

		void InitializeLazyCreate(bool isOnInvoiceLine)
		{
			var list = new List<string>(ECCCPGAHeader.AvailableLPCOFields);
			list.Remove(CusCALPCO.Schema.CLP_EndDate);
			list.Remove(CusCALPCO.Schema.CLP_HolderType);
			list.Remove(CusCALPCO.Schema.LPCOHolderOrgPK);
			list.Remove(CusCALPCO.Schema.CLP_HolderName);
			list.Remove(CusCALPCO.Schema.CLP_OA_Holder);
			list.Remove(CusCALPCO.Schema.CLP_AlternativeQuotaQuantity);
			list.Remove(CusCALPCO.Schema.CLP_AlternativeQuotaUQ);
			list.Remove(CusCALPCO.Schema.CLP_IssueDate);
			list.Remove(CusCALPCO.Schema.CLP_HolderContactName);
			list.Remove(CusCALPCO.Schema.CLP_HolderContactPhone);
			list.Remove(CusCALPCO.Schema.CLP_HolderContactEmail);
			list.Remove(CusCALPCO.Schema.CLP_IsHolderOverridden);
			LPCOGridUserControl.RemoveExceptAvailableColumns(list);

			if (!isOnInvoiceLine)
			{
				DetailsGroupBox.Controls.Remove(this.ConsigneeAddressControl);
			}
		}
	}
}
