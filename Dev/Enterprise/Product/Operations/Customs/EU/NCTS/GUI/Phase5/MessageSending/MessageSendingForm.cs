using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class MessageSendingForm : MessageSendingFormWithValidationDetails
	{
		[Obsolete("Do not call. Only for designer use.")]
		public MessageSendingForm()
		{
		}

		public MessageSendingForm(NctsHeaderMessageSendingObjectParent sendingObjectParent)
			: base(sendingObjectParent)
		{
			InitializeComponent();
		}

		protected override bool SendWithAdditionalWarningCheckBoxVisible =>
			NctsHeader is NctsHeader header
			? !header.Configuration.MessageSendingConfiguration.ShouldHideSendWithAdditionalWarningCheckBox && base.SendWithAdditionalWarningCheckBoxVisible
			: base.SendWithAdditionalWarningCheckBoxVisible;

		protected override bool PreviewMessageCheckboxVisible => true;

		protected override IGridColumnLayoutProvider MessageSendingGridColumnLayoutProvider => GetMessageSendingGridColumnLayoutProvider();

		IGridColumnLayoutProvider GetMessageSendingGridColumnLayoutProvider()
		{
			var nctsHeader = NctsHeader;
			return nctsHeader == null || !nctsHeader.IsPhase5
				? null
				: NctsPhase5LayoutProvider.GetLayoutProvider(nctsHeader.DefaultDataGroupingCode)?.GetMessageSendingGridColumnLayout(BusinessEntity);
		}

		protected NctsHeader NctsHeader => (NctsHeader)BusinessEntity?.TopLevelBusinessObject;
	}
}
