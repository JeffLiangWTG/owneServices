using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.ExportManifest.GUI
{
	/// <summary>
	/// Summary description for ExportManifestDeclarationUserControl.
	/// </summary>
	public partial class ExportManifestDeclarationUserControl : ZUserControl
	{
		public ExportManifestDeclarationUserControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			// TODO: Add any initialization after the InitializeComponent call
		}

		#region Header

		public ExportCustomsManifestHeader Header
		{
			get
			{
				return header;
			}
			set
			{
				header = value;
				ManifestUserControl.Header = value;
			}
		}
		ExportCustomsManifestHeader header;

		#endregion

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
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
