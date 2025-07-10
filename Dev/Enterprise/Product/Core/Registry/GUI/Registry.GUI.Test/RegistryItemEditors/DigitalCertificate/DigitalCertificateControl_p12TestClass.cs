using System.Collections.Generic;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class DigitalCertificateControl_p12TestClass : DigitalCertificateControl_p12
	{
		public new IEnumerable<string> SupportingFileExtensionList => base.SupportingFileExtensionList;
	}
}
