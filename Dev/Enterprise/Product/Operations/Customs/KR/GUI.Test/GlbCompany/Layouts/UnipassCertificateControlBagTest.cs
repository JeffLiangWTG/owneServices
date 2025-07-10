using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(UnipassCertificateControlBag))]
	sealed class UnipassCertificateControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(UnipassCertificateControlBag.UserIDTextBox);
				yield return nameof(UnipassCertificateControlBag.MailBoxTextBox);
				yield return nameof(UnipassCertificateControlBag.SenderIDTextBox);
				yield return nameof(UnipassCertificateControlBag.CertificatePasswordTextBox);
				yield return nameof(UnipassCertificateControlBag.StatusTextBox);
				yield return nameof(UnipassCertificateControlBag.StatusReasonTextBox);
				yield return nameof(UnipassCertificateControlBag.CertificateFileTextBox);
				yield return nameof(UnipassCertificateControlBag.CertificateLoaderUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => UnipassCertificateControlBag.Instance;
	}
}
