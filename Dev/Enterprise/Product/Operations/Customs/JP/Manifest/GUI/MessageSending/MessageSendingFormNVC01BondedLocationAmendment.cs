using Enterprise.Customs.JP.Manifest.Business;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	public partial class MessageSendingFormNVC01BondedLocationAmendment : MessageSendingForm
	{
		public MessageSendingFormNVC01BondedLocationAmendment()
		{
		}

		public MessageSendingFormNVC01BondedLocationAmendment(ManifestMessageSendingObjectParent parent)
			: base(parent) { }

		public new ManifestMessageSendingObjectParent BusinessEntity => base.BusinessEntity;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
