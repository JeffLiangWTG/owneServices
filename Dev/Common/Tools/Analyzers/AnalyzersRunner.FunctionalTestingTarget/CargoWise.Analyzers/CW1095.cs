using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1095 : ZPage
	{
		public void Method()
		{
			//CW1095:HTTP Parameter Names Should Not Be Translated
			GetGuidFromParameter(Res.GetString("resource key", "english text"));
		}
	}
}
