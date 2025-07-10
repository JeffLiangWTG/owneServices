using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	sealed partial class SecurityAtDeparturePlaceOfUnloadingUserControl : ZUserControl, IExtendedControl
	{
		public SecurityAtDeparturePlaceOfUnloadingUserControl()
		{
			InitializeComponent();

			Extensions = new DefaultControlExtensionCollection(this);
			this.GetExtension<ILabelCaptionRenderer>().Caption = !DesignModeFinder.IsDesigning
				? Res.GetString("9A31AF7E-C7E8-444B-9A3C-E6DD4417BA9B", "Place of Unloading")
				: (NoResString)ZString.Empty;
		}

		public Control Host => this;
		public IControlExtensionCollection Extensions { get; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
