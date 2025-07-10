using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class BusinessObjectLoggingServiceTest : TestCaseWithFactory
	{
		public void TestDoNotCreateLogsForNonLogTargetBizo()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			Assert(!(dummy is IAutoAdminLogTarget));

			var bizosInFactory = ProcessBusinessObjectAndGetTheFinalBizosArray(dummy, 0);

			AssertEquals(bizosInFactory.Count, 1);
		}

		IReadOnlyList<BusinessObject> ProcessBusinessObjectAndGetTheFinalBizosArray(BusinessObject bizo, int expectedDeterredLoggerCalls)
		{
			var deferredLogger = new DummyBizoLoggerDeferred(Factory);
			var nonDeferredLogger = new DummyBizoLoggerNonDeferred(Factory);

			var businessObjectLoggingService = new BusinessObjectLoggingService(b => new IBusinessObjectLogger[] { deferredLogger, nonDeferredLogger });

			businessObjectLoggingService.ProcessBusinesObjects(new[] { bizo });

			AssertEquals(expectedDeterredLoggerCalls, deferredLogger.CreateSaveLogCount);
			AssertEquals(0, nonDeferredLogger.CreateSaveLogCount);

			return ((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects;
		}
	}
}
