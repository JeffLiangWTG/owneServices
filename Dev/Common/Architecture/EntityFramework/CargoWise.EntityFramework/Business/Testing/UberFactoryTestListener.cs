#if DEBUG
using System;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Business.Testing
{
	class UberFactoryTestListener : BaseTestListener
	{
		public override void BeforeEachTest(DateTime startTime)
		{
			base.BeforeEachTest(startTime);
			RowFactory.uberFactory = null;
		}

		public override void AfterEachTest(DateTime endTime)
		{
			RowFactory.uberFactory = null;
			base.AfterEachTest(endTime);
		}
	}
}
#endif
