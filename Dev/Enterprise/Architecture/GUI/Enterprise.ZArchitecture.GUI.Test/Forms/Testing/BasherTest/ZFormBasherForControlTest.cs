using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(ZChildForm))]
	public abstract class ZFormBasherForControlTest : ZFormBasherTest
	{
		protected abstract IBusiness GetBusinessEntityForBinding();
		protected abstract Control GetControlForTest();

		protected override Form GetFormToBashCore()
		{
			var filter = GetBusinessEntityForBinding();
			var filterControl = GetControlForTest();

			var form = new ZChildForm(filter) { CaptionRenderingEnabled = true };
			form.Controls.Cast<Control>().ForEach(x => x.Dispose());
			form.Controls.Add(filterControl);
			form.ClientSize = filterControl.Size;

			return form;
		}
	}
}
