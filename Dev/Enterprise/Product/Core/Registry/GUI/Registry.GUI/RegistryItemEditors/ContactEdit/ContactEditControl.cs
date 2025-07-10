using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class ContactEditControl : ZUserControl
	{
		public ZContactBusinessObject Contact;

		public ContactEditControl(ZContactBusinessObject contact)
		{
			InitializeComponent();
			this.Contact = contact;
			SetDataBinding(contact, "");
		}

		public bool ReadOnly
		{
			get { return fReadOnly; }
			set
			{
				fReadOnly = value;
				Contact.ReadOnly = value;
			}
		}
		bool fReadOnly;
	}
}
