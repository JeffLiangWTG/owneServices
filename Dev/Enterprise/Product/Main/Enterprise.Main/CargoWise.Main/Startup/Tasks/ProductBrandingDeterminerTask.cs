using System;
using CargoWise.BrandManager;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Startup
{
	public abstract class ProductBrandingDeterminerTask : IApplicationStartupTask
	{
		internal ProductBrandingDeterminerTask(Action updateBrandingCallback)
		{
			this.updateBrandingCallback = updateBrandingCallback;
		}

		protected readonly Action updateBrandingCallback;

		string IApplicationStartupTask.TaskDescription => string.Empty; // Showing a description for this might be confusing...

		public abstract int FailureExitCode { get; }

		protected virtual bool Execute(CommandLineArguments arguments)
		{
			var isPWEnabled = CheckForProductivityWiseModeEnabled(arguments);

			if (isPWEnabled)
			{
				BrandingFactory.Configure(BrandingFactory.BrandingType.ProductivityWise);
			}
			else
			{
				BrandingFactory.Configure(BrandingFactory.BrandingType.CargoWiseOne);
			}

			updateBrandingCallback();

			return true;
		}

		bool IApplicationStartupTask.Execute(CommandLineArguments arguments) => Execute(arguments);

		public abstract bool CheckForProductivityWiseModeEnabled(CommandLineArguments arguments);

		bool IApplicationStartupTask.ShouldExecute(CommandLineArguments arguments) => true;
	}
}
