using System;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	[TestedType(typeof(EMCSTopMenuProvider))]
	sealed class EMCSTopMenuProviderBaseOnlyTest : EMCSTopMenuProviderAbstractTest<EMCSTopMenuProvider>
	{
		protected override Type ExpectedMenuType => typeof(EMCSMenu);

		protected override string CountryOrGroupingCode => Core.Constants.CountryCodes.Portugal;
	}
}
