using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.PlugIn.Testing;

class EntryInstructionSupportingDocumentsFieldControlTest : TestCaseWithFactory
{
	public void TestSupportingDocumentsGroupBox_Caption()
	{
		using (var supportingDocumentsFieldControl = new EntryInstructionSupportingDocumentsFieldsControl(Factory.New<JobDeclaration>()))
		{
			var groupBox = supportingDocumentsFieldControl.FindSingleOrDefault<ZGroupBox>();
			CombineAssertions(() =>
			{
				AssertEquals("group box: DockStyle is fill", DockStyle.Fill, groupBox.Dock);
				AssertEquals("caption set by resource string", "Supporting Documents", groupBox.CaptionResourceString.Caption);
			});
		}
	}
}
