using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Grid.Testing
{
	[TestedType(typeof(GridRowFinderForm))]
	sealed class MinimumSizeGridRowFinderFormTest : GridRowFinderFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var form = base.GetFormToBashCore();
			form.Size = form.MinimumSize;

			return form;
		}
	}
}
