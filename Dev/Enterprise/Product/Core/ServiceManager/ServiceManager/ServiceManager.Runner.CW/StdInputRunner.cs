using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using CargoWise.Common;
using ServiceManager.Common.Abstractions;
using ServiceManager.Logging.CW;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Runner
{
	class StdInputRunner : TaskRunner
	{
		public StdInputRunner(
			IRunnerLogger logger,
			IServiceTaskHandlerInitializer serviceTaskHandlerInitializer,
			IServiceTaskScheduleManager serviceTaskScheduleManager,
			IHostedServiceAttributeProvider hostedServiceAttributeProvider,
			IClientHostedServiceAttributeProvider clientHostedServiceAttributeProvider)
			: base(logger, clientHostedServiceAttributeProvider)
		{
			this.serviceTaskHandlerInitializer = serviceTaskHandlerInitializer ?? throw new ArgumentNullException(nameof(serviceTaskHandlerInitializer));
			this.serviceTaskScheduleManager = serviceTaskScheduleManager ?? throw new ArgumentNullException(nameof(serviceTaskScheduleManager));
			this.hostedServiceAttributeProvider = hostedServiceAttributeProvider ?? throw new ArgumentNullException(nameof(hostedServiceAttributeProvider));
		}

		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		[SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
		static extern bool AllocConsole();

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Catching this exception is necessary to write to console during debugging session")]
		[SuppressMessage("CargoWiseOne", "CW1106:Do Not Leave In Debug Messages", Justification = "Baseline")]
		protected override RunnerExitCode RunInternal(bool singleRun)
		{
			AllocConsole();
			LoggerConfiguration.InitializeLoggingConsole();

			Console.WriteLine("Welcome to the ServiceManager function debugging runner.");
			Console.WriteLine("If you are using this option frequently, consider to write a test to capture what you are doing.");
			Console.WriteLine("This will make your testing more predictable, and faster.");
			Console.WriteLine("");
			Console.WriteLine("'run' is the default command and can be omitted.");
			Console.WriteLine("");
			Console.WriteLine("Syntax examples:");
			Console.WriteLine("run -assemblyName:Enterprise.ServiceManager.Tasks.MailProcessor -code:IMS -configString:<HostedServiceSerializableSettings/>");
			Console.WriteLine("run IMS");
			Console.WriteLine("IMS");

			var parser = new StdInputRunnerCommandLineArgsParser(HostedServiceAttributeProvider);

			while (true)
			{
				Console.WriteLine("");
				Console.WriteLine("Please type your command (type stop/exit to exit)");

				ICommandInfo commandInfo;
				try
				{
					commandInfo = parser.Parse(Console.ReadLine());
				}
				catch (ArgumentException argumentException)
				{
					Console.WriteLine(argumentException.Message);
					continue;
				}

				switch (commandInfo)
				{
					case IStopCommandInfo _:
						return 0;
					case IRunCommandInfo runCommandInfo:
						try
						{
							Console.WriteLine("Initialising");
							var configString = runCommandInfo.ConfigString;
							var serviceAttribute = hostedServiceAttributeProvider.GetHostedServiceAttribute(runCommandInfo.AssemblyName, runCommandInfo.Code);
							var schedule = serviceTaskScheduleManager.ConfigureSchedules(new[] { serviceAttribute }).Single(t => t.Code == runCommandInfo.Code);
							if (string.IsNullOrEmpty(configString))
							{
								configString = schedule.ConfigString;
							}
							var serviceTask = serviceTaskHandlerInitializer.CreateServiceTaskHandler(runCommandInfo.AssemblyName, runCommandInfo.Code, configString);
							Console.WriteLine("Running");
							serviceTask.Run(CancellationToken.None);
							Console.WriteLine("Complete");
						}
						catch (Exception e) when (!e.IsCriticalException())
						{
							Console.WriteLine("An exception was thrown:");
							Console.WriteLine(e.ToString());
						}

						break;

					default:
						Console.WriteLine($"Unrecognised command '{commandInfo.GetType().Name}'.");
						break;
				}
			}
		}

		readonly IServiceTaskHandlerInitializer serviceTaskHandlerInitializer;
		readonly IServiceTaskScheduleManager serviceTaskScheduleManager;
		readonly IHostedServiceAttributeProvider hostedServiceAttributeProvider;
	}
}
