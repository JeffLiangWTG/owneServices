using System;
using Enterprise.Customs.EU.GUI.PlugIn;

namespace Enterprise.Customs.IE.GUI
{
	public partial class OrgSupplierPartFormCustomsControl : EU.GUI.EUOrgSupplierPartFormCustomsControl
	{
		public OrgSupplierPartFormCustomsControl()
		{
			InitializeComponent();
		}

		protected override Type GetPreviousDocumentsUserControlType() => typeof(PreviousDocumentsUserControl);

		protected override Type GetAdditionalInfosUserControlType() => typeof(AdditionalInfosUserControlWithGrid);

		protected override Type GetSupportingDocumentsUserControlType() => typeof(PartPivotLayoutSupportingDocumentsUserControl);
	}
}
