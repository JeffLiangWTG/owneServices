using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinzorFramework;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public partial class ZFilterStripDropCodeBox
	{
		protected override bool AutoComplete => false;

		protected override void OnInputChanged(string value)
		{
			ParentDropEdit.ProposedText = value;

			if (!ParentDropEdit.DropButton.IsDroppedDown)
			{
				ParentDropEdit.DropButton.ShowDropDown(false);
			}

			base.OnInputChanged(value);
		}
	}
}
