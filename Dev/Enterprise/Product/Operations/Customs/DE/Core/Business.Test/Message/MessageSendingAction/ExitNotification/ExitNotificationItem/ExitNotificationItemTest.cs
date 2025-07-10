using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ExitNotificationItem))]
	class ExitNotificationItemTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShouldSend()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Select", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationItem), nameof(ExitNotificationItem.ShouldSend)).Caption);
				AssertEquals(true, exitNotificationItem.ShouldSend);
			});
		}

		public void TestLineNo()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Line No.", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationItem), nameof(ExitNotificationItem.LineNo)).Caption);
				AssertEquals(12, exitNotificationItem.LineNo);
			});
		}

		public void TestGrossMass()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Gross Mass", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationItem), nameof(ExitNotificationItem.GrossMass)).Caption);
				AssertEquals(101m, exitNotificationItem.GrossMass);
			});
		}
		public void TestGross()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Gross", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationItem), nameof(ExitNotificationItem.Gross)).Caption);
				AssertEquals("LB", exitNotificationItem.Gross);
			});
		}
		public void TestNetMass()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Net Mass", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationItem), nameof(ExitNotificationItem.NetMass)).Caption);
				AssertEquals(100m, exitNotificationItem.NetMass);
			});
		}
		public void TestNet()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Net", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationItem), nameof(ExitNotificationItem.Net)).Caption);
				AssertEquals("KG", exitNotificationItem.Net);
			});
		}
		public void TestStatus()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Status", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationItem), nameof(ExitNotificationItem.Status)).Caption);
				AssertEquals("310", exitNotificationItem.Status);
			});
		}

		public void TestStatusDescription()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Status Description", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationItem), nameof(ExitNotificationItem.StatusDescription)).Caption);
				AssertEquals("Gestellung erfolgt", exitNotificationItem.StatusDescription);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => new ExitNotificationItem(exitItem);

		protected override void SetUp()
		{
			base.SetUp();

			CreateExportCustomsStatus();

			exitHeader = Factory.New<CusExitControlHeader>();
			exitHeader.CEH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			exitDetail = exitHeader.CusExitDetails.AddNew();
			exitItem = exitDetail.CusExitItems.AddNew();
			exitItem.CXI_LineNumber = 12;
			exitItem.CXI_GrossMass = 101m;
			exitItem.CXI_GrossMassUQ = "LB";
			exitItem.CXI_NetMass = 100m;
			exitItem.CXI_NetMassUQ = "KG";
			exitItem.CXI_Status = "310";
			exitNotificationItem = (ExitNotificationItem)GetNewBusinessObject();
			exitNotificationItem.ShouldSend = true;
		}
		ExitNotificationItem exitNotificationItem;
		CusExitControlHeader exitHeader;
		CusExitDetail exitDetail;
		CusExitItem exitItem;

		void CreateExportCustomsStatus()
		{
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "CSTEX DESC");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "310", "Gestellung erfolgt", startDate, endDate);
			Factory.Save();
		}
	}
}
