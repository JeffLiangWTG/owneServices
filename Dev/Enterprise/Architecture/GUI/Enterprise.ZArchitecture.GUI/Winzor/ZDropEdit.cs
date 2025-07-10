using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;
using Microsoft.AspNetCore.Components;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZDropEdit : ZListUserControl
	{
		public override bool CaptureElementReference => true;

		public override ElementReference? GetFocusElement() => CodeBox.GetFocusElement();

		protected override string ClassName => (NoResString)"zdropedit";

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			if (IsDroppedDown)
			{
				DropButton.HideDropDown();
			}
		}

		protected override void InvalidateListCore()
		{
			CodeBox.ResetSuggestion();
			NotifyRenderRequired();
		}
	}
}
