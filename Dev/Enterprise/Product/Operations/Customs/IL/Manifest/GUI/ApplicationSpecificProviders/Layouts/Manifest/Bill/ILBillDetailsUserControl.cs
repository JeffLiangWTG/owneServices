using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class ILBillDetailsUserControl : ZUserControl
	{
		public ILBillDetailsUserControl()
		{
			InitializeComponent();
		}
	}
}
