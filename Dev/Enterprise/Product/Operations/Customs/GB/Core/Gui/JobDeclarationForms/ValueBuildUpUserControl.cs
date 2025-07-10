using System;

using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public partial class ValueBuildUpUserControl : ZUserControl
	{
		public ValueBuildUpUserControl()
		{
			InitializeComponent();
			FARPCodeFindBox.AllowOverlap(ApportionByWeightCheckBox);
			FARPCodeFindBox.AllowOverlap(ManualValueBuildupCheckBox);
		}

		void Box68Calculator_Click(object sender, EventArgs e)
		{
			try
			{
				var declaration = BindingSource.DataSource as JobDeclaration;
				if (declaration != null)
				{
					declaration.CalculateVATAdjustmentForBox68();
				}
			}
			catch (InvalidOperationException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}
	}
}
