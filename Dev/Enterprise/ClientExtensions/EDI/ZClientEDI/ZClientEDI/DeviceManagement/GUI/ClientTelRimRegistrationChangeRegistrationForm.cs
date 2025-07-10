using System;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.Telematics.Tca;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.DeviceManagement.GUI
{
	abstract partial class ClientTelRimRegistrationChangeRegistrationForm : ZChildForm
	{
		public ClientTelRimRegistrationChangeRegistrationForm(ClientDeviceHeader clientDeviceHeader)
			: this(new RimEnrolmentRequestProcessorFactory().GetProcessor(TimeSpan.FromMilliseconds(30000)), clientDeviceHeader)
		{
		}

		internal ClientTelRimRegistrationChangeRegistrationForm(IRimEnrolmentRequestProcessor rimEnrollmentRequestProcessor, ClientDeviceHeader clientDeviceHeader)
		{
			this.clientDeviceHeader = clientDeviceHeader ?? throw new ArgumentNullException(nameof(clientDeviceHeader));
			this.rimEnrolmentRequestProcessor = rimEnrollmentRequestProcessor ?? throw new ArgumentNullException(nameof(rimEnrollmentRequestProcessor));
			InitializeComponent();
		}
	}
}
