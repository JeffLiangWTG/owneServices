using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.EMCS.GUI
{
	sealed class DeclarationOrganizationControlBag : ControlBag
	{
		public DeclarationOrganizationControlBag()
		{
			CertificateIdentifierGroupBox = RegisterControl(nameof(DeclarationOrganizationUserControl.CertificateIdentifierGroupBox));
		}

		public ControlReference CertificateIdentifierGroupBox { get; }

		public static DeclarationOrganizationControlBag Instance => instance ?? (instance = new DeclarationOrganizationControlBag());

		[ThreadStatic]
		static DeclarationOrganizationControlBag instance;

		protected override Control CreateTemplate() => new DeclarationOrganizationUserControl();
	}
}
