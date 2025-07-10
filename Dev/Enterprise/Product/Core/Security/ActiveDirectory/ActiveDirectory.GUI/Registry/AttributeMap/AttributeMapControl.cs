using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Security.ActiveDirectory.GUI.Registry
{
	public partial class AttributeMapControl : ZUserControl
	{
		public AttributeMapControl()
		{
			InitializeComponent();
		}

		public AttributeMap Value
		{
			get { return BindingSource.DataSource as AttributeMap; }
			set { BindingSource.DataSource = value; }
		}

		public bool IsReadOnly
		{
			get { return attributeMapGrid.ReadOnly; }
			set { attributeMapGrid.ReadOnly = value; }
		}
	}
}