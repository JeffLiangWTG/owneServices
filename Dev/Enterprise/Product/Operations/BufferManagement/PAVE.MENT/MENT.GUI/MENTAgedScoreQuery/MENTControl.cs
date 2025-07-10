using Enterprise.BufferManagement.Business;
using Enterprise.PAVE.MENT.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.PAVE.MENT.GUI
{
	public partial class MENTControl : ZUserControl
	{
		public MENTControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var band = dataSource as BMComponentAcceptabilityBand;
			if (band != null)
			{
				dataSource = new MENTAcceptabilityBandViewModel(band);
			}

			base.SetDataBinding(dataSource, dataMember);
		}
	}
}
