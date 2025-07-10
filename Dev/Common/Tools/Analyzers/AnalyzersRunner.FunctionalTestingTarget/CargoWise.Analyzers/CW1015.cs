namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1015
	{
		public void Method()
		{
			//CW1015:No Application.OpenForms Rule
			foreach (var form in System.Windows.Forms.Application.OpenForms)
			{ }
		}
	}
}
