using System.Collections.Generic;
using System.Linq;
using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.EndpointManagement.GUI
{
	public class EDIDigitalCertificateControl : DigitalCertificateControl_p12
	{
		public EDIDigitalCertificateControl() : base()
		{
		}

		protected override IEnumerable<string> SupportingFileExtensionList => base.SupportingFileExtensionList.Append(".cer");

		public void SetViewOnly()
		{
			LoadButton.Enabled = ClearButton.Enabled = false;
		}
	}
}
