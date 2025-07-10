using System;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class ExportOutOfEnclosureUserControl : ZUserControl
	{
		public ExportOutOfEnclosureUserControl()
		{
			InitializeComponent();
		}
		JobDeclaration Declaration => DataSource as JobDeclaration;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var declaration = Declaration;
			if (declaration != null)
			{
				declaration.ClearanceOfficeIsCustomsEnclosureInfo.ValueChanged += ClearanceOfficeIsCustomsEnclosureInfo_ValueChanged;
				declaration.BoardingOfficeIsCustomsEnclosureInfo.ValueChanged += BoardingOfficeIsCustomsEnclosureInfo_ValueChanged;

				ClearanceOfficeIsCustomsEnclosureInfo_ValueChanged(this, e);
				BoardingOfficeIsCustomsEnclosureInfo_ValueChanged(this, e);
			}
		}

		void ClearanceOfficeIsCustomsEnclosureInfo_ValueChanged(object sender, EventArgs e)
		{
			ClearanceLocalGroupBox.Enabled = !Declaration.ClearanceOfficeIsCustomsEnclosure;
		}

		void BoardingOfficeIsCustomsEnclosureInfo_ValueChanged(object sender, EventArgs e)
		{
			BoardingLocalGroupBox.Enabled = !Declaration.BoardingOfficeIsCustomsEnclosure;
		}
	}
}
