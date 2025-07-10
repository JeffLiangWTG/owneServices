#if DEBUG
using System.Collections;
using System.Windows.Forms;
using System.Windows.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Startup
{
	class StartDat : AbstractApplicationStartupTask
	{
		public StartDat(ApplicationStartupDirector startupDirector)
		{
			this.startupDirector = startupDirector;
		}

		public override string TaskDescription => "*** Running DAT ***";

		public override int FailureExitCode => ExitCodes.StartDatError;

		protected override bool GetShouldExecute(CommandLineArguments arguments)
		{
			return (bool)arguments[ApplicationArguments.OptionTestAdapter] ||
						 arguments[ApplicationArguments.OptionTestOnly] != null ||
						 arguments[ApplicationArguments.OptionLabelCaptionLogger] != null;
		}

		protected override bool DoExecute(CommandLineArguments arguments)
		{
			if ((bool)arguments[ApplicationArguments.OptionTestAdapter])
			{
				DoStartTestAdapter(arguments);
			}
			else if (arguments[ApplicationArguments.OptionLabelCaptionLogger] != null)
			{
				new ResourceStrings.Maintenance.Test.LabelCaptionLogger().Run((string)arguments[ApplicationArguments.OptionLabelCaptionLogger]);
			}
			else
			{
				LoadTestOnlyForm(arguments);
			}
			return false;
		}

		void DoStartTestAdapter(CommandLineArguments arguments)
		{
			string enterprisePath = (string)arguments[ApplicationArguments.OptionDatEnterprisePath];

			SetupSystemForTesting();

			NUnit.Framework.TestingState.Setup();
			NUnit.Framework.SnailTestAttribute.IncludeSnailTests = true;

			using (var datForm = new DatForm(enterprisePath))
			{
				ApplicationDispatcher.Current = new ZDispatcherSynchronizationContext(Dispatcher.CurrentDispatcher);
				Application.Run(datForm);
			}
		}

		void LoadTestOnlyForm(CommandLineArguments arguments)
		{
			SetupSystemForTesting();
			string[] assemblies = arguments[ApplicationArguments.OptionTestOnly].ToString().Split(new char[] { ',', ';' });
			UnitTestRunner.ShowUnitTestsFromAssemblies(assemblies, "", new Testing.UnitTestErrorDescriptionList(), true);
		}

		void SetupSystemForTesting()
		{
			MainForm.LoadModuleTree();
			SqlSynonymNameResolver.Initialize();
			foreach (IModuleInitialisedAtDATStartup module in ObjectFactory.Get<ArrayList>("ModulesInitialisedAtDATStartup"))
			{
				module.Initialise();
			}
			startupDirector.DisposeSplash();
		}

		readonly ApplicationStartupDirector startupDirector;
	}
}
#endif
