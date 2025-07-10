using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(RCREntryInstructionLayoutTemplate))]
	sealed class RCREntryInstructionLayoutTemplateTest : TestCase
	{
		public void TestControls()
		{
			using var control = new RCREntryInstructionLayoutTemplate();
			CombineAssertions("Visible", () =>
			{
				Assert("RCRActionDropEdit", control.FindSingle<ZDropEdit>("RCRActionDropEdit").Visible);
				Assert("PreviousBillNumberTextBox", control.FindSingle<ZTextBox>("PreviousBillNumberTextBox").Visible);
				Assert("ViaLocationCodeFindBox", control.FindSingle<ZCodeFindBox>("ViaLocationCodeFindBox").Visible);
			});
		}
	}
}
