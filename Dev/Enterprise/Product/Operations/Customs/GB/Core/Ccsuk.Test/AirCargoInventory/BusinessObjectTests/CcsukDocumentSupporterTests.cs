using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	[TestedType(typeof(CcsukDocumentSupporter))]
	class CcsukDocumentSupporterTests : DocumentSupporterTest
	{
		public void TestGetFilterValue()
		{
			var mawb = Factory.New<CusMAWB>();
			var supporter = new CcsukDocumentSupporter(mawb);
			AssertEquals("GB", supporter.GetFilterValue(DocumentFilters.CTY));
			AssertEquals(null, supporter.GetFilterValue(DocumentFilters.SGAIRSHP)); // random base value
		}

		public void TestErrorMessagesForRRADocument()
		{
			var basic = Factory.New<CusMAWB>();
			var menuItem = Factory.Load<StmMenuItem>(PrinterFromEdiMessageHelper_MenuKeys.Ccsuk.RRA_ReleaseRemovalAuthority);
			var result = basic.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			Assert(result.IsValid);
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			result = mawb.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			Assert(!result.IsValid);
			AssertContains("consol", result.ErrorMessage);
			result = hawb.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			Assert(result.IsValid);
			var split = hawb.Splits.AddNew();
			result = hawb.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			Assert(!result.IsValid);
			AssertContains("split", result.ErrorMessage);
			result = split.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			Assert(result.IsValid);
		}

		public void TestWrappersForConsignmentDetailsReport()
		{
			var reportMenuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, CcsukDocumentSupporter.ConsignmentDetailsReportName));
			var basic = Factory.New<CusMAWB>();
			var supporter = new CcsukDocumentSupporter(basic);
			AssertNull(supporter.GetDocumentWrappers(Core.Constants.DataContext.Cartage, reportMenuItem));
			var wrappers = supporter.GetDocumentWrappers(Core.Constants.DataContext.GbCcsuk, reportMenuItem);
			AssertEquals(1, wrappers.Length);
			AssertContains("CcsukWrapperForConsignmentReport", wrappers.First().GetType().FullName);
		}

		public void TestGetBODocDataProvidersNotFoundMessage()
		{
			var reportMenuItem = Factory.New<StmMenuItem>();
			var basic = Factory.New<CusMAWB>();
			var supporter = new CcsukDocumentSupporter(basic);
			AssertContains("restricted", supporter.GetBODocDataProvidersNotFoundMessage(null, reportMenuItem));
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			IWhsLocation locationBac1;
			IWhsLocation locationBac2;
			IWhsLocation locationCax;
			CusOutTurnTest.CreateWarehouseAreasForTest(Factory, out locationBac1, out locationBac2, out locationCax);

			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			var ot1 = mawb.OutTurns.AddNew();
			ot1.C5_PackagesOutturned = 1;
			ot1.WarehouseLocationID = locationBac1.PK;
			var ot2 = mawb.OutTurns.AddNew();
			ot2.C5_PackagesOutturned = 2;
			ot2.WarehouseLocationID = locationBac2.PK;

			return mawb;
		}
	}
}
