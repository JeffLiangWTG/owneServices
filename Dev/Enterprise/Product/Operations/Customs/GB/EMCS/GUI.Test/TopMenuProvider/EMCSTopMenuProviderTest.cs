using System;
using Enterprise.Customs.EU.EMCS.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.GUI.Testing
{
	[TestedType(typeof(EMCSTopMenuProvider))]
	sealed class EMCSTopMenuProviderTest : EMCSTopMenuProviderAbstractTest<EMCSTopMenuProvider>
	{
		protected override string CountryOrGroupingCode => Core.Constants.CountryCodes.UnitedKingdom;

		protected override Type ExpectedMenuType => typeof(EMCSMenu);
	}
}
