using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.GUI
{
	public partial class UCC6TemporaryStorageMessageSendingForm : MessageSendingFormWithValidationDetails
	{
		public UCC6TemporaryStorageMessageSendingForm(BaseMessageSendingObjectParent messageSendingObjectParent)
			: base(messageSendingObjectParent)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			SetColumns();
		}

		protected override bool PreviewMessageCheckboxVisible => true;

		void SetColumns()
		{
			MessageSendingObjectsGrid.SetColumnGroupName(nameof(TemporaryStorageMessageSendingObject.AlternativeDateOfAcceptance), Res.GetData("60CA0BC6-CB40-4E81-9D63-A1FB5A0931F4", "Fallback Procedure"));
			MessageSendingObjectsGrid.SetColumnGroupName(nameof(TemporaryStorageMessageSendingObject.CustomsReference), Res.GetData("60CA0BC6-CB40-4E81-9D63-A1FB5A0931F4", "Fallback Procedure"));
			MessageSendingObjectsGrid.SetColumnGroupName(nameof(TemporaryStorageMessageSendingObject.CustomsJustification), Res.GetData("60CA0BC6-CB40-4E81-9D63-A1FB5A0931F4", "Fallback Procedure"));
		}
	}
}
