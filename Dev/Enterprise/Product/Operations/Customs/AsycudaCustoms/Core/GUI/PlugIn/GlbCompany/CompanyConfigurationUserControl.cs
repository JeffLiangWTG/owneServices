using CargoWise.BrandManager;
using CargoWise.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public partial class CompanyConfigurationUserControl : ZUserControl
	{
		public CompanyConfigurationUserControl()
		{
			InitializeComponent();
			if (!this.IsDesignMode())
			{
				NoteLabel.Text = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetString("C22970CC-8543-415B-BB25-445034FA0790", "NOTE: In the case that exchange rate data is published by {0}, changing this setting normally will have no effect.", BrandingFactory.Instance.ProductName);
			}
		}
	}
}
