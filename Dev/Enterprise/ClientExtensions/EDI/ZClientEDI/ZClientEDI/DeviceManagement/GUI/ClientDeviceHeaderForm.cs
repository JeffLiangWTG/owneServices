using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.DeviceManagement.GUI
{
	public partial class ClientDeviceHeaderForm : ZTemplateForm
	{
		public ClientDeviceHeaderForm(ClientDeviceHeader device)
			: base(device)
		{
			InitializeComponent();
			Load += SetTcaRegistrationVisibility;
		}

		protected override bool SupportsEDocs => false;

		public override string FormCaption => "Telematics Device - " + ((ClientDeviceHeader)BusinessEntity).Code;

		protected override bool AllowNew
		{
			get
			{
				var device = (ClientDeviceHeader)BusinessEntity;
				return device.CDH_IsTemplate;
			}
		}
	}
}
