namespace Enterprise.Registry.GUI
{
	public partial class ContactSalutationControl : RegistryZUserControl
	{
		public ContactSalutationControl()
		{
			InitializeComponent();
		}

		public object Data
		{
			get { return data; }
			set
			{
				data = value;
				SetDataBinding(data, null);
			}
		}
		object data;

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			contactSalutationsGrid.ReadOnly = readOnly;
		}
	}
}
