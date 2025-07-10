using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ECREntryInstructionLayoutTemplate))]
	sealed class ECREntryInstructionLayoutTemplateTest : TestCase
	{
		public void TestControls()
		{
			using var control = new ECREntryInstructionLayoutTemplate();
			CombineAssertions("Visible", () =>
			{
				Assert("CusEntryInstructionNSITextBox", control.FindSingle<ZTextBox>("CusEntryInstructionNSITextBox").Visible);
				Assert("ECRNotesTextBox", control.FindSingle<ZTextBox>("ECRNotesTextBox").Visible);
				Assert("ECRCargoTypeDropEdit", control.FindSingle<ZDropEdit>("ECRCargoTypeDropEdit").Visible);
				Assert("SpecialCargoCodeFindBox", control.FindSingle<ZCodeFindBox>("SpecialCargoCodeFindBox").Visible);
				Assert("VolumeCalcDropEdit", control.FindSingle<ZCalcDropEdit>("VolumeCalcDropEdit").Visible);
				Assert("CustomsVolumeCalcDropEdit", control.FindSingle<ZCalcDropEdit>("CustomsVolumeCalcDropEdit").Visible);
			});
		}
	}
}
