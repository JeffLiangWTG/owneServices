using System;
using System.Drawing;
using Enterprise.ZArchitecture;

namespace Enterprise.BufferManagement.GUI
{
	public class WorkflowDependencyStatusCalcEdit : ZCalcEdit
	{
		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null)
			{
				BindingManager.CurrentItemChanged += BindingManager_CurrentItemChanged;
				this.TextChanged += CalcEdit_TextChanged;
				UpdateBackgroundColour();
			}
		}

		void BindingManager_CurrentItemChanged(object sender, EventArgs e)
		{
			UpdateBackgroundColour();
		}

		void CalcEdit_TextChanged(object sender, EventArgs e)
		{
			UpdateBackgroundColour();
		}

		void UpdateBackgroundColour()
		{
			var value = (decimal)CalcValue;

			if (value > 0)
			{
				ColorChanger.ForceBackColor(Color.FromArgb(255, 202, 213));
			}
			else
			{
				ColorChanger.ResetForcedColor();
			}
		}
	}
}
