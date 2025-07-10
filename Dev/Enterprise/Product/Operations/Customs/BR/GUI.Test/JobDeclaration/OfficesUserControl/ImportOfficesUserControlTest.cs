using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(ImportOfficesUserControl))]
	class ImportOfficesUserControlTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			using (var control = new ImportOfficesUserControl())
			{
				AssertType<ZGroupBox>("ClearanceGroupBox type should be", control.ClearanceGroupBox);
				AssertType<ZDropEdit>("ClearanceOfficeDropEdit type should be", control.ClearanceOfficeDropEdit);
				AssertType<ZDropEdit>("ClearanceEnclosureDropEdit type should be", control.ClearanceEnclosureDropEdit);
				AssertType<ZGroupBox>("EntranceLocationGroupBox type should be", control.EntranceLocationGroupBox);
				AssertType<ZDropEdit>("EntranceOfficeDropEdit type should be", control.EntranceOfficeDropEdit);
			}
		}

		public void TestCaptions()
		{
			using (var control = new ImportOfficesUserControl())
			{
				AssertEquals("ClearanceGroupBox caption should be", "Clearance Local", control.ClearanceGroupBox.CaptionResourceString.Caption);
				AssertEquals("EntranceLocationGroupBox caption should be", "Entrance Location", control.EntranceLocationGroupBox.CaptionResourceString.Caption);
			}
		}
	}
}
