using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class GuaranteeAccessCodesSendingForm : MessageSendingObjectForm
	{
		public GuaranteeAccessCodesSendingForm()
		{
			InitialiseForm();
		}

		public GuaranteeAccessCodesSendingForm(GuaranteeAccessCodesSendingObjectParent parent) : base(parent)
		{
		}

		public new GuaranteeAccessCodesSendingObjectParent BusinessEntity => (GuaranteeAccessCodesSendingObjectParent)base.BusinessEntity;

		public override string FormHeading => Res.GetString("F5C4AF66-A095-4BC3-B850-BA3ED5B51914", "Guarantee Access Codes");

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
