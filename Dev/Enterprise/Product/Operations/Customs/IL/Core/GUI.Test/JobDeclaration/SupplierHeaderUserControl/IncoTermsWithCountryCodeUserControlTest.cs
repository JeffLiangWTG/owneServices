using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.GUI.Testing
{
	class IncoTermsWithCountryCodeUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new IncoTermsWithCountryCodeUserControl())
			{
				var incoTermsUserControl = control.Controls.Find("IncoTermsUserControl", true).FirstOrDefault();
				AssertNotNull(incoTermsUserControl);

				var incoTermsCountryCodeFindBox = control.Controls.Find("IncoTermsCountryCodeFindBox", true).FirstOrDefault();
				AssertNotNull(incoTermsCountryCodeFindBox);
			}
		}
	}
}
