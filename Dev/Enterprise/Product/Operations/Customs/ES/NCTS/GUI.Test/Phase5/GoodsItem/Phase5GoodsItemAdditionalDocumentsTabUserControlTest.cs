using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	sealed class Phase5GoodsItemAdditionalDocumentsTabUserControlTest : TestCaseWithFactory
	{
		public void TestGoodsItemAdditionalDocumentsGridUserControl_Arrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			userControl.SetDataBinding(nctsHeader, "");

			var additionalDocumentsSplitContainer = userControl.FindSingleOrDefault<KSplitContainer>("AdditionalDocumentsSplitContainer");
			var additionalDocumentsGridUserControl = additionalDocumentsSplitContainer.Panel1.FindSingle<ArrivalGoodsItemAdditionalDocumentsGridUserControl>("GoodsItemAdditionalDocumentsGridUserControl");

			CombineAssertions(() =>
			{
				AssertType<ArrivalGoodsItemAdditionalDocumentsGridUserControl>(additionalDocumentsGridUserControl);
				AssertEquals("Dock", DockStyle.Fill, additionalDocumentsGridUserControl.Dock);
				AssertEquals("BindingMember", ".", additionalDocumentsGridUserControl.GetBindingMember());
			});
		}

		public void TestGoodsItemAdditionalDocumentsGridUserControl_ArrivalTNN()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

			var arrivalMovement = nctsHeader.ArrivalMovementHeader;
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.ArrivalMrnFromUser = "ES123456";
			nctsHeader.ESNctsHeader.CEN_TNNArrival = false;

			var tnnDataCodeInfo = TnnDataCodeInfo.LoadNew(nctsHeader);
			tnnDataCodeInfo.AcceptanceDate = ZDateTime.Now;
			tnnDataCodeInfo.ClearanceDate = ZDateTime.Now.AddDays(-1);

			arrivalMovement.GenerateTNNDeparture(tnnDataCodeInfo);
			Factory.Save();

			var headerTNN = arrivalMovement.HeaderTNN;
			var bill = headerTNN.Bills.AddNew();
			var item = bill.GoodsItems.AddNew();
			var additonalDoc = item.AdditionalInfos.AddNew();
			Factory.Save();

			using (var form = new Phase5DepartureMovementForm(headerTNN))
			{
				form.Show();

				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				mainTabControl.SelectTab(form.HouseConsignmentsTabPage);

				var houseConsignmentsTabUserControl = (EU.NCTS.GUI.HouseConsignmentsTabUserControl)form.Controls.Find("HouseConsignmentsTabUserControl", true).First();
				var goodsItemsTabPage = houseConsignmentsTabUserControl.FindSingleOrDefault<ZTabPage>("GoodsItemsTabPage");
				var houseConsignmentTabControl = (ZTabControl)houseConsignmentsTabUserControl.Controls.Find("HouseConsignmentTabControl", true).FirstOrDefault();
				houseConsignmentTabControl.SelectTab(goodsItemsTabPage);

				var phase5GoodsItemsTabUserControl = (Phase5GoodsItemsTabUserControl)houseConsignmentsTabUserControl.Controls.Find("Phase5GoodsItemsTabUserControl", true).First();
				var goodsItemAdditionalDocumentsTabPage = phase5GoodsItemsTabUserControl.FindSingleOrDefault<ZTabPage>("GoodsItemAdditionalDocumentsTabPage");
				phase5GoodsItemsTabUserControl.GoodsItemTabControl.SelectTab(goodsItemAdditionalDocumentsTabPage);

				var goodsItemAdditionalDocumentsTabUserControl = (Phase5GoodsItemAdditionalDocumentsTabUserControl)phase5GoodsItemsTabUserControl.Controls.Find("GoodsItemAdditionalDocumentsTabUserControl", true).First();
				var additionalDocumentsSplitContainer = goodsItemAdditionalDocumentsTabUserControl.FindSingleOrDefault<KSplitContainer>("AdditionalDocumentsSplitContainer");
				var additionalDocumentsGridUserControl = additionalDocumentsSplitContainer.Panel1.FindSingle<DepartureGoodsItemAdditionalDocumentsGridUserControl>("GoodsItemAdditionalDocumentsGridUserControl");

				CombineAssertions(() =>
				{
					AssertType<DepartureGoodsItemAdditionalDocumentsGridUserControl>(additionalDocumentsGridUserControl);
					AssertEquals("Dock", DockStyle.Fill, additionalDocumentsGridUserControl.Dock);
					AssertEquals("BindingMember", ".", additionalDocumentsGridUserControl.GetBindingMember());
				});
			}
		}

		public void TestGoodsItemAdditionalDocumentsGridUserControl_Departure()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			userControl.SetDataBinding(nctsHeader, "");

			var additionalDocumentsSplitContainer = userControl.FindSingleOrDefault<KSplitContainer>("AdditionalDocumentsSplitContainer");
			var additionalDocumentsGridUserControl = additionalDocumentsSplitContainer.Panel1.FindSingle<DepartureGoodsItemAdditionalDocumentsGridUserControl>("GoodsItemAdditionalDocumentsGridUserControl");

			CombineAssertions(() =>
			{
				AssertType<DepartureGoodsItemAdditionalDocumentsGridUserControl>(additionalDocumentsGridUserControl);
				AssertEquals("Dock", DockStyle.Fill, additionalDocumentsGridUserControl.Dock);
				AssertEquals("BindingMember", ".", additionalDocumentsGridUserControl.GetBindingMember());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5GoodsItemAdditionalDocumentsTabUserControl();
		}
		Phase5GoodsItemAdditionalDocumentsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
