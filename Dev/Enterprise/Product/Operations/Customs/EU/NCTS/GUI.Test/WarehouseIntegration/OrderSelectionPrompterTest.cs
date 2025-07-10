using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class OrderSelectionPrompterTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestPrompt()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var bill = header.Bills.AddNew();

			var selectionHeaderMock = new Mock<NctsOrderInventorySelectionHeader>(bill)
			{
				CallBase = true,
			};

			var prompter = new OrderSelectionPrompter(selectionHeaderMock.Object);
			prompter.Prompt();

			var lastForm = ZFormModaliser.LastFormShownDialogForTest;
			AssertEquals(typeof(EmbeddedModulePopup),
				lastForm.GetType());
			AssertEquals(ModuleIDs.WhsOrder, ((EmbeddedModulePopup)lastForm).CurrentModule.ModuleID);

			selectionHeaderMock.Verify(e => e.GetCollectionForWhsOrderSelection(It.IsAny<BusinessObjectFactory>()), Times.Once);
		}

		[RequiresSTA]
		public void TestShowsImportInventoriesResultToUser()
		{
			var helper = new WhsDataTestHelper(Factory);
			var (whsOrder, _) = Enterprise.Customs.EU.Business.Testing.BondedWarehousingHelperTest.CreateOrderWithPick(helper, Factory, createMultiplePickLines: false);
			Factory.Save();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var bill = header.Bills.AddNew();

			var selectionHeaderMock = new Mock<NctsOrderInventorySelectionHeader>(bill)
			{
				CallBase = true,
			};

			selectionHeaderMock.Protected().Setup("UpdateParentData").Callback(() => selectionHeaderMock.Object.ImportInventoriesResult = "Test");
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
			{
				((EmbeddedModulePopup)dialog).EmbeddedModulePopupOKButtonStrategy?.HandleFindBoxOKButton(new BusinessObject[] { (BusinessObject)whsOrder });
			});
			var prompter = new OrderSelectionPrompter(selectionHeaderMock.Object);
			prompter.Prompt();

			AssertEquals("Test", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
