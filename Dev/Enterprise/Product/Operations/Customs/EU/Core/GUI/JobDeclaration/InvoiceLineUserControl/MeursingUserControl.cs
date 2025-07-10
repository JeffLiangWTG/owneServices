using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Meursing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class MeursingUserControl : ZUserControl
	{
		public MeursingUserControl()
		{
			InitializeComponent();
		}

		public MeursingUserControl(IMeursingTableManager manager, bool closeFormAfterCalculation = true)
		{
			this.manager = manager;
			this.closeFormAfterCalculation = closeFormAfterCalculation;
			if (!this.IsDesignMode() && manager != null)
			{
				SetDataBinding(manager.MeursingTable, "");
			}
			InitializeComponent();
		}

		readonly IMeursingTableManager manager;

		void CalculateButton_Click(object sender, EventArgs e)
		{
			manager.Execute();
			if (!closeFormAfterCalculation)
			{
				this.ParentForm.DialogResult = System.Windows.Forms.DialogResult.None;
			}
		}

		readonly bool closeFormAfterCalculation;
	}
}
