using System;
using CargoWise.BrandManager;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Startup.Tasks
{
	public class ProductBrandingRegistryDeterminerTask : ProductBrandingDeterminerTask
	{
		public ProductBrandingRegistryDeterminerTask(Action updateBrandingCallback)
			: base(updateBrandingCallback)
		{
		}

		public override int FailureExitCode => ExitCodes.ProductBrandingRegistryDeterminerTaskError;

		public override bool CheckForProductivityWiseModeEnabled(CommandLineArguments arguments) => DataRegistry.Instance.ProductivityWiseModeEnabled;

		protected override bool Execute(CommandLineArguments arguments)
		{
			var isPWEnabled = CheckForProductivityWiseModeEnabled(arguments);

			if (isPWEnabled)
			{
				BrandingFactory.Configure(BrandingFactory.BrandingType.ProductivityWise);
			}
			else
			{
				if (CWNextFeatureHelper.IsCWNextEnabled())
				{
					BrandingFactory.Configure(BrandingFactory.BrandingType.CargoWiseNext);
				}
				else
				{
					BrandingFactory.Configure(BrandingFactory.BrandingType.CargoWiseOne);
				}
			}

			updateBrandingCallback();

			return true;
		}
	}
}
