using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	class EALAESGoodsShipmentWrapperTest : WrapperHelperTest<EALAESGoodsShipmentWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if Exit Report is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "exitReport"), () => GetWrapper(null));

				AssertExceptionThrown("Constructor Throws Exception if Consignment is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "Consignment"), () => GetWrapper(Factory.New<CusExitReport>()));
			});
		}

		public void TestConsignment()
		{
			var consignment = wrapper.Consignment;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Consignment", consignment);
				AssertSame("Cached Consignment", wrapper.Consignment, consignment);
			});
		}

		public void TestGoodsItem()
		{
			CombineAssertions(() =>
			{
				exitReport.CER_Calc_Discrepancies = ZBool.True;
				AssertEquals("Expected empty GoodsItem list when there are discrepancies but there are no reportItems", 0, wrapper.GoodsItem.Count);

				var exitConsignmentItem1 = exitConsignment.CusExitConsignmentItems.AddNew();
				exitConsignmentItem1.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
				var exitReportItem1 = exitReport.CusExitReportItems.AddNew();
				exitReportItem1.ERI_CCI_ConsignmentItem = exitConsignmentItem1.PK;

				var exitConsignmentItem2 = exitConsignment.CusExitConsignmentItems.AddNew();
				exitConsignmentItem2.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
				var exitReportItem2 = exitReport.CusExitReportItems.AddNew();
				exitReportItem2.ERI_CCI_ConsignmentItem = exitConsignmentItem2.PK;

				exitReport.CusExitReportItems.AddNew();

				wrapper = GetWrapper(exitReport);
				var goodsItem = wrapper.GoodsItem;
				AssertEquals("Expected filled GoodsItem with 2 elements when there are discrepancies and there are reportItems associated to consignmentItems", 2, goodsItem.Count);
				AssertSame("Cached GoodsItem", wrapper.GoodsItem, goodsItem);

				exitReport.CER_Calc_Discrepancies = ZBool.False;
				wrapper = GetWrapper(exitReport);
				AssertEquals("Expected null GoodsItem when no discrepancies", 0, wrapper.GoodsItem.Count);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			CusExitHeader exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitConsignment = exitHeader.CusExitConsignments.AddNew();
			exitReport = exitHeader.CusExitReports.AddNew();
			exitReport.CER_CXC_Consignment = exitConsignment.PK;

			wrapper = GetWrapper(exitReport);
		}
		CusExitConsignment exitConsignment;
		CusExitReport exitReport;
		EALAESGoodsShipmentWrapper wrapper;

		EALAESGoodsShipmentWrapper GetWrapper(CusExitReport exitReport) => new EALAESGoodsShipmentWrapper(exitReport);

		protected override EALAESGoodsShipmentWrapper GetProvider() => wrapper;
	}
}
