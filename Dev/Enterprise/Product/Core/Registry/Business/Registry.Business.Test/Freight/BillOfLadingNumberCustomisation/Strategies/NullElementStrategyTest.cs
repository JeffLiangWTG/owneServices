using System;
using Enterprise.Registry.Business.BillCustomisationStrategies;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class NullElementStrategyTest : TestCase
	{
		public void TestGetRegExForDataType()
		{
			var strategy = new NullElementStrategy();
			var customisation = new BillOfLadingNumberCustomisation();

			var element = new BillOfLadingNumberCustomisationElement(customisation, strategy);
			AssertExceptionThrown<NotImplementedException>(() => strategy.GetRegExForDataType(element));
		}
	}
}
