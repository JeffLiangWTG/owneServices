using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class OrganisationLVSConsolidationUserControl : ZUserControl
	{
		public OrganisationLVSConsolidationUserControl()
		{
			InitializeComponent();
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
	}
}
