using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(TaxChangeAssessmentController))]
	class TaxChangeAssessmentControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.DE.TaxChangeAssessment;

		protected override string CountryCode => Core.Constants.CountryCodes.Germany;
	}
}
