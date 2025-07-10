using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class OrganisationDetailPlugInUserControlTest : TestCaseWithFactory
	{
		public void TestGroupBoxCaption()
		{
			using (var control = new OrganisationDetailPlugInUserControl())
			{
				AssertEquals("AdditionalIdentificationGroupBox caption must be Additional Identification", "Additional Identification", control.AdditionalIdentificationGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestComponents()
		{
			using (var control = new OrganisationDetailPlugInUserControl())
			{
				CombineAssertions(() =>
				{
					AssertType<ZGrid>("AdditionalIdentificationGrid must be ZGrid", control.AdditionalIdentificationGrid);
					AssertType<ZGroupBox>("AdditionalIdentificationGroupBox must be ZGroupBox", control.AdditionalIdentificationGroupBox);
					AssertType<ZCodeFindBox>("BrokerCodeFindBox must be ZCodeFindBox", control.BrokerCodeFindBox);
				});
			}
		}
	}
}
