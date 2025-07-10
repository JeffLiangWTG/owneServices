using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngineCore
{
	public class DocumentEngineVersionRetriever
	{
		public ZInt ActivePrintersCount
		{
			get
			{
				ZQuery filter = new ZQuery(StmPrintQueueSchema.SQ_AllowPrinting, ZBool.True);
				return Factory.GetDatabaseCount(ObjectFactory.GetType<Integration.DocumentEngine.IStmPrintQueue>(), filter);
			}
		}

		BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}
				return factory;
			}
		}
		BusinessObjectFactory factory;
	}
}
