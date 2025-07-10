using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class BusinessObjectLoggerFactoryTest : TestCaseWithFactory
	{
		public void TestReturnsProperLoggers()
		{
			var dummyLogged = Factory.New<DummyAutoLogged>();

			var loggers = BusinessObjectLoggerFactory.GetLoggers(dummyLogged).ToList();

			AssertEquals(1, loggers.Count);
			AssertEquals(true, loggers[0] is AutoAdminBusinessObjectLogger);

			var dummyWithoutLoggingSupport = Factory.New<DummyBusinessObject>();

			AssertEquals(false, BusinessObjectLoggerFactory.GetLoggers(dummyWithoutLoggingSupport).Any());
		}
	}
}