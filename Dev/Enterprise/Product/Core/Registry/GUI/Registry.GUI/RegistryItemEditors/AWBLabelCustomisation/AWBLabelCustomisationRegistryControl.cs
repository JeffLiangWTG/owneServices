using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class AWBLabelCustomisationRegistryControl : RegistryZUserControl
	{
		public AWBLabelCustomisationRegistryControl()
		{
			InitializeComponent();
			CustomDesignDropEdit.SelectedIndexChanged += CustomDesignDropEdit_SelectedIndexChanged;
#if DEBUG
			TypeDescriptor.AddAttributes(DesignTemplateLabel, new SuppressControlRequiresTextBasherAttribute());
#endif
		}

		void CustomDesignDropEdit_SelectedIndexChanged(object sender, EventArgs e)
		{
			RefreshCustomDesignPanel();
		}

		void RefreshCustomDesignPanel()
		{
			switch (CustomDesignDropEdit.Text)
			{
				case AWBLabelCustomDesignList.Codes.Default:
					DesignTemplateLabel.Text = defaultDesign;
					break;

				case AWBLabelCustomDesignList.Codes.Design1:
					DesignTemplateLabel.Text = customDesign1;
					break;

				case AWBLabelCustomDesignList.Codes.Design2:
					DesignTemplateLabel.Text = customDesign2;
					break;

				case AWBLabelCustomDesignList.Codes.Design3:
					DesignTemplateLabel.Text = customDesign3;
					break;

				default:
					DesignTemplateLabel.Text = string.Empty;
					break;
			}
		}

		#region Designs label text

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Space-sensitive text")]
		const string defaultDesign =
@"                  BARCODE
-----------------------------------------------
Optional1
Optional2
Optional3 (6'' only)
";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Space-sensitive text")]
		const string customDesign1 =
@"Optional1
-----------------------------------------------
Optional2	            Optional3   Optional4
-----------------------------------------------
                  BARCODE
";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Space-sensitive text")]
		const string customDesign2 =
@"Optional1
-----------------------------------------------
Optional2             Optional3 
-----------------------------------------------
                  BARCODE
";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Space-sensitive text")]
		const string customDesign3 =
@"-----------------------------------------------
Optional1             Optional2
-----------------------------------------------
                  BARCODE
-----------------------------------------------
";

		#endregion

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			foreach (Control control in this.Controls)
			{
				control.Enabled = !readOnly;
			}
		}
	}
}
