using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class UnipassCertificateControlBag : ControlBag
	{
		UnipassCertificateControlBag()
		{
			MailBoxTextBox = RegisterControl(nameof(UnipassCertificateUserControl.MailBoxTextBox));
			SenderIDTextBox = RegisterControl(nameof(UnipassCertificateUserControl.SenderIDTextBox));
			CertificatePasswordTextBox = RegisterControl(nameof(UnipassCertificateUserControl.CertificatePasswordTextBox));
			StatusTextBox = RegisterControl(nameof(UnipassCertificateUserControl.StatusTextBox));
			StatusReasonTextBox = RegisterControl(nameof(UnipassCertificateUserControl.StatusReasonTextBox));
			CertificateFileTextBox = RegisterControl(nameof(UnipassCertificateUserControl.CertificateFileTextBox));
			CertificateLoaderUserControl = RegisterControl(nameof(UnipassCertificateUserControl.CertificateLoaderUserControl));
			UserIDTextBox = RegisterControl(nameof(UnipassCertificateUserControl.UserIDTextBox));
		}

		public static UnipassCertificateControlBag Instance => instance ?? (instance = new UnipassCertificateControlBag());

		[ThreadStatic]
		static UnipassCertificateControlBag instance;

		protected override Control CreateTemplate() => new UnipassCertificateUserControl();

		public ControlReference MailBoxTextBox { get; }
		public ControlReference SenderIDTextBox { get; }
		public ControlReference CertificatePasswordTextBox { get; }
		public ControlReference StatusTextBox { get; }
		public ControlReference StatusReasonTextBox { get; }
		public ControlReference CertificateFileTextBox { get; }
		public ControlReference CertificateLoaderUserControl { get; }
		public ControlReference UserIDTextBox { get; }
	}
}
