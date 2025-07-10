using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.EU.H7.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class EUH7BillPartiesUserControl : ZUserControl
	{
		public EUH7BillPartiesUserControl()
		{
			InitializeComponent();
		}
	}
}

