using System.Collections.Generic;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class ECCCWildlifeUserControl : ZUserControl
	{
		public ECCCWildlifeUserControl(bool isOnInvoiceLine)
		{
			InitializeComponent();
			InitializeLazyCreate(isOnInvoiceLine);
		}

		protected void InitializeLazyCreate(bool isOnInvoiceLine)
		{
			var list = new List<string>(ECCCPGAHeader.AvailableLPCOFields);
			list.Remove(CusCALPCO.Schema.CLP_AlternativeQuotaQuantity);
			list.Remove(CusCALPCO.Schema.CLP_AlternativeQuotaUQ);
			LPCOGridUserControl.RemoveExceptAvailableColumns(list);

			IDUserControl.SetColumnCaption(AutoComponent.Schema.CA_Type, Res.GetString("a9683115-c41d-4fd5-b446-428b8b65f7cb", "Identity Code"));
			IDUserControl.SetColumnCaption(AutoComponent.Schema.CA_Name, Res.GetString("a42d8254-3046-4ee5-ac85-72b3ef59e945", "ID Number"));
			IDUserControl.ReOrderColumns(nameof(Component.CA_Type), nameof(Component.TypeDescription), nameof(Component.CA_Name));

			IDUserControl.RemoveFromAvailableColumns(
				AutoComponent.Schema.CA_Concentration,
				AutoComponent.Schema.CA_Origin,
				AutoComponent.Schema.CA_Qty,
				AutoComponent.Schema.CA_UQ,
				AutoComponent.Schema.CA_QualityOrYield);

			if (!isOnInvoiceLine)
			{
				DetailsGroupBox.Controls.Remove(CountryCodeFindBox);
				DetailsGroupBox.Controls.Remove(ModelTextBox);
			}
		}
	}
}
