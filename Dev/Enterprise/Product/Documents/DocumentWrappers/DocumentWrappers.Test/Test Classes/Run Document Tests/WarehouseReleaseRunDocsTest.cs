using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class WarehouseReleaseRunDocsTest : BaseRunDocumentsTest
	{
		public WarehouseReleaseRunDocsTest() { }

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
		public void TestNeutralStraightBillOfLading()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Neutral Straight Bill of Lading");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			RunDocumentWithAllSections = ZBool.True;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestNeutralStraightBillOfLadingLaser()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Straight Bill Of Lading");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			RunDocumentWithAllSections = ZBool.True;
			RunDocument();
		}
	}
}
