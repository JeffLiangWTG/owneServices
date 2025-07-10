using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.Manifest.GUI
{
	public sealed class BRBillControlBag : ControlBag
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "There is no way to change static backing field of this property.")]
		public static BRBillControlBag Instance { get; } = new BRBillControlBag();

		public BRBillControlBag()
		{
			DocumentTypeDropEdit = RegisterControl(nameof(BRBillCountrySpecificUserControl.DocumentTypeDropEdit));
			ToOrderCheckBox = RegisterControl(nameof(BRBillCountrySpecificUserControl.ToOrderCheckBox));
			BLServiceCheckBox = RegisterControl(nameof(BRBillCountrySpecificUserControl.BLServiceCheckBox));
			FRTModeDropEdit = RegisterControl(nameof(BRBillCountrySpecificUserControl.FRTModeDropEdit));
			SellerCountryCodeFindBox = RegisterControl(nameof(BRBillCountrySpecificUserControl.SellerCountryCodeFindBox));
			CEMercanteTextBox = RegisterControl(nameof(BRBillCountrySpecificUserControl.CEMercanteTextBox));
		}

		protected override Control CreateTemplate() => new BRBillCountrySpecificUserControl();

		public ControlReference DocumentTypeDropEdit { get; }
		public ControlReference ToOrderCheckBox { get; }
		public ControlReference BLServiceCheckBox { get; }
		public ControlReference FRTModeDropEdit { get; }
		public ControlReference SellerCountryCodeFindBox { get; }
		public ControlReference CEMercanteTextBox { get; }
	}
}
