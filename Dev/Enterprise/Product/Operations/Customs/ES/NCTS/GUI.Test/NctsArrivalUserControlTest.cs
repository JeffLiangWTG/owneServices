using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	class NctsArrivalUserControlTest : TestCaseWithFactory
	{
		public void TestAgreedLocationOfGoodsCodeCodeTextBox()
		{
			using (var form = new NctsMovementForm(nctsHeader))
			{
				form.Controls.Add(new NctsArrivalUserControl());
				form.Show();

				var agreedLocationOfGoods = (ZTextBox)form.Controls.Find("AgreedLocationOfGoodsCodeCodeTextBox", true).FirstOrDefault();
				AssertEquals(false, agreedLocationOfGoods.Visible);
			}
		}

		public void TestAgreedLocationOfGoodsCodeDropEdit()
		{
			using (var form = new NctsMovementForm(nctsHeader))
			{
				form.Controls.Add(new NctsArrivalUserControl());
				form.Show();

				var agreedLocationOfGoods = (ZDropEdit)form.Controls.Find("AgreedLocationOfGoodsCodeDropEdit", true).FirstOrDefault();
				AssertEquals(true, agreedLocationOfGoods.Visible);
			}
		}

		[RequiresSTA]
		public void TestCombinedMessageCheckBoxVisibility()
		{
			using (var form = new NctsMovementForm(nctsHeader))
			using (var control = new NctsArrivalUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var combinedMessage = (ZCheckBox)control.Controls.Find("CombinedMessageCheckBox", true).FirstOrDefault();
				AssertEquals(true, combinedMessage.Visible);
			}
		}

		[RequiresSTA]
		public void TestUnloadingRemarksTabPageVisibility_NctsMovementForm()
		{
			using (var form = new NctsMovementForm(nctsHeader))
			using (var control = new NctsArrivalUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				nctsHeader.CombinedMessage = ZBool.False;
				var unloadingTab = (ZTabPage)form.Controls.Find("UnloadingRemarksTabPage", true).FirstOrDefault();

				CombineAssertions(() =>
				{
					AssertNull(unloadingTab);

					nctsHeader.CombinedMessage = ZBool.True;
					unloadingTab = (ZTabPage)form.Controls.Find("UnloadingRemarksTabPage", true).FirstOrDefault();
					AssertEquals(true, unloadingTab.TabVisible);
				});
			}
		}

		[RequiresSTA]
		public void TestUnloadingRemarksTabPageVisibility_ShipmentForm()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "ESMAD";
			shipment.JS_RL_NKDestination = "GBLON";

			nctsHeader.BH_ParentID = shipment.PK;
			nctsHeader.BH_ParentTableCode = shipment.TablePrefix;

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);

			using (ObjectFactory.Substitute(mockSettings.Object))
			using (var form = new ShipmentForm(shipment))
			{
				var control = (NctsUserControlForPlugin)form.PlugIns.GetPlugIn(ControllerIDs.Customs.EU.NctsMovementController).UserControl;
				control.SetDataBinding(nctsHeader, "");
				form.Controls.Add(control);
				form.Show();

				nctsHeader.CombinedMessage = ZBool.False;
				var unloadingTab = (ZTabPage)form.Controls.Find("UnloadingRemarksTabPage", true).FirstOrDefault();

				CombineAssertions(() =>
				{
					AssertNull(unloadingTab);

					nctsHeader.CombinedMessage = ZBool.True;
					unloadingTab = (ZTabPage)form.Controls.Find("UnloadingRemarksTabPage", true).FirstOrDefault();
					AssertEquals(true, unloadingTab.TabVisible);
				});
			}
		}

		[RequiresSTA]
		public void TestShowParentUnloadingTabParentForm_NctsHeader()
		{
			CombineAssertions(() =>
			{
				using (var form = new NctsMovementForm(nctsHeader))
				using (var control = new NctsArrivalUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					var unloadingTab = (ZTabPage)form.Controls.Find("UnloadingRemarksTabPage", true).FirstOrDefault();
					AssertNull("[PRE-CONDITION] UnloadingRemarks tab just after opening the form", unloadingTab);
					AssertEquals("[PRE-CONDITION] CombinedMessage default value", false, nctsHeader.CombinedMessage);

					nctsHeader.CombinedMessage = false;
					unloadingTab = (ZTabPage)form.Controls.Find("UnloadingRemarksTabPage", true).FirstOrDefault();
					AssertNull("UnloadingRemarks tab after setting CombinedMessage to the same value", unloadingTab);

					nctsHeader.CombinedMessage = true;
					unloadingTab = (ZTabPage)form.Controls.Find("UnloadingRemarksTabPage", true).FirstOrDefault();
					AssertEquals("UnloadingRemarks tab is visible after setting CombinedMessage to true", true, unloadingTab.TabVisible);
				}
			});
		}

		[RequiresSTA]
		public void TestShowParentUnloadingTabParentForm_ForwardingShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "ESMAD";
			shipment.JS_RL_NKDestination = "GBLON";

			nctsHeader.BH_ParentID = shipment.PK;
			nctsHeader.BH_ParentTableCode = shipment.TablePrefix;

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);

			using (ObjectFactory.Substitute(mockSettings.Object))
			using (var form = new ShipmentForm(shipment))
			{
				var control = (NctsUserControlForPlugin)form.PlugIns.GetPlugIn(ControllerIDs.Customs.EU.NctsMovementController).UserControl;
				control.SetDataBinding(nctsHeader, "");
				form.Controls.Add(control);
				form.Show();

				var nctsTab = (ZTabPage)form.Controls.Find("NCTSTabPage", true).FirstOrDefault();
				var tabControl = (ZTabControl)form.Controls.Find("MainTabControl", true).FirstOrDefault();
				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				CombineAssertions(() =>
				{
					AssertNoExceptionThrown("When Parent is Shipment form", () => tabControl.SelectedTab = nctsTab);

					var unloadingTab = (ZTabPage)form.Controls.Find("UnloadingRemarksTabPage", true).FirstOrDefault();
					AssertNull("[PRE-CONDITION] UnloadingRemarks tab just after opening the form", unloadingTab);
					AssertEquals("[PRE-CONDITION] CombinedMessage default value", false, nctsHeader.CombinedMessage);

					nctsHeader.CombinedMessage = true;
					unloadingTab = (ZTabPage)form.Controls.Find("UnloadingRemarksTabPage", true).FirstOrDefault();
					AssertEquals("UnloadingRemarks tab is visible after setting CombinedMessage to true", true, unloadingTab.TabVisible);

					nctsHeader.CombinedMessage = false;
					unloadingTab = (ZTabPage)form.Controls.Find("UnloadingRemarksTabPage", true).FirstOrDefault();
					AssertNull("UnloadingRemarks tab after setting CombinedMessage to the same value", unloadingTab);
				});
			}
		}

		public void TestVisibilityTIRDataArrival()
		{
			using (var form = new NctsMovementForm(nctsHeader))
			using (var control = new NctsArrivalUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var tirPartialUnloadingCheckBox = (ZCheckBox)form.Controls.Find("TIRPartialUnloadingCheckBox", true).FirstOrDefault();
					var tirCarnetPageIntEdit = (ZIntEdit)form.Controls.Find("TIRCarnetPageIntEdit", true).FirstOrDefault();
					AssertEquals("TIRPartialUnloading is invisible initially.", false, tirPartialUnloadingCheckBox.Visible);
					AssertEquals("TIRCarnetPage is invisible initially.", false, tirCarnetPageIntEdit.Visible);

					nctsHeader.ESNctsHeader.CEN_TIRArrival = true;
					AssertEquals("TIRPartialUnloading is visible when TIR arrival is checked.", true, tirPartialUnloadingCheckBox.Visible);
					AssertEquals("TIRCarnetPage is visible when TIR arrival is checked.", true, tirCarnetPageIntEdit.Visible);
				});
			}
		}

		public void TestCertificateDropEdit_Added()
		{
			using (var form = new NctsMovementForm(nctsHeader))
			using (var control = new NctsArrivalUserControl())
			{
				AssertNotNull(control.FindSingle<ZDropEdit>("CertificateDropEdit"));
			}
		}

		[RequiresSTA]
		public void TestCertificateAndBrokerEditable()
		{
			using (var form = new NctsMovementForm(nctsHeader))
			using (var control = new NctsArrivalUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var arrivalTab = (ZTabPage)form.Controls.Find("ArrivalNotificationTabPage", true).FirstOrDefault();
				var certificateDropEdit = (ZDropEdit)arrivalTab.Controls.Find("CertificateDropEdit", true).FirstOrDefault();
				var brokerFindBox = (ZCodeFindBox)arrivalTab.Controls.Find("BrokerFindBox", true).FirstOrDefault();
				var transportAtDepartureTextBox = (ZTextBox)arrivalTab.Controls.Find("TransportAtDepartureTextBox", true).FirstOrDefault();
				CombineAssertions(() =>
				{
					nctsHeader.EffectiveMessageStatus = ZString.Empty;
					AssertEquals("CertificateDropEdit is not readonly when nctsHeader is not sent (EffectiveMessageStatus is empty)", false, certificateDropEdit.ReadOnly);
					AssertEquals("BrokerFindBox is not readonly when nctsHeader is not sent (EffectiveMessageStatus is empty)", false, brokerFindBox.ReadOnly);
					AssertEquals("TransportAtDepartureTextBox is not readonly when nctsHeader is not sent (EffectiveMessageStatus is empty)", false, transportAtDepartureTextBox.ReadOnly);

					nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationSent;
					form.Refresh();
					AssertEquals("CertificateDropEdit is not readonly when nctsHeader is sent (EffectiveMessageStatus is MAS)", false, certificateDropEdit.ReadOnly);
					AssertEquals("BrokerFindBox is not readonly when nctsHeader is sent (EffectiveMessageStatus is MAS)", false, brokerFindBox.ReadOnly);
					AssertEquals("TransportAtDepartureTextBox is readonly when nctsHeader is sent (EffectiveMessageStatus is MAS)", true, transportAtDepartureTextBox.ReadOnly);
				});
			}
		}

		public void TestOverrideFreightDefaultsLocation()
		{
			using (var control = new NctsArrivalUserControl())
			{
				var overrideFreightDefaultsCheckBox = (ZCheckBox)control.Controls.Find("OverrideFreightDefaults", true).FirstOrDefault();
				CombineAssertions(() =>
				{
					AssertEquals("OverrideFreightDefaults Location for ES", ControlDpiScalingHelper.ScaleToCurrentDpiX(357), overrideFreightDefaultsCheckBox.Location.X);
					AssertEquals("OverrideFreightDefaults Location for ES", 0, overrideFreightDefaultsCheckBox.Location.Y);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		}
		NctsHeader nctsHeader;
	}
}
