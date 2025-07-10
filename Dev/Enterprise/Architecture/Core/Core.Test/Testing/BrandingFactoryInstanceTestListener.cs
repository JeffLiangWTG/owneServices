using System;
using CargoWise.BrandManager;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class BrandingFactoryInstanceTestListener : BaseTestListener
	{
		public override void BeforeEachTest(DateTime startTime)
		{
			if (instance == null)
			{
				instance = BrandingFactory.Instance;
			}
		}

		public override void AfterEachTest(DateTime endTime)
		{
			if (BrandingFactory.Instance != instance)
			{
				BrandingFactory.Configure(BrandingFactory.BrandingType.CargoWiseOne);
				instance = BrandingFactory.Instance;
			}
		}

		IBranding instance;
	}
}
