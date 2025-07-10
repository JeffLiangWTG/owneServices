using System;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class LinkedDocumentUserControl : ZUserControl
	{
		public LinkedDocumentUserControl()
		{
			InitializeComponent();
		}

		protected JobDeclaration Declaration => (JobDeclaration)DataSource;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (Declaration != null)
			{
				using (LinkedDocumentGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					LinkedDocumentGrid.SetAllAvailability(false);
					LinkedDocumentGrid.SetAvailability(true, GetAvailableColumnsForLinkedDocument(Declaration));
				}
			}
		}

		string[] GetAvailableColumnsForLinkedDocument(JobDeclaration declaration) => declaration.IsImportSiscomex ? availableColumnsForImportSiscomex : availableColumnsForImport;

		readonly string[] availableColumnsForImport = new string[]
		{
			CusSupportingInfo.Schema.CSI_Code,
			CusSupportingInfo.Schema.CSI_ReferenceNumber,
			CusSupportingInfo.Schema.CSI_ItemNumber
		};

		readonly string[] availableColumnsForImportSiscomex = new string[]
		{
			CusSupportingInfo.Schema.CSI_Code,
			CusSupportingInfo.Schema.CSI_ReferenceNumber
		};
	}
}
