using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	class PresentationUserControlTest : TestCaseWithFactory
	{
		public void TestPresentationGroupBox()
		{
			using (var control = new PresentationUserControl())
			{
				AssertEquals("PresentationGroupBox.Caption", "Presentation", control.FindSingleOrDefault<ZGroupBox>("PresentationGroupBox").CaptionResourceString.Caption);
			}
		}

		public void TestPresentationStartDate()
		{
			using (var control = new PresentationUserControl())
			{
				var startDateDateEdit = control.FindSingleOrDefault<ZDateEdit>("StartDateDateEdit");
				AssertNotNull(startDateDateEdit);
				AssertEquals("StartDateDateEdit.DateTimeFormat", ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds, startDateDateEdit.DateTimeFormat);
			}
		}
	}
}
