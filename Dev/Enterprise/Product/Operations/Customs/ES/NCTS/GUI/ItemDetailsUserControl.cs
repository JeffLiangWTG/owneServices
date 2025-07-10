using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class ItemDetailsUserControl : EU.NCTS.GUI.ItemDetailsUserControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public ItemDetailsUserControl()
		{
			InitializeComponent();

			ItemDetailsGroupBox.AllowOutsideOfParent();
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return (control.Name == "CustomsThirdQtyDropEdit" && previousControl.Name == "FiscalUnitsDropEdit")
				|| (control.Name == "FiscalUnitsDropEdit" && previousControl.Name == "CustomsThirdQtyDropEdit");
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			CustomsValueDropEdit.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("47AD5DBC-CBC8-498F-9D74-1B18F23809B4", "Stat. Value");
		}
	}
}
