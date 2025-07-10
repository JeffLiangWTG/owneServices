using CargoWise.Common;
using CargoWise.Macros;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class Console : IConsole
	{
		public Console(IConsoleService service)
		{
			Argument.NotNull(service, nameof(service));
			this.service = service;
		}

		readonly IConsoleService service;

		[MacroInvokable]
		public void Log(object obj)
		{
			var map = obj.ToMap();
			service.Log(map?.ToJSON() ?? obj);
		}
	}
}
