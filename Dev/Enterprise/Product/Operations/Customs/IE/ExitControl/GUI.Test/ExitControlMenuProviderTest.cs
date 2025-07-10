using System;
using Enterprise.Customs.EU.ExitControl.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.GUI.Testing
{
	[TestedType(typeof(ExitControlMenuProvider))]
	class ExitControlMenuProviderTest : ExitControlMenuProviderAbstractTest<ExitControlMenuProvider>
	{
		protected override Type ExpectedConsignmentsGridUserControlMenuProviderType => typeof(ConsignmentsGridUserControlMenuProvider);

		protected override Type ExpectedReportsGridUserControlMenuProviderType => typeof(ReportsGridUserControlMenuProvider);

		protected override Type ExpectedExitControlMainMenuProviderType => typeof(ExitControlMainMenuProvider);

		protected override string CountryOrGroupingCode => Core.Constants.CountryCodes.Ireland;
	}
}
