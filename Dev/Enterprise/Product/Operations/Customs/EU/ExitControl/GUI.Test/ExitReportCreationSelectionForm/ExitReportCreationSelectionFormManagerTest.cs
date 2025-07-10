using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class ExitReportCreationSelectionFormManagerTest : TestCaseWithFactory
	{
		public void TestShowExitReportCreationSelectionForm()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			var consignment = exitHeader.CusExitConsignments.AddNew();
			UnitTestUserNotification.Instance.ClearMessages();
			ExitReportCreationSelectionFormManager.ShowExitReportCreationSelectionForm(consignment, null);
			AssertEquals("A consignment should have at least one item.", UnitTestUserNotification.Instance.LastMessage.Text);

			var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
			UnitTestUserNotification.Instance.ClearMessages();
			ExitReportCreationSelectionFormManager.ShowExitReportCreationSelectionForm(consignment, null);
			AssertEquals("All items should have at least one package.", UnitTestUserNotification.Instance.LastMessage.Text);

			consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			UnitTestUserNotification.Instance.ClearMessages();
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
			{
				var dialog = (ExitReportCreationSelectionForm)obj;
				var exitConsignment = dialog.BusinessEntity;
				foreach (var item in exitConsignment.CusExitConsignmentItems)
				{
					item.CCI_Calc_ShouldReportItem = true;
					item.CCI_Calc_ReportGrossMass = 50m;
					item.CCI_Calc_ReportNetMass = 30m;
					foreach (var pivot in item.CusExitConsignmentPackagePivots)
					{
						var package = pivot.Package;
						package.CXP_Calc_ShouldReportItem = true;
						package.CXP_Calc_ReportQuantity = 2;
					}
				}
			});
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ExitReportCreationSelectionFormManager.ShowExitReportCreationSelectionForm(consignment, null);
			AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(1, exitHeader.CusExitReports.Count);
		}

		public void TestShowExitReportCreationSelectionForm_ShipmentIsNotNull()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				CreateUNLOCOsForTest();
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "DE123";

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_RL_NKDischargePort = "DE123";
				consol.Shipments.Add(shipment);
				consol.Transports.RemoveAndDeleteAll();
				var transport = consol.Transports.AddNew("DE123", "US123");
				transport.JW_TransportMode = "AIR";

				var exitHeader = Factory.New<CusExitHeader>();
				exitHeader.Parent = shipment;
				var consignment = exitHeader.CusExitConsignments.AddNew();
				var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
				consignmentItem.CusExitConsignmentPackagePivots.AddNew();

				UnitTestUserNotification.Instance.ClearMessages();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					var dialog = (ExitReportCreationSelectionForm)obj;
					var exitConsignment = dialog.BusinessEntity;
					foreach (var item in exitConsignment.CusExitConsignmentItems)
					{
						item.CCI_Calc_ShouldReportItem = true;
						item.CCI_Calc_ReportGrossMass = 50m;
						item.CCI_Calc_ReportNetMass = 30m;
						foreach (var pivot in item.CusExitConsignmentPackagePivots)
						{
							var package = pivot.Package;
							package.CXP_Calc_ShouldReportItem = true;
							package.CXP_Calc_ReportQuantity = 2;
						}
					}
				});
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ExitReportCreationSelectionFormManager.ShowExitReportCreationSelectionForm(consignment, null);
				AssertEquals("Should call report.DefaultDataFromShipment()", "AIR", exitHeader.CusExitReports.Single().CER_TransportMode);
			}
		}

		void CreateUNLOCOsForTest()
		{
			var helper = new MasterFilesTestHelper(Factory);
			var germany = RefCountry.LoadFromCountryCode(Factory, "DE");
			var unitedStates = RefCountry.LoadFromCountryCode(Factory, "US");
			helper.CreateUnlocoIfNotExists("DE123", germany);
			helper.CreateUnlocoIfNotExists("US123", unitedStates);
			Factory.Save();
		}
	}
}
