using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.RemotePrinting.Server.Business;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing.Business
{
	[TestedType(typeof(StmPrintQueueExtCollection))]
	class StmPrintQueueExtCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmPrintQueueExtCollection(Factory);
		}
	}
}
