using System;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class MercosulForeignDeclarationUserControl : ZUserControl
	{
		public MercosulForeignDeclarationUserControl()
		{
			InitializeComponent();
		}

		protected JobDeclaration Declaration => ((JobDeclaration)DataSource);

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (Declaration != null)
			{
				using (MercosulForeignDeclarationGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					MercosulForeignDeclarationGrid.SetAllAvailability(false);
					MercosulForeignDeclarationGrid.SetAvailability(true, GetAvailableColumnsForMercosulForeignDeclaration(Declaration));
				}
			}
		}

		string[] GetAvailableColumnsForMercosulForeignDeclaration(JobDeclaration declaration)
		{
			if (declaration.IsImportSiscomex)
			{
				return availableColumnsForImportSiscomex;
			}
			return availableColumnsForImport;
		}

		readonly string[] availableColumnsForImport = new string[]
		{
			CusSupportingInfo.Schema.CSI_Code,
			CusSupportingInfo.Schema.CSI_Quantity3
		};

		readonly string[] availableColumnsForImportSiscomex = new string[]
		{
			CusSupportingInfo.Schema.CSI_Description,
			CusSupportingInfo.Schema.CSI_ReferenceNumber,
			CusSupportingInfo.Schema.CSI_ReferenceNumber2,
			CusSupportingInfo.Schema.CSI_RN_NKCountryCode,
			CusSupportingInfo.Schema.CSI_Code,
			CusSupportingInfo.Schema.CSI_ItemNumber,
			CusSupportingInfo.Schema.CSI_Quantity3
		};
	}
}
