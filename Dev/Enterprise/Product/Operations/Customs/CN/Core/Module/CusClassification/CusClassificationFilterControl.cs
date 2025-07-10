using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Module
{
	public partial class CusClassificationFilterControl : Customs.Module.CusClassificationFilterControl
	{
		public CusClassificationFilterControl()
		{
			InitializeComponent();
		}

		public CusClassificationFilterControl(IBusinessObjectCollection gridCollection, CusClassificationFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
