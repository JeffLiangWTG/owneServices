using System;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.CA.GUI
{
	public partial class OrganisationCSAUserControl : ZUserControl
	{
		public OrganisationCSAUserControl()
		{
			InitializeComponent();
			MissingResourceStringChecker.ExcludeFromTest(DetailsGroupBox);
		}

		OrgImpAddInfo parent
		{
			get { return DataSource as OrgImpAddInfo; }
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			TradeChainPartnerGroupBox.Visible = parent.IsTCPMessageAvailable;
		}
	}
}
