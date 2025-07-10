using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class SEDDetailsGroupBoxControlBag
		: ControlBag
	{
		protected override Control CreateTemplate() => new SEDDetailsGroupBoxUserControl();

		[ThreadStatic]
		static SEDDetailsGroupBoxControlBag instance;

		public static SEDDetailsGroupBoxControlBag Instance => instance ?? (instance = new SEDDetailsGroupBoxControlBag());

		SEDDetailsGroupBoxControlBag()
		{
			CertificateOfOriginGroupBox = RegisterControl(nameof(SEDDetailsGroupBoxUserControl.CertificateOfOriginGroupBox));
			ManufacturerGroupBox = RegisterControl(nameof(SEDDetailsGroupBoxUserControl.ManufacturerGroupBox));
			ImporterGroupBox = RegisterControl(nameof(SEDDetailsGroupBoxUserControl.ImporterGroupBox));
			SupplierGroupBox = RegisterControl(nameof(SEDDetailsGroupBoxUserControl.SupplierGroupBox));
		}

		public ControlReference CertificateOfOriginGroupBox { get; }
		public ControlReference ManufacturerGroupBox { get; }
		public ControlReference ImporterGroupBox { get; }
		public ControlReference SupplierGroupBox { get; }
	}
}
