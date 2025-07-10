using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class GoodsItemsGridUserControlTest : TestCaseWithFactory
	{
		public void TestGridCustomsValueCaption()
		{
			using (var control = new GoodsItemsGridUserControl())
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Departure);
				control.SetDataBinding(header, "MovementHeader.GoodsItems");
				AssertEquals("Value in EUR", control.GoodsItemsGrid.GetColumnStyle(nameof(NctsCommonCargoDesc.BY_MonetaryValue)).Caption);
			}
		}

		public void TestTariffColumnStyle()
		{
			using (var control = new GoodsItemsGridUserControl())
			{
				var columnStyle = control.GoodsItemsGrid.GetColumnStyle(nameof(NctsCommonCargoDesc.BY_FormattedHarmonisedTariff));
				AssertType<Universal.GUI.TariffColumnStyleInfo>(columnStyle);

				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Departure);
				control.SetDataBinding(header, "MovementHeader.GoodsItems");
				var getEffectiveDate = ((Universal.GUI.TariffColumnStyleInfo)columnStyle).GetEffectiveDate;
				AssertNotNull("ColumnStyle.GetEffectiveDate", getEffectiveDate);

				var testDate = new ZDateTime(2022, 11, 1);
				header.MovementHeader.BM_ValuationDate = testDate;

				AssertEquals(testDate, getEffectiveDate());
			}
		}

		public void TestDutiableAndVatAmountVisibleInGrid()
		{
			using (var control = new GoodsItemsGridUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("DutyAmount", true, control.GoodsItemsGrid.GetColumnStyle(nameof(NctsDepartureCargoDesc.DutyAmount)).IsVisible);
					AssertEquals("VatAmount", true, control.GoodsItemsGrid.GetColumnStyle(nameof(NctsDepartureCargoDesc.VatAmount)).IsVisible);
				});
			}
		}

		public void TestGoodsItemsRowsWithoutDeleteConfirmationSupport()
		{
			using (var control = new GoodsItemsGridUserControl())
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

				AssertEquals("Pre: Delete confirmation not supported in EU", false, nctsHeader.Configuration.GoodsItemsConfiguration.DeleteConfirmationSupport(nctsHeader));

				control.SetDataBinding(nctsHeader, ZString.Empty);
				var bill = nctsHeader.Bills.AddNew();
				control.GoodsItemsGrid.SetDataBinding(bill.GoodsItems, ZString.Empty);

				for (int i = 0; i < 10; i++)
				{
					var item1 = bill.GoodsItems.AddNew();
					item1.BY_Description = "ABC" + i;
				}

				AssertEquals("Before Deleting Goods items", 10, bill.GoodsItems.Count);

				control.GoodsItemsGrid.SelectAllElements();
				control.GoodsItemsGrid.OnDeleteKeyPressed();

				AssertEquals("After Deleting Goods items", 0, bill.GoodsItems.Count);
			}
		}

		public void TestGoodsItemsRowsWithDeleteConfirmationSupport()
		{
			using (var control = new GoodsItemsGridUserControl())
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemsConfiguration<ZBool>(Factory, "DeleteConfirmationSupportCore", true, ItExpr.IsAny<NctsHeader>()))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

				Assert(nctsHeader.Configuration.GoodsItemsConfiguration.DeleteConfirmationSupport(nctsHeader));

				control.SetDataBinding(nctsHeader, ZString.Empty);
				var bill = nctsHeader.Bills.AddNew();
				control.GoodsItemsGrid.SetDataBinding(bill.GoodsItems, ZString.Empty);

				for (int i = 0; i < 10; i++)
				{
					var item1 = bill.GoodsItems.AddNew();
					item1.BY_Description = "ABC" + i;
				}
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				AssertEquals("Before Deleting Goods items", 10, bill.GoodsItems.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				control.GoodsItemsGrid.SelectAllElements();
				control.GoodsItemsGrid.OnDeleteKeyPressed();

				AssertEquals("After clicking 'No' on popup, should not delete lines", 10, bill.GoodsItems.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				control.GoodsItemsGrid.SelectAllElements();
				control.GoodsItemsGrid.OnDeleteKeyPressed();

				AssertEquals("After clicking 'Yes' on popup, should delete lines", 0, bill.GoodsItems.Count);
			}
		}
	}
}
