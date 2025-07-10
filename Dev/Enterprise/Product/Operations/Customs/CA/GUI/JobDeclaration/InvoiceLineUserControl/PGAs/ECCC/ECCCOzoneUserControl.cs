using System.Collections.Generic;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class ECCCOzoneUserControl : ZUserControl
	{
		public ECCCOzoneUserControl(bool isOnInvoiceLine)
		{
			InitializeComponent();
			InitStyle();
			InitializeLazyCreate(isOnInvoiceLine);
		}

		void InitStyle()
		{
			var list = new List<string>(ECCCPGAHeader.AvailableLPCOFields);
			list.Remove(CusCALPCO.Schema.CLP_IsHolderOverridden);
			list.Remove(CusCALPCO.Schema.CLP_HolderContactEmail);
			list.Remove(CusCALPCO.Schema.CLP_HolderContactName);
			list.Remove(CusCALPCO.Schema.CLP_HolderContactPhone);
			list.Remove(CusCALPCO.Schema.CLP_EndDate);
			list.Remove(CusCALPCO.Schema.CLP_HolderType);
			list.Remove(CusCALPCO.Schema.LPCOHolderOrgPK);
			list.Remove(CusCALPCO.Schema.CLP_HolderName);
			list.Remove(CusCALPCO.Schema.CLP_OA_Holder);
			list.Remove(CusCALPCO.Schema.CLP_IssueDate);
			LPCOGridUserControl.RemoveExceptAvailableColumns(list);

			ComponentUserControl.RemoveFromAvailableColumns(
				nameof(Component.CA_Concentration),
				nameof(Component.CA_QualityOrYield),
				nameof(Component.CA_Type),
				nameof(Component.TypeDescription)
			);
		}

		void InitializeLazyCreate(bool isOnInvoiceLine)
		{
			if (!isOnInvoiceLine)
			{
				SplitContainerHorizontal.Panel2.Controls.Remove(LPCOGroupBox);
				SplitContainer.Panel2.Controls.Add(LPCOGroupBox);

				SplitContainerHorizontal.Dispose();
			}
		}
	}
}
