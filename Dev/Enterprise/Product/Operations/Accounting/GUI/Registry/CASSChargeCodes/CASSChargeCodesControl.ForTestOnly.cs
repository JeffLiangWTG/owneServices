#if DEBUG

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class CASSChargeCodesControl
	{
		public ZArchitecture.ZGrid CASSComponentsGrid_ForTestOnly
		{
			get { return CASSComponentsGrid; }
			set { CASSComponentsGrid = value; }
		}
	}
}

#endif
