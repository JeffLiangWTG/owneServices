using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.GUI.Testing
{
	sealed class ConsignmentsGridUserControlMenuProviderTest : TestCaseWithFactory
	{
		public void TestAdditionalConsignmentsGridMenuItemsForDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = Factory.New<CusExitHeader>();
			header.CXH_ParentID = declaration.PK;
			header.CXH_ParentTableCode = declaration.TablePrefix;
			AssertAdditionalConsignmentsGridMenuItems(header);
		}

		public void TestAdditionalConsignmentsGridMenuItemsForShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var header = Factory.New<CusExitHeader>();
			header.Parent = shipment;
			AssertAdditionalConsignmentsGridMenuItems(header);
		}

		static void AssertAdditionalConsignmentsGridMenuItems(CusExitHeader header)
		{
			var gridProviderMock = new Mock<IConsignmentsGridUserControlProvider>();
			gridProviderMock.Setup(x => x.ExitHeader).Returns(header);
			var gridProvider = gridProviderMock.Object;

			IConsignmentsGridUserControlMenuProvider provider = new ConsignmentsGridUserControlMenuProvider(gridProvider);
			var additionalConsignmentsGridMenuItems = provider.AdditionalConsignmentsGridMenuItems;
			CombineAssertions(() =>
			{
				AssertEquals(2, additionalConsignmentsGridMenuItems.Count);
				AssertEquals("&Import Goods Items/Entry Lines", additionalConsignmentsGridMenuItems[1].Caption);
			});
		}

		[RequiresSTA]
		public void TestImportGoodsItemsMenuItemForDeclaration_Click()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = Factory.New<CusExitHeader>();
			header.CXH_ParentID = declaration.PK;
			header.CXH_ParentTableCode = declaration.TablePrefix;
			AssertImportGoodsItemsMenuItemClick(header);
		}

		public void TestImportGoodsItemsMenuItemForShipment_Click()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var header = Factory.New<CusExitHeader>();
			header.Parent = shipment;
			AssertImportGoodsItemsMenuItemClick(header);
		}

		void AssertImportGoodsItemsMenuItemClick(CusExitHeader header)
		{
			var consignments = header.CusExitConsignments;
			var consignment = consignments.AddNew();
			using (var form = new ZForm(header))
			using (var grid = new ConsignmentsGridUserControl())
			{
				form.Controls.Add(grid);
				form.Show();

				grid.SetDataBinding(consignments, "");
				var gridProviderMock = new Mock<IConsignmentsGridUserControlProvider>();
				gridProviderMock.Setup(x => x.ExitHeader).Returns(header);
				gridProviderMock.Setup(x => x.UserControl).Returns(grid);
				var gridProvider = gridProviderMock.Object;
				IConsignmentsGridUserControlMenuProvider provider = new ConsignmentsGridUserControlMenuProvider(gridProvider);
				var additionalConsignmentsGridMenuItems = provider.AdditionalConsignmentsGridMenuItems.ToArray();
				var consignmentsGrid = grid.ConsignmentsGrid;
				var contextMenu = consignmentsGrid.ContextMenu;
				contextMenu.MenuItems.AddRange(additionalConsignmentsGridMenuItems);

				var createExitReportMenuItem = contextMenu.MenuItems.Cast<ZMenuItem>().FirstOrDefault(x => x.Caption == "&Import Goods Items/Entry Lines");
				UnitTestUserNotification.Instance.ClearMessages();
				createExitReportMenuItem.PerformClick();
				AssertEquals("Please select a single consignment.", UnitTestUserNotification.Instance.LastMessage.Text);

				consignmentsGrid.Select(0);
				UnitTestUserNotification.Instance.ClearMessages();
				createExitReportMenuItem.PerformClick();
				AssertEquals("Empty MRN is invalid for this operation.", UnitTestUserNotification.Instance.LastMessage.Text);

				consignment.CXC_MovementReference = "MRN001";
				UnitTestUserNotification.Instance.ClearMessages();
				createExitReportMenuItem.PerformClick();
				AssertEquals("Import completed.", UnitTestUserNotification.Instance.LastMessage.Text);

				var euDeclaration = Factory.New<Integration.Customs.DE.IJobDeclaration>();
				form.SetDataBinding(euDeclaration, "");
				UnitTestUserNotification.Instance.ClearMessages();
				createExitReportMenuItem.PerformClick();
				AssertEquals("Please select a single consignment.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
