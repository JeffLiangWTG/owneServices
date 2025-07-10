using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class AFRMessageNumberGeneratorTest : TransactionedTestCase
	{
		public void TestGetMessageReferenceNumber()
		{
			var factory = new BusinessObjectFactory();
			var fountain = new FormattedNumberFountainFactory("JPAFRMESSAGENUMBER", "JP").New();
			fountain.SetNext(factory, 1001);
			using (((IDbConnected)factory).Connection.BeginTransactionWithManager())
			{
				AssertEquals("JP00001001", new AFRMessageNumberGenerator(factory).GetMessageReferenceNumber());
			}
		}
	}
}
