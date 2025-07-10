using Enterprise.Registry.Business.Internal;

namespace Enterprise.Registry.GUI
{
	public partial class DecimalArrayControl : RegistryZUserControl
	{
		public DecimalArrayControl()
		{
			InitializeComponent();
			DataSourceType = typeof(DecimalLineCollection);
		}

		public DecimalLineCollection BusinessEntity
		{
			get { return (DecimalLineCollection)BindingSource.Current; }
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			DecimalGrid.ReadOnly = readOnly;
		}
	}
}
