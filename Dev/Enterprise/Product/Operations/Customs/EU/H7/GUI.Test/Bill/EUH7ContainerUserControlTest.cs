using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7ContainerUserControl))]
	sealed class EUH7ContainerUserControlTest : TestCaseWithFactory
	{
		public void TestContainerNumberTextBox()
		{
			using (var control = new EUH7ContainerUserControl())
			{
				control.Show();

				var containerNumberTextBox = control.FindSingle<ZTextBox>("ContainerNumber");

				AssertEquals("BindingMember", "ContainerNumber", containerNumberTextBox.GetBindingMember());
				AssertEquals("Caption", "Container", containerNumberTextBox.CaptionResourceString.Caption);
			}
		}

		public void TestContainerModeDropEdit()
		{
			using (var control = new EUH7ContainerUserControl())
			{
				control.Show();

				var containerModeDropEdit = control.FindSingle<ZDropEdit>("ContainerMode");

				AssertEquals("BindingMember", "ABL_ContainerMode", containerModeDropEdit.GetBindingMember());
			}
		}
	}
}
