using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Module
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
	}
}
