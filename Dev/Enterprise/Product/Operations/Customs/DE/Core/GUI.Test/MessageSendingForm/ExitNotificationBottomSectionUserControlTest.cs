using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class ExitNotificationBottomSectionUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new ExitNotificationBottomSectionUserControl())
			{
				AssertNotNull("MissingQuantityCheckBox", control.FindSingleOrDefault<ZCheckBox>("MissingQuantityCheckBox"));
				AssertNotNull("RedirectionCheckbox", control.FindSingleOrDefault<ZCheckBox>("RedirectionCheckbox"));
				AssertNotNull("IntendedExitCustomsOfficeFindbox", control.FindSingleOrDefault<ZCodeFindBox>("IntendedExitCustomsOfficeFindbox"));
				AssertNotNull("FinalizationCheckbox", control.FindSingleOrDefault<ZCheckBox>("FinalizationCheckbox"));
				AssertNotNull("ItemsGroupBox", control.FindSingleOrDefault<ZGroupBox>("ItemsGroupBox"));
				AssertNotNull("PackingGroupBox", control.FindSingleOrDefault<ZGroupBox>("PackingGroupBox"));
				AssertNotNull("ItemsGrid", control.FindSingleOrDefault<ZGrid>("ItemsGrid"));
				AssertNotNull("PackagingGrid", control.FindSingleOrDefault<ZGrid>("PackagingGrid"));
			}
		}
	}
}
