using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.ZArchitecture.DevTools;

namespace Enterprise.ZArchitecture.Tools
{
	sealed class MacroEvaluationDevTool : IDevTool
	{
		public MacroEvaluationDevTool()
		{
			this.lazyTool = new Lazy<IMacroEvaluationTool>(ObjectFactory.Get<IMacroEvaluationTool>);
		}

		readonly Lazy<IMacroEvaluationTool> lazyTool;

		public string Name => lazyTool.Value.Name;
		public bool AddAsButton => lazyTool.Value.ShowAsButton;

		public void Show(Form form)
		{
			lazyTool.Value.Run(form);
		}
	}
}
