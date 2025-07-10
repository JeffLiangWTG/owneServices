using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.AspNetCore.Components;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZFilterStrip
	{
		public override bool UseParentDivForLayout => false;

		public void SetFocusAfterRenderControl()
		{
			var control = GetNextSelectableControl(FilterDescriptionDropEdit, true, true, true, false);
			if (control != null)
			{
				RegisterAfterRenderAction(async () =>
				{
					var element = control.GetFocusElement();
					if (element.HasValue)
					{
						await element.Value.FocusAsync();
					}
				});
				NotifyRenderRequired();
			}
		}
	}
}
