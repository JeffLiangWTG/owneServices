using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class PackingPlugInUserControl : ZUserControl
	{
		public PackingPlugInUserControl()
		{
			InitializeComponent();
			new UNDGDataItemFormManager(PackingGrid).Initialize(DGLinkLabel, DangerousGoodGuidFindBox);
			overrideDefaultValuesSupporter = new ZGridOverrideDefaultValuesSupporter(PackingGrid);
		}

		readonly ZGridOverrideDefaultValuesSupporter overrideDefaultValuesSupporter;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				overrideDefaultValuesSupporter.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
