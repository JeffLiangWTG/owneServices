using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class WarehouseRunDocsTest : BaseRunDocumentsTest
	{
		public WarehouseRunDocsTest() { }

		public override BusinessObject GetBusinessObject
		{
			get { return Factory.New<WhsOrder>(); }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.WhsOrder; }
		}

		ZQuery fFilterForMenuItem;
		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		[ExpectNoExceptions]
		public void TestOrderCopy()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Order Copy");
			RunDocument();
		}
	}
}
