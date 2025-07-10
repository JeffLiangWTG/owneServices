using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.OperationalActions.Ccsuk.Testing
{
	public class RenominateUserControlTest : TestCase
	{
		public void TestControls()
		{
			using (var control = new RenominateUserControl())
			{
				var newAgentDropEdit = control.Controls.Find("NewAgentCodeFind", true)[0];
				AssertType<ZDropEdit>(newAgentDropEdit);
				Assert(newAgentDropEdit.Visible);
			}
		}
	}
}
