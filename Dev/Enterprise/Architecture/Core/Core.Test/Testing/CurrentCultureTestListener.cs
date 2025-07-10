using System;
using System.Globalization;
using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class CurrentCultureTestListener : BaseTestListener
	{
		public override void AfterEachTest(DateTime endTime)
		{
			if (!CultureInfo.CurrentCulture.Equals(DefaultCulture.Instance))
			{
				var culture = CultureInfo.CurrentCulture;
				CultureInfo.CurrentCulture = DefaultCulture.Instance;
				Assertion.Fail("CultureInfo.CurrentCulture changed after test to " + culture.ToString());
			}

			base.AfterEachTest(endTime);
		}
	}
}
