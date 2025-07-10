using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	sealed partial class SecurityAtDeparturePlaceOfLoadingUserControl : ZUserControl, IExtendedControl
	{
		public SecurityAtDeparturePlaceOfLoadingUserControl()
		{
			InitializeComponent();
			Extensions = new DefaultControlExtensionCollection(this);
			this.GetExtension<ILabelCaptionRenderer>().Caption = !DesignModeFinder.IsDesigning ? Res.GetString("6E01D265-4212-4202-AE96-5261FA8EE0C9", "Place of Loading") : (NoResString)ZString.Empty;
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
