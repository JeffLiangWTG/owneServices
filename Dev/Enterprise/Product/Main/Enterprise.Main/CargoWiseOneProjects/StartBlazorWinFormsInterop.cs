#if DEBUG
using System;
using CargoWise.Application;
using CargoWise.Definitions;
using Enterprise.BlazorWinFormsInterop;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Startup.Tasks
{
	class StartBlazorWinFormsInterop : AbstractApplicationStartupTask
	{
		public override string TaskDescription => "Starting Blazor/WinForms Interop";

		public override int FailureExitCode => ExitCodes.StartBlazorWinFormsInteropError;

		protected override bool GetShouldExecute(CommandLineArguments arguments)
		{
			return (bool)arguments[ApplicationArguments.OptionStartBlazorWinFormsInterop];
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Baseline")]
		protected override bool DoExecute(CommandLineArguments arguments)
		{
			var winFormsListener = ObjectFactory.Get<IWinFormsListener>();
			winFormsListener.StartListener();
			Console.Out.WriteLine(winFormsListener.ListenUrl);

			return true; // success
		}
	}
}
#endif
