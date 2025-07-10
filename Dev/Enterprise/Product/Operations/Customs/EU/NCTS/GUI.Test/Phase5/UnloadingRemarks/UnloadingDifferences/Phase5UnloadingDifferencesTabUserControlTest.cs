using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5UnloadingDifferencesTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSource()
		{
			AssertEquals("Phase5UnloadingDifferencesTabUserControl BindingSource DataSourceType", typeof(NctsHeader), userControl.BindingSource.DataSourceType);
		}

		[RequiresSTA]
		public void TestUnloadingDifferencesDeclaredValueDynamicLayoutPanel_IsBoundToArrivalMovementHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;
			var shipment = Factory.New<ForwardingShipment>();
			nctsHeader.BH_ParentID = shipment.PK;
			nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
			using (var form = new ZForm(nctsHeader))
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(nctsHeader, "");
				form.Show();
				var panel = userControl.UnloadingDifferencesDeclaredValueDynamicLayoutPanel;
				var dataMember = panel.BindingSource.DataMember;
				AssertEquals(nameof(nctsHeader.ArrivalMovementHeader), dataMember);
			}
		}

		public void TestUnloadingDifferencesUnloadedValueDynamicLayoutPanel_IsBoundToArrivalMovementHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;
			var shipment = Factory.New<ForwardingShipment>();
			nctsHeader.BH_ParentID = shipment.PK;
			nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
			using (var form = new ZForm(nctsHeader))
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(nctsHeader, "");
				form.Show();
				var panel = userControl.UnloadingDifferencesUnloadedValueDynamicLayoutPanel;
				var dataMember = panel.BindingSource.DataMember;
				AssertEquals(nameof(nctsHeader.ArrivalMovementHeader), dataMember);
			}
		}

		public void TestUnloadingDetailsDynamicLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			userControl.SetDataBinding(nctsHeader, "");
			var unloadingDetailsDynamicLayoutPanel = userControl.UnloadingDetailsDynamicLayoutPanel;
			CombineAssertions(() =>
			{
				AssertEquals("UnloadingDetailsDynamicLayoutPanel Dock", DockStyle.Fill, unloadingDetailsDynamicLayoutPanel.Dock);
				AssertEquals("Within UnloadingDetailsGroupBox", true, userControl.UnloadingDetailsGroupBox.Controls.Contains(unloadingDetailsDynamicLayoutPanel));

				DynamicLayoutPanelTest.AssertControlsOrder(unloadingDetailsDynamicLayoutPanel,
					nameof(UnloadingDetailsControlBag.UnloadingDateDateEdit),
					nameof(UnloadingDetailsControlBag.UnloadingConformCheckBox),
					nameof(UnloadingDetailsControlBag.StateOfSealsCheckBox),
					nameof(UnloadingDetailsControlBag.UnloadingCompletedCheckBox),
					nameof(UnloadingDetailsControlBag.UnloadingRemarksTextBox),
					nameof(UnloadingDetailsControlBag.OtherThingsToReportTextBox));
			});
		}

		public void TestUnloadingDetailsGroupBox()
		{
			AssertEquals("Caption", "Unloading Details", userControl.UnloadingDetailsGroupBox.CaptionResourceString.Caption);
		}

		[RequiresSTA]
		public void TestGuaranteeGroupBoxDynamicLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			userControl.SetDataBinding(nctsHeader, "");
			userControl.GuaranteeGroupBox.Visible = true;
			var guaranteeGroupBoxDynamicLayoutPanel = userControl.GuaranteeGroupBoxDynamicLayoutPanel;
			CombineAssertions(() =>
			{
				AssertEquals("GuaranteeGroupBoxDynamicLayoutPanel Dock", DockStyle.Fill, guaranteeGroupBoxDynamicLayoutPanel.Dock);
				AssertEquals("Binding", nameof(NctsHeader.ArrivalMovementHeader) + "." + nameof(NctsArrivalMovementHeader.SingleGuaranteeForArrival), guaranteeGroupBoxDynamicLayoutPanel.GetBindingMember());
				AssertEquals("Within GuaranteeGroupBox", true, userControl.GuaranteeGroupBox.Controls.Contains(guaranteeGroupBoxDynamicLayoutPanel));

				DynamicLayoutPanelTest.AssertControlsOrder(guaranteeGroupBoxDynamicLayoutPanel,
					nameof(GuaranteeGroupBoxControlBag.BondNumberCodeFindBox),
					nameof(GuaranteeGroupBoxControlBag.AmountCalcDropEdit),
					nameof(GuaranteeGroupBoxControlBag.OverrideCheckBox));
			});
		}

		public void TestGuaranteeGroupBox()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Guarantee", userControl.GuaranteeGroupBox.CaptionResourceString.Caption);
				AssertEquals("Visibility for EU", false, userControl.GuaranteeGroupBox.Visible);
			});
		}

		public void TestUnloadingDifferencesGroupBox()
		{
			AssertEquals("Caption", "Unloading Differences", userControl.UnloadingDifferencesGroupBox.CaptionResourceString.Caption);
		}

		public void TestUnloadingDifferencesDeclaredValueDynamicLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			userControl.SetDataBinding(nctsHeader, "");
			var unloadingDifferencesDeclaredValueDynamicLayoutPanel = userControl.UnloadingDifferencesDeclaredValueDynamicLayoutPanel;
			CombineAssertions(() =>
			{
				AssertEquals("Within MainSplitContainer Panel1", true, userControl.MainSplitContainer.Panel1.Controls.Contains(unloadingDifferencesDeclaredValueDynamicLayoutPanel));

				DynamicLayoutPanelTest.AssertControlsOrder(unloadingDifferencesDeclaredValueDynamicLayoutPanel,
					nameof(UnloadingDifferencesDetailsControlBag.DeclaredValueLabel),
					nameof(UnloadingDifferencesDetailsControlBag.TotalGrossMassDeclaredValueCalcEdit),
					nameof(UnloadingDifferencesDetailsControlBag.TotalPackagesDeclaredValueCalcEdit),
					nameof(UnloadingDifferencesDetailsControlBag.InlandTransportModeDropEdit));
			});
		}

		public void TestUnloadingDifferencesUnloadedValueDynamicLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			userControl.SetDataBinding(nctsHeader, "");
			var unloadingDifferencesUnloadedValueDynamicLayoutPanel = userControl.UnloadingDifferencesUnloadedValueDynamicLayoutPanel;
			CombineAssertions(() =>
			{
				AssertEquals("Within MainSplitContainer Panel1", true, userControl.MainSplitContainer.Panel1.Controls.Contains(unloadingDifferencesUnloadedValueDynamicLayoutPanel));

				DynamicLayoutPanelTest.AssertControlsOrder(unloadingDifferencesUnloadedValueDynamicLayoutPanel,
					nameof(UnloadingDifferencesDetailsControlBag.UnloadedValueLabel),
					nameof(UnloadingDifferencesDetailsControlBag.EffectiveGrossWeightUnloadedCalcEdit),
					nameof(UnloadingDifferencesDetailsControlBag.RecalculateTotalsButton),
					nameof(UnloadingDifferencesDetailsControlBag.TotalPackagesUnloadedValueCalcEdit),
					nameof(UnloadingDifferencesDetailsControlBag.SeparatorLabel));
			});
		}

		public void TestUnloadingDifferencesSupportingDocumentsGroupBox()
		{
			var supportingDocumentsGroupBox = userControl.UnloadingDifferencesSupportingDocumentsGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Supporting Documents", supportingDocumentsGroupBox.CaptionResourceString.Caption);
				AssertEquals("Within DocumentsMainSplitContainer.Panel1", true, userControl.DocumentsMainSplitContainer.Panel1.Contains(supportingDocumentsGroupBox));
				AssertEquals("Dock", DockStyle.Fill, supportingDocumentsGroupBox.Dock);
			});
		}

		public void TestUnloadingDifferencesSupportingDocumentsGridUserControl()
		{
			var supportingDocumentsGrid = userControl.UnloadingDifferencesSupportingDocumentsGridUserControl;
			CombineAssertions(() =>
			{
				AssertType<UnloadingDifferencesSupportingDocumentsGridUserControl>("Type", supportingDocumentsGrid);
				AssertEquals("Within UnloadingDifferencesSupportingDocumentsGroupBox", true, userControl.UnloadingDifferencesSupportingDocumentsGroupBox.Contains(supportingDocumentsGrid));
				AssertEquals("Binding", nameof(NctsHeader.ArrivalMovementHeader) + "." + nameof(NctsArrivalMovementHeader.SupportingDocuments), supportingDocumentsGrid.GetBindingMember());
				AssertEquals("Dock", DockStyle.Fill, supportingDocumentsGrid.Dock);
			});
		}

		public void TestUnloadingDifferencesAdditionalDocumentsGroupBox()
		{
			var additionalDocumentsGroupBox = userControl.UnloadingDifferencesAdditionalDocumentsGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Additional Documents", additionalDocumentsGroupBox.CaptionResourceString.Caption);
				AssertEquals("Within DocumentsSubSplitContainer.Panel1", true, userControl.DocumentsSubSplitContainer.Panel1.Contains(additionalDocumentsGroupBox));
				AssertEquals("Dock", DockStyle.Fill, additionalDocumentsGroupBox.Dock);
			});
		}

		public void TestUnloadingDifferencesAdditionalDocumentsGridUserControl()
		{
			var additionalDocumentsGrid = userControl.UnloadingDifferencesAdditionalDocumentsGridUserControl;
			CombineAssertions(() =>
			{
				AssertType<UnloadingDifferencesAdditionalDocumentsGridUserControl>("Type", additionalDocumentsGrid);
				AssertEquals("Within UnloadingDifferencesAdditionalDocumentsGroupBox", true, userControl.UnloadingDifferencesAdditionalDocumentsGroupBox.Contains(additionalDocumentsGrid));
				AssertEquals("Binding", nameof(NctsHeader.ArrivalMovementHeader) + "." + nameof(NctsArrivalMovementHeader.AdditionalDocuments), additionalDocumentsGrid.GetBindingMember());
				AssertEquals("Dock", DockStyle.Fill, additionalDocumentsGrid.Dock);
			});
		}

		public void TestUnloadingDifferencesPreviousDocumentsUserControl()
		{
			var unloadingDifferencesPreviousDocumentsUserControl = userControl.Phase5UnloadingDifferencesPreviousDocumentsUserControl;
			CombineAssertions(() =>
			{
				AssertEquals("Within DocumentsSubSplitContainer.Panel2", true, userControl.DocumentsSubSplitContainer.Panel2.Controls.Contains(unloadingDifferencesPreviousDocumentsUserControl));
				AssertEquals("Dock", DockStyle.Fill, unloadingDifferencesPreviousDocumentsUserControl.Dock);
			});
		}

		public void TestArrivalContainerAndSealsUserControl()
		{
			var containerAndSealsUserControl = userControl.ArrivalContainersAndSealsUserControl;
			CombineAssertions(() =>
			{
				AssertEquals("Within UnloadingDifferencesContainersEquipmentTabPage", true, userControl.UnloadingDifferencesContainersEquipmentTabPage.Controls.Contains(containerAndSealsUserControl));
				AssertEquals("Dock", DockStyle.Fill, containerAndSealsUserControl.Dock);
			});
		}

		public void TestUnloadingDifferencesDocumentsTabPage()
		{
			var unloadingDifferencesDocumentsTabPage = userControl.UnloadingDifferencesDocumentsTabPage;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Documents", unloadingDifferencesDocumentsTabPage.CaptionResourceString.Caption);
				AssertEquals("Within UnloadingDifferencesTabControl", true, userControl.UnloadingDifferencesTabControl.Controls.Contains(unloadingDifferencesDocumentsTabPage));
			});
		}

		public void TestUnloadingDifferencesContainersEquipmentTabPage()
		{
			var unloadingDifferencesContainersEquipmentTabPage = userControl.UnloadingDifferencesContainersEquipmentTabPage;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Containers/Equipment", unloadingDifferencesContainersEquipmentTabPage.CaptionResourceString.Caption);
				AssertEquals("Within UnloadingDifferencesContainersEquipmentTabPage", true, userControl.UnloadingDifferencesTabControl.Controls.Contains(unloadingDifferencesContainersEquipmentTabPage));
			});
		}

		public void TestUnloadingDifferencesTabControl()
		{
			var unloadingDifferencesTabControl = userControl.UnloadingDifferencesTabControl;
			CombineAssertions(() =>
			{
				AssertEquals("TabCount", 2, unloadingDifferencesTabControl.TabCount);
				AssertEquals("Within SubSplitContainer Panel2", true, userControl.SubSplitContainer.Panel2.Controls.Contains(unloadingDifferencesTabControl));
			});
		}

		public void TestArrivalTransportInfosGridUserControl()
		{
			AssertNull("Before binding", userControl.ArrivalTransportInfosGridUserControl);
		}

		public void TestArrivalTransportInfosGridUserControlAfterBinding()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			userControl.SetDataBinding(nctsHeader, string.Empty);

			CombineAssertions("After binding", () =>
			{
				AssertType<ArrivalTransportInfosGridUserControl>("Type", userControl.ArrivalTransportInfosGridUserControl);
				AssertEquals("BindingMember", "ArrivalMovementHeader.ArrivalTransportInfos", userControl.ArrivalTransportInfosGridUserControl.GetBindingMember());
				AssertEquals("Dock", DockStyle.Fill, userControl.ArrivalTransportInfosGridUserControl.Dock);
			});
		}

		public void TestArrivalTransportInfoGroupBox()
		{
			var arrivalTransportInfoGroupBox = userControl.ArrivalTransportInfoGroupBox;
			AssertEquals("Caption", "Transport Info", arrivalTransportInfoGroupBox.CaptionResourceString.Caption);
		}

		public void TestMainSplitContainer()
		{
			var mainSplitContainer = userControl.MainSplitContainer;
			AssertEquals("Within UnloadingDifferencesGroupBox", true, userControl.UnloadingDifferencesGroupBox.Controls.Contains(mainSplitContainer));
		}

		public void TestSubSplitContainer()
		{
			var subSplitContainer = userControl.SubSplitContainer;
			AssertEquals("Within MainSplitContainer Panel 2", true, userControl.MainSplitContainer.Panel2.Controls.Contains(subSplitContainer));
		}

		public void TestDocumentsMainSplitContainer()
		{
			var documentsMainSplitContainer = userControl.DocumentsMainSplitContainer;
			AssertEquals("Within UnloadingDifferencesDocumentsTabPage", true, userControl.UnloadingDifferencesDocumentsTabPage.Controls.Contains(documentsMainSplitContainer));
		}

		public void TestDocumentsSubSplitContainer()
		{
			var documentsSubSplitContainer = userControl.DocumentsSubSplitContainer;
			AssertEquals("Within DocumentsMainSplitContainer Panel 2", true, userControl.DocumentsMainSplitContainer.Panel2.Controls.Contains(documentsSubSplitContainer));
		}

		[RequiresSTA]
		public void TestShowWarningBoxWhenBM_StateOfSealsBooleanValueChanged()
		{
			const string message = "Not all seals have the state DEC." +
					"\r\nPress CANCEL if you want to check or change the seals or its state." +
					"\r\nIf you press OK, all seals with the state blanks, MIS or DAM will be set to DEC. The seals with state NEW remain as is so that they can be sent to customs.";

			var nctsHeader = Factory.New<Business.Testing.NctsHeaderForTest>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = "ULR";
			nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = false;
			nctsHeader.AllArrivalSealStateAreDECForTest = false;
			using (var form = new ZForm(nctsHeader))
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(nctsHeader, "");
				form.Show();
				nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = true;
				AssertEquals("When NotAllSealStateAreDEC, warningBox appear", message, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestShowWarningBoxWhenBM_NoChangesToReportValueChanged()
		{
			const string message = "Not all entries in the 'House consignment' tab have the value DEC." +
					"\r\nPress CANCEL if you want to check the unloaded state of the house consignments, goods items, packages and documents." +
					"\r\nIf you press OK, all entries with the state blanks, MIS or DIF will be set to DEC. The entries with state NEW will be removed.";

			var nctsHeader = Factory.New<Business.Testing.NctsHeaderForTest>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = "ULR";
			nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
			nctsHeader.ExistNonDECEntryCoreForTest = true;
			using (var form = new ZForm(nctsHeader))
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(nctsHeader, "");
				form.Show();
				nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
				AssertEquals("When ExistNonDECEntry, warningBox appear", message, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRecalculateTotalGrossWeightsDialogResult_OK()
		{
			var (movementHeader, bill1, bill2) = PrepareBOsForRecalculateTotalGrossWeightsDialogTest();
			using (var form = new ZForm())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(movementHeader.Header, string.Empty);
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				movementHeader.Factory.Save();
				CombineAssertions(() =>
				{
					AssertEquals("Dialog Result pops up during saving", "Unloaded Gross Weights have changed. Recalculate Total Gross Weights in Unloading Remarks and House Consignments?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("BM_GrossWeightUnloaded updated", 2m, movementHeader.BM_GrossWeightUnloaded);
					AssertEquals("Bill1.B0_GrossWeightUnloaded updated", 2m, bill1.B0_GrossWeightUnloaded);
					AssertEquals("Bill2.B0_GrossWeightUnloaded not updated", 0m, bill2.B0_GrossWeightUnloaded);
				});
			}
		}

		[RequiresSTA]
		public void TestRecalculateTotalGrossWeightsDialogResult_Cancel()
		{
			var (movementHeader, bill1, bill2) = PrepareBOsForRecalculateTotalGrossWeightsDialogTest();
			using (var form = new ZForm())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(movementHeader.Header, string.Empty);
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				movementHeader.Factory.Save();
				CombineAssertions(() =>
				{
					AssertEquals("Dialog Result pops up during saving", "Unloaded Gross Weights have changed. Recalculate Total Gross Weights in Unloading Remarks and House Consignments?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("BM_GrossWeightUnloaded not updated", 10m, movementHeader.BM_GrossWeightUnloaded);
					AssertEquals("Bill1.B0_GrossWeightUnloaded not updated", 1m, bill1.B0_GrossWeightUnloaded);
					AssertEquals("Bill2.B0_GrossWeightUnloaded not updated", 3m, bill2.B0_GrossWeightUnloaded);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5UnloadingDifferencesTabUserControl();
		}
		Phase5UnloadingDifferencesTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		(NctsArrivalMovementHeader, NctsBill, NctsBill) PrepareBOsForRecalculateTotalGrossWeightsDialogTest()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill1 = header.Bills.AddNew();
			bill1.B0_WeightUQ = "KG";
			bill1.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			var bill2 = header.Bills.AddNew();
			bill2.B0_WeightUQ = "KG";
			bill2.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;

			var arrivalMovement = header.ArrivalMovementHeader;
			arrivalMovement.BM_NoChangesToReport = false;

			var goodsItem1 = bill1.ArrivalGoodsItems.AddNew();
			goodsItem1.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			goodsItem1.BY_GrossWeight = 1;
			goodsItem1.BY_GrossWeightUnit = "KG";

			var goodsItem2 = bill1.ArrivalGoodsItems.AddNew();
			goodsItem2.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			goodsItem2.BY_GrossWeight = 1;
			goodsItem2.BY_GrossWeightUnit = "KG";

			arrivalMovement.BM_GrossWeightUnloaded = 10;
			bill1.B0_GrossWeightUnloaded = 1;
			bill2.B0_GrossWeightUnloaded = 3;

			return (arrivalMovement, bill1, bill2);
		}
	}
}
