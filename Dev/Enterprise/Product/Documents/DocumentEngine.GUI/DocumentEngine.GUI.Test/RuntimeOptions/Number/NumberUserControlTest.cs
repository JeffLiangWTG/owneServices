using System.Windows.Forms;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(NumberUserControl))]
	sealed class NumberUserControlTest : RuntimeOptionUserControlBaseTest<NumberUserControl>
	{
		[RequiresSTA]
		public void TestSetFilter()
		{
			NumberField field = new NumberField(Factory);
			using (ZForm form = new ZForm())
			using (NumberUserControl numberControl = new NumberUserControl())
			{
				form.Controls.Add(numberControl);
				form.Show();
				Application.DoEvents();

				numberControl.SetFilter(field);
				AssertEquals("CalcEdit should be bound", true, numberControl.CalcEdit.DataBindings.Count > 0);
			}
		}
	}
}
