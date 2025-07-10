using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.CommercialInvoice
{
	public sealed class EUInvoiceHeaderDetailsControlBag : ControlBag
	{
		public static EUInvoiceHeaderDetailsControlBag Instance => instance ?? (instance = new EUInvoiceHeaderDetailsControlBag());

		[ThreadStatic]
		static EUInvoiceHeaderDetailsControlBag instance;

		public EUInvoiceHeaderDetailsControlBag() : base()
		{
			AgreedPlaceCodeFindBox = RegisterControl(nameof(InvoiceHeaderDetailsLayouUserControl.AgreedPlaceCodeFindBox));
		}

		public ControlReference AgreedPlaceCodeFindBox { get; }
		protected override Control CreateTemplate() => new InvoiceHeaderDetailsLayouUserControl();
	}
}
