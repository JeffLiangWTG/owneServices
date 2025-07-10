using System.Windows.Forms;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1094 : Control
	{
		internal string ToolTipText { get; set; }

		public void Method()
		{
			//CW1094:ToolTipText Should Be Set With Res.GetString
			ToolTipText = "text";
		}
	}
}
