using System;
using NUnit.Framework;

namespace CargoWise.Common
{
	public class OverridableTestListener : BaseTestListener
	{
		public override void BeforeEachTest(DateTime startTime)
		{
			Overridable.ResetAll();
		}

		public override void AfterEachTest(DateTime endTime)
		{
			Overridable.ResetAll();
		}
	}
}
