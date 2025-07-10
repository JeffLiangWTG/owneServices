using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(CategorizedApplicationLogger))]
	public class CategorizedApplicationLoggerTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUniqueIndexOnNameProductAndCategory()
		{
			var logger = Factory.New<CategorizedApplicationLogger>();
			logger.CTL_Category = "Test";
			logger.CTL_Name = "Test";
			logger.CTL_Product = "Test";

			var logger2 = Factory.New<CategorizedApplicationLogger>();
			logger2.CTL_Category = "Test";
			logger2.CTL_Name = "Test";
			logger2.CTL_Product = "Test";

			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
		}
	}
}
