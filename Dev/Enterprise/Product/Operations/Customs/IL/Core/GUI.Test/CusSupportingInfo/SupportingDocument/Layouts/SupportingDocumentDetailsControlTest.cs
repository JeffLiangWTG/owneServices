using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI.Testing
{
	sealed class SupportingDocumentDetailsControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new SupportingDocumentDetailsControl())
			{
				AssertNotNull(control.FindSingle<ZDropEdit>("typeDropEdit"));
				AssertNotNull(control.FindSingle<ZTextBox>("referenceNumberTextBox"));
				AssertNotNull(control.FindSingle<ZTextBox>("statusTextBox"));
				AssertNotNull(control.FindSingle<ZTextBox>("additionalDescriptionTextBox"));
				AssertNotNull(control.FindSingle<ZGuidDropEdit>("eDocGuidDropEditGuidDropEdit"));
				AssertNotNull(control.FindSingle<ZTextBox>("customsDocIDTextBox"));
			}
		}
	}
}
