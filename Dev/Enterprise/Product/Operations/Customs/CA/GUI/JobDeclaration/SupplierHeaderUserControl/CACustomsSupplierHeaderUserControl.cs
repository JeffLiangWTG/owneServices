
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class CACustomsSupplierHeaderUserControl : Customs.GUI.NonLayoutCustomsSupplierHeaderUserControl
	{
		public static string IsCIFComponent
		{
			get { return Res.GetString("2d9c198e-c8dd-4e9b-9010-0b160f3dd61d", "CIF component"); }
		}

		public CACustomsSupplierHeaderUserControl()
		{
			InitializeComponent();
		}

		protected override string ColumnTitleForGSTApplies
		{
			get { return IsCIFComponent; }
		}
	}
}
