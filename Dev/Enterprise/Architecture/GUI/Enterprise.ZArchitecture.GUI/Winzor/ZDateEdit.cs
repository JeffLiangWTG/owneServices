using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZDateEdit : ZUserControl
	{
		public override bool CaptureElementReference => true;

		public override ElementReference? GetFocusElement() => DateTextBox.GetFocusElement();
	}
}
