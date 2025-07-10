using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using ResString = CargoWise.Main.ResString;

namespace Enterprise.Startup
{
	public partial class ExitConfirmationControl : ZUserControl, IDialogDefaultControlMembers
	{
		public ExitConfirmationControl()
		{
			InitializeComponent();

			TopLabel.Font = new System.Drawing.Font("Segoe UI", 10f);

			HomeButton.Image = Icons.GetImage(IconTypes.HomeLarge);
			SetupColors(HomeButton);

			ExitButton.Image = Icons.GetImage(IconTypes.ExitLarge);
			SetupColors(ExitButton);
		}

		static void SetupColors(Control control)
		{
			control.BackColor = SystemDataRegistry.Instance.ColorTheme.NavBarButtonColor1;
			control.MouseEnter += (sender, args) => ((Control)sender).BackColor = SystemDataRegistry.Instance.ColorTheme.NavBarGroupSelected1;
			control.MouseLeave += (sender, args) => ((Control)sender).BackColor = SystemDataRegistry.Instance.ColorTheme.NavBarButtonColor1;
		}

		new KForm Parent { get { return (KForm)base.Parent; } }

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				Parent.DialogResult = DialogResult.Cancel;
				return true;
			}
			else if (keyData == Keys.Enter)
			{
				Parent.DialogResult = DialogResult.Yes;
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		public static DialogDefaultContext DialogDefaultContext
		{
			get
			{
				return new DialogDefaultContext(new ZGuid("407D8E17-6A62-4729-9B23-DC0D578BD5C8"), ResString.GetMultilingualString("E172967A-8B3F-473E-9510-2DE4CC31A107", "Exit or Home"), null, ZMessageBoxIcon.None, showCheckboxOnly: true);
			}
		}

		public void SetReadOnly(bool readOnly, ZDialogResult allowedResult)
		{
			var isHomeButton = allowedResult == (ZDialogResult)HomeButton.DialogResult;
			HomeButton.Enabled = !readOnly || isHomeButton;
			ExitButton.Enabled = !readOnly || !isHomeButton;
		}
	}
}
