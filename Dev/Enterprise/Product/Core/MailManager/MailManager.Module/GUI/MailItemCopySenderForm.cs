using Enterprise.MailManager.Business;
using Res = MailManager.Module.Res;

namespace Enterprise.MailManager.GUI
{
	public partial class MailItemCopySenderForm : ZArchitecture.GUI.ZChildForm
	{
		public MailItemCopySenderForm(MailItemCopySender businessEntity) : base(businessEntity)
		{
			InitializeComponent();
			Text = Res.GetString("ced1b4a7-5630-4de0-b4b2-f94e0380d831", "Copy of Selected Emails");
		}

		public override string FormVerb
		{
			get
			{ return Res.GetString("2894cd8b-b0d2-49d4-9ee0-58492ee00164", "Send"); }
		}

		protected MailItemCopySender CopySender
		{
			get
			{
				return (MailItemCopySender)BusinessEntity;
			}
		}
	}
}
