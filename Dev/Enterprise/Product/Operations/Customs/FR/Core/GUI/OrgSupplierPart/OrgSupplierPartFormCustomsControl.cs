using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.GUI.PlugIn;

namespace Enterprise.Customs.FR.GUI
{
	public partial class OrgSupplierPartFormCustomsControl : EU.GUI.EUOrgSupplierPartFormCustomsControl
	{
		public OrgSupplierPartFormCustomsControl()
		{
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			additionalInfosTabPage.TabVisible = false;
		}

		protected override Type GetSupportingDocumentsUserControlType()
		{
			return typeof(SupportingDocumentsUserControl);
		}

		protected override ZString GetTariffTypeCore()
		{
			return TariffFormatter.GetTariffType(false);
		}
	}
}
