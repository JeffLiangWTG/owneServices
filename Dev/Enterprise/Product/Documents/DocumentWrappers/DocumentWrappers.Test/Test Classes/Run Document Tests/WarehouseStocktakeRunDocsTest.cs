using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class WarehouseStocktakeRunDocsTest : BaseRunDocumentsTest
	{
		public WarehouseStocktakeRunDocsTest() { }

		TestDataSimpleEnvironment Data
		{
			get
			{
				if (data == null)
				{
					data = new TestDataSimpleEnvironment(Factory);
					Helper.CreateRowAndGenerateLocations(Data.Whs1, "SSS", 1, 1, 2);
					Factory.Save();
				}
				return data;
			}
		}
		TestDataSimpleEnvironment data;

		WhsTestHelperFunctions Helper { get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); } }
		WhsTestHelperFunctions helper;

		public override BusinessObject GetBusinessObject
		{
			get
			{
				var stocktake = Helper.CreateWhsStocktake(Data.Org1, Data.Whs1, StocktakeStatus.Codes.Loaded);

				stocktake.Lines.AddNew();
				stocktake.Lines.AddNew();
				stocktake.Lines.AddNew();
				stocktake.Lines.AddNew();

				stocktake.Lines[0].LocationString = "SSS-1-1-1";
				stocktake.Lines[0].WU_OP = Data.Part1.PK;
				stocktake.Lines[0].WU_SystemUnits = 10;
				stocktake.Lines[0].WU_LastCount = 5;

				stocktake.Lines[1].LocationString = "SSS-1-1-1";
				stocktake.Lines[1].WU_OP = Data.Part1.PK;
				stocktake.Lines[1].WU_SystemUnits = 99.99;
				stocktake.Lines[1].WU_LastCount = 99.98;

				stocktake.Lines[2].LocationString = "SSS-1-1-2";
				stocktake.Lines[2].WU_OP = Data.Part1.PK;
				stocktake.Lines[2].WU_SystemUnits = 500;
				stocktake.Lines[2].WU_LastCount = 490;

				stocktake.Lines[3].WU_OP = Data.Part1.PK;
				stocktake.Lines[3].LocationString = "SSS-1-1-2";
				stocktake.Lines[3].WU_SystemUnits = 100;
				stocktake.Lines[3].WU_LastCount = 0;
				stocktake.Lines[3].WU_Status = Enterprise.Warehouse.Transactions.CodeLists.StocktakeLineStatus.Codes.Closed;

				return stocktake;
			}
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.WhsStocktake; }
		}

		ZQuery fFilterForMenuItem;
		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		[ExpectNoExceptions]
		public void TestStocktakeSheet()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Stocktake Sheet");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			RunDocumentWithAllSections = ZBool.True;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestStocktakeSheetWithSystemCounts()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Stocktake Sheet With System Counts");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			RunDocumentWithAllSections = ZBool.True;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestStocktakeVarianceByLocation()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Stocktake Variance By Location");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			RunDocumentWithAllSections = ZBool.True;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestStocktakeVarianceByProduct()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Stocktake Variance By Product");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			RunDocumentWithAllSections = ZBool.True;
			RunDocument();
		}
	}
}
