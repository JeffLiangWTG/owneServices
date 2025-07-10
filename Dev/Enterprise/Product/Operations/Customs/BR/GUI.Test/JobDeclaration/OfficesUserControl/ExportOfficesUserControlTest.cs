using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(ExportOfficesUserControl))]
	class ExportOfficesUserControlTest : TestCase
	{
		public void TestProperties()
		{
			using (var control = new ExportOfficesUserControl())
			{
				AssertType<ZGroupBox>("ClearanceGroupBox type should be", control.ClearanceGroupBox);
				AssertType<ZDropEdit>("ClearanceOfficeDropEdit type should be", control.ClearanceOfficeDropEdit);
				AssertType<ZDropEdit>("ClearanceEnclosureDropEdit type should be", control.ClearanceEnclosureDropEdit);
				AssertType<ZCheckBox>("ClearanceIsCustomsEnclosureCheckBox type should be", control.ClearanceIsCustomsEnclosureCheckBox);
				AssertType<ZGroupBox>("BoardingGroupBox type should be", control.BoardingGroupBox);
				AssertType<ZDropEdit>("BoardingOfficeDropEdit type should be", control.BoardingOfficeDropEdit);
				AssertType<ZDropEdit>("BoardingEnclosureDropEdit type should be", control.BoardingEnclosureDropEdit);
				AssertType<ZCheckBox>("BoardingOfficeIsCustomsEnclosureCheckBox type should be", control.BoardingOfficeIsCustomsEnclosureCheckBox);
			}
		}

		public void TestCaptions()
		{
			using (var control = new ExportOfficesUserControl())
			{
				AssertEquals("ClearanceGroupBox caption should be", "Clearance Local", control.ClearanceGroupBox.CaptionResourceString.Caption);
				AssertEquals("BoardingGroupBox caption should be", "Boarding Local", control.BoardingGroupBox.CaptionResourceString.Caption);
			}
		}
	}
}
