using Enterprise.PAVE.MENT.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.PAVE.MENT.GUI
{
	public partial class MENTCollectionControl : ZUserControl
	{
		public MENTCollectionControl()
		{
			InitializeComponent();
		}

		public MENTAcceptabilityBandViewModel ViewModel
		{
			get { return (MENTAcceptabilityBandViewModel)DataSource; }
		}
	}
}
