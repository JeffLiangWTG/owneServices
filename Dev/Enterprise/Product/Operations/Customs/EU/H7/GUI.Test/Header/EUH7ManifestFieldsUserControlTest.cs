using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7ManifestFieldsUserControl))]
	sealed class EUH7ManifestFieldsUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestCustomsOffices()
		{
			using (var control = new EUH7ManifestFieldsUserControl())
			{
				control.Show();

				var customsOfficesLabel = control.FindSingle<ZLabel>("CustomsOfficeLabel");
				var customsOfficeFindbox = control.FindSingle<ZCodeFindBox>("CustomsOfficeCodeFindBox");
				var presentationOfficeFindbox = control.FindSingle<ZCodeFindBox>("PresentationOfficeCodeFindBox");
				var presenterAddressControl = control.FindSingle<ZAddressControl>("PresenterAddressControl");

				CombineAssertions(() =>
				{
					AssertEquals("Customs Offices", customsOfficesLabel.CaptionResourceString.Caption);

					AssertEquals("Lodgement Caption", "Lodgement", customsOfficeFindbox.CaptionResourceString.Caption);
					AssertEquals("Lodgement MediumCaption", "Lodgement", customsOfficeFindbox.CaptionResourceString.MediumCaption);
					AssertEquals("Lodgement ShortCaption", "Lodgement", customsOfficeFindbox.CaptionResourceString.ShortCaption);
					AssertEquals("Lodgement FullDescription", "Code identifying the Customs Office of Lodgement of the declaration.", customsOfficeFindbox.CaptionResourceString.FullDescription);
					AssertEquals("AMA_CustomsOffice", customsOfficeFindbox.GetBindingMember());

					AssertEquals("Presentation Caption", "Presentation", presentationOfficeFindbox.CaptionResourceString.Caption);
					AssertEquals("Presentation MediumCaption", "Presentation", presentationOfficeFindbox.CaptionResourceString.MediumCaption);
					AssertEquals("Presentation ShortCaption", "Presentation", presentationOfficeFindbox.CaptionResourceString.ShortCaption);
					AssertEquals("Presentation FullDescription", "Code identifying the Customs Office of goods presentation.", presentationOfficeFindbox.CaptionResourceString.FullDescription);
					AssertEquals("PresentationOffice", presentationOfficeFindbox.GetBindingMember());

					AssertEquals("PresenterAddressControl", presenterAddressControl.Name);
					AssertEquals("AMA_OA_Presenter", presenterAddressControl.GetBindingMember());
				});
			}
		}

		public void TestConsolidateStatus()
		{
			using (var control = new EUH7ManifestFieldsUserControl())
			{
				control.Show();

				var separator = control.FindSingle<SeparatorUserControl>("ConsolidatedStatusSeparatorUserControl");
				var statusDropEdit = control.FindSingle<ZDropEdit>("ConsolidatedCustomsStatusDropEdit");

				CombineAssertions(() =>
				{
					AssertEquals("Consolidated Status", separator.CaptionResourceString.Caption);

					AssertEquals("Caption", "Customs Status", statusDropEdit.CaptionResourceString.Caption);
					AssertEquals("Medium Caption", "Cus. Status", statusDropEdit.CaptionResourceString.MediumCaption);
					AssertEquals("Short Caption", "Cus. Status", statusDropEdit.CaptionResourceString.ShortCaption);
					AssertEquals("Presentation FullDescription", "Code identifying the customs status of the declaration.", statusDropEdit.CaptionResourceString.FullDescription);
					AssertEquals("RegistrationStatus", statusDropEdit.GetBindingMember());
				});
			}
		}
	}
}
