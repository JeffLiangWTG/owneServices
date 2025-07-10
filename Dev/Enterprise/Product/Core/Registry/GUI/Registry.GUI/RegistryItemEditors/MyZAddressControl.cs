using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	sealed class MyZAddressControl : ZAddressControl
	{
		public new ZAddressBusinessObject DataSource;

		public MyZAddressControl(ZAddressBusinessObject dataSource)
		{
			this.DataSource = dataSource;
		}
	}
}
