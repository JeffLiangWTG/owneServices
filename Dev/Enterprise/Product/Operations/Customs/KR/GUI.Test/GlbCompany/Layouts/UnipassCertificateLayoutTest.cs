using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(UnipassCertificateLayout))]
	sealed class UnipassCertificateLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (UnipassCertificateControlBag.Instance.UserIDTextBox, ControlWidthClass.Long);
				yield return (UnipassCertificateControlBag.Instance.MailBoxTextBox, ControlWidthClass.Long);
				yield return (UnipassCertificateControlBag.Instance.SenderIDTextBox, ControlWidthClass.Long);
				yield return (UnipassCertificateControlBag.Instance.CertificatePasswordTextBox, ControlWidthClass.Long);
				yield return (UnipassCertificateControlBag.Instance.StatusTextBox, ControlWidthClass.Long);
				yield return (UnipassCertificateControlBag.Instance.StatusReasonTextBox, ControlWidthClass.Long);
				yield return (UnipassCertificateControlBag.Instance.CertificateFileTextBox, ControlWidthClass.Long);
				yield return (UnipassCertificateControlBag.Instance.CertificateLoaderUserControl, ControlWidthClass.Auto);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new UnipassCertificateLayoutBuilder();
	}
}
