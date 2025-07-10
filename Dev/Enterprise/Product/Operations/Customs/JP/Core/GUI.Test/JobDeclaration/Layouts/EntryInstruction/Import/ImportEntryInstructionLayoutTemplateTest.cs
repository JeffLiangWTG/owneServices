using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ImportEntryInstructionLayoutTemplate))]
	sealed class ImportEntryInstructionLayoutTemplateTest : TestCase
	{
		public void TestControls()
		{
			using (var control = new ImportEntryInstructionLayoutTemplate())
			{
				Assert("DutyDrawbackDropEdit", control.FindSingle<ZDropEdit>("DutyDrawbackDropEdit").Visible);
				Assert("ContentInspectionResultDropEdit", control.FindSingle<ZDropEdit>("ContentInspectionResultDropEdit").Visible);

				Assert("BP application reason for import", control.FindSingle<ZDropEdit>(ctrl => control.BindingSource.GetBindingMember(ctrl).Equals("CEI_BeforePermitApplicationReason")).Visible);

				Assert("BondedLocationCodeFindBox", control.FindSingle<ZCodeFindBox>("BondedLocationCodeFindBox").Visible);
				Assert("BondedLocationNameTextBox", control.FindSingle<ZTextBox>("BondedLocationNameTextBox").Visible);
				Assert("DeclarationCargoTypeDropEdit", control.FindSingle<ZDropEdit>("DeclarationCargoTypeDropEdit").Visible);
				Assert("SpecialDeclarationTypeDropEdit", control.FindSingle<ZDropEdit>("SpecialDeclarationTypeDropEdit").Visible);
			}
		}
	}
}
