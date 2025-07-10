using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.JP.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(JobDeclarationUserControl))]
	class JobDeclarationUserControlTest : BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
	{
		public void TestImporterDocAddressControl()
		{
			using (var jobDeclarationForm = new TestJobDeclarationForm(Declaration))
			{
				jobDeclarationForm.Show();

				var control = jobDeclarationForm.DeclarationUserControl.ImporterDocAddressControl;
				AssertNotNull(control);

				Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Consignee", control.CaptionResourceString.Caption);

				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Importer", control.CaptionResourceString.Caption);
			}
		}

		public void TestSupplierDocAddressControl()
		{
			using (var jobDeclarationForm = new TestJobDeclarationForm(Declaration))
			{
				jobDeclarationForm.Show();

				var control = jobDeclarationForm.DeclarationUserControl.SupplierDocAddressControl;
				AssertNotNull(control);

				Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Exporter", control.CaptionResourceString.Caption);

				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Shipper", control.CaptionResourceString.Caption);
			}
		}

		public void TestShipmentTypeGroupBoxTabOrder()
		{
			using (var jobDeclarationForm = new TestJobDeclarationForm(Declaration))
			{
				jobDeclarationForm.Show();
				var decControl = jobDeclarationForm.DeclarationUserControl;
				var shipmentTypeGroupBox = decControl.ShipmentTypeGroupBox;
				AssertEquals("JE_MessageTypeBoundDropDownEdit should be next to ShipmentTypeGroupBox", decControl.JE_MessageTypeBoundDropDownEdit, shipmentTypeGroupBox.GetNextControl(shipmentTypeGroupBox, true));
				AssertEquals("JE_TransportModeBoundDropDownEdit should be next to SpecialDecTypeDropEdit", decControl.JE_TransportModeBoundDropDownEdit, shipmentTypeGroupBox.GetNextControl(decControl.JE_MessageSubTypeBoundDropDownEdit, true));
				AssertEquals("JE_ContainerModeBoundDropDownEdit should be next to CargoTypeDropEdit", decControl.JE_ContainerModeBoundDropDownEdit, shipmentTypeGroupBox.GetNextControl(decControl.JE_TransportModeBoundDropDownEdit, true));
				var serviceLevelBoundFindBox = shipmentTypeGroupBox.FindSingle<ZCodeFindBox>("JE_RS_NKServiceLevelBoundFindBox");
				AssertEquals("JE_RS_NKServiceLevelBoundFindBox should be next to JE_ContainerModeBoundDropDownEdit", serviceLevelBoundFindBox, shipmentTypeGroupBox.GetNextControl(decControl.JE_ContainerModeBoundDropDownEdit, true));
				var applicationCodeBoundDropEdit = shipmentTypeGroupBox.FindSingle<ZDropEdit>("JE_ApplicationCodeBoundDropEdit");
				AssertEquals("JE_ApplicationCodeBoundDropEdit should be next to JE_RS_NKServiceLevelBoundFindBox", applicationCodeBoundDropEdit, shipmentTypeGroupBox.GetNextControl(serviceLevelBoundFindBox, true));
			}
		}

		public void TestShipmentDetailsGroupBox()
		{
			using (var control = new JobDeclarationUserControl())
			{
				var customsOfficeDropEdit = control.ShipmentDetailsGroupBox.FindSingle<ZDropEdit>("CustomsOfficeDropEdit");
				var customsOfficeDepartmentDropEdit = control.ShipmentDetailsGroupBox.FindSingle<ZDropEdit>("CustomsOfficeDepartmentDropEdit");
				var receiptModeDropEdit = control.ShipmentDetailsGroupBox.FindSingle<ZDropEdit>("ReceiptModeDropEdit");
				var deliveryModeDropEdit = control.ShipmentDetailsGroupBox.FindSingle<ZDropEdit>("DeliveryModeDropEdit");
				var innerControls = new Control[] {
					control.HouseBillParcelPostTextEdit,
					control.OriginFindBox,
					control.JE_ExportDateBoundDateEdit2,
					receiptModeDropEdit,
					control.FinalDestinationFindBox,
					control.JE_DateOfArrivalBoundDateEdit2,
					deliveryModeDropEdit,
					control.GoodsDescriptionTextBox,
					control.OwnersReferenceTextBox,
					control.WeightzCalcDropEdit,
					control.VolumeCalcDropEdit,
					control.JE_ContainerCountCalcEdit,
					control.IncoTermDropEdit,
					control.ScreeningStatusDropEdit,
					customsOfficeDropEdit,
					customsOfficeDepartmentDropEdit
				};

				CombineAssertions(() =>
				{
					AssertContainsExactElementsInExactOrder("Controls should be in correct order Top-down then left-right", innerControls, innerControls.OrderBy(ctrl => ctrl.Left).OrderBy(ctrl => ctrl.Top));
					AssertContainsExactElementsInExactOrder("Controls should be in correct order of tab", innerControls, innerControls.OrderBy(ctrl => ctrl.TabIndex));
					AssertEquals("Controls are left aligned except for trailing ones", 2, innerControls.Select(ctrl => ctrl.Left).Distinct().Count());
				});
			}
		}

		public void TestVesselFindBox()
		{
			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_LloydsNumber = "0982432";
			refVessel.RV_Code = "VESSELNAME";
			refVessel.RV_RadioCallSign = "CALLSIGN";
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			using var form = new TestJobDeclarationForm(Declaration);
			form.Show();
			var control = form.DeclarationUserControl;
			var vesselFindBox = control.TransportDetailsGroupBox.FindSingle<ZCodeFindBox>("VesselFindBox");

			Declaration.JE_VesselName = "VESSELNAME";
			AssertEquals("CALLSIGN", vesselFindBox.DescriptionBox.Text);
		}

		public void TestFinalDestinationName()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using var form = new TestJobDeclarationForm(Declaration);
			form.Show();
			var control = form.DeclarationUserControl;
			var finalDestinationNameTextBox = control.ShipmentDetailsGroupBox.FindSingle<ZTextBox>("FinalDestinationNameTextBox");
			var finalDestinationFindBox = control.ShipmentDetailsGroupBox.FindSingle<ZCodeFindBox>("FinalDestinationFindBox");

			AssertEquals(finalDestinationNameTextBox.Visible, true);
			AssertEquals(finalDestinationFindBox.Visible, true);
			AssertEquals(finalDestinationFindBox.ShowDescriptionBox, false);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(finalDestinationNameTextBox.Visible, false);
			AssertEquals(finalDestinationFindBox.Visible, true);
			AssertEquals(finalDestinationFindBox.ShowDescriptionBox, true);
		}

		public void TestOwnerSectionCodeTextBox()
		{
			using var form = new TestJobDeclarationForm(Declaration);
			form.Show();
			var control = form.DeclarationUserControl;
			var ownerSectionCodeTextBox = control.ShipmentDetailsGroupBox.FindSingle<ZTextBox>("OwnerSectionCodeTextBox");

			Declaration.JE_OwnerSectionCode = "Owner Section Code";
			AssertEquals("OWNERSECTIONCODE", ownerSectionCodeTextBox.Text);
		}

		public void TestReceiptAndDeliveryModeDropEditVisibility()
		{
			using var form = new TestJobDeclarationForm(Declaration);
			form.Show();
			var control = form.DeclarationUserControl;
			var receiptModeDropEdit = control.ShipmentDetailsGroupBox.FindSingle<ZDropEdit>("ReceiptModeDropEdit");
			var deliveryModeDropEdit = control.ShipmentDetailsGroupBox.FindSingle<ZDropEdit>("DeliveryModeDropEdit");
			var radioCallSignCodeFindBox = control.TransportDetailsGroupBox.FindSingle<ZCodeFindBox>("RadioCallSignCodeFindBox");
			Assert("Receipt Mode should be hidden when the declaration is not Export and Sea", !receiptModeDropEdit.Visible);
			Assert("Delivery Mode should be hidden when the declaration is not Export and Sea", !receiptModeDropEdit.Visible);
			Assert("Radio Call Sign should be hidden when the declaration is not Sea", !radioCallSignCodeFindBox.Visible);
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert("Receipt Mode should not be hidden when the declaration is Export and Sea", receiptModeDropEdit.Visible);
			Assert("Delivery Mode should not be hidden when the declaration is Export and Sea", receiptModeDropEdit.Visible);
			Assert("Radio Call Sign should not be hidden when the declaration is Sea", radioCallSignCodeFindBox.Visible);
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		public void TestArrivalAtLoadingDateEdit()
		{
			using var form = new TestJobDeclarationForm(Declaration);
			form.Show();
			var control = form.DeclarationUserControl;
			var arrivalAtLoadingDateEdit = control.TransportDetailsGroupBox.FindSingle<ZDateEdit>("ArrivalAtLoadingDateEdit");
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(false, arrivalAtLoadingDateEdit.Visible);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(true, arrivalAtLoadingDateEdit.Visible);
		}

		public void TestJE_ExportDateBoundDateEditLocation()
		{
			using var form = new TestJobDeclarationForm(Declaration);
			form.Show();
			var control = form.DeclarationUserControl;
			var jE_ExportDateBoundDateEdit = control.TransportDetailsGroupBox.FindSingle<ZDateEdit>("JE_ExportDateBoundDateEdit");
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 88, true), jE_ExportDateBoundDateEdit.Location);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 114, true), jE_ExportDateBoundDateEdit.Location);
		}

		class TestJobDeclarationForm : JobDeclarationForm
		{
			public TestJobDeclarationForm(JobDeclaration declaration)
				: base(declaration)
			{
			}

			public JobDeclarationUserControl DeclarationUserControl => (JobDeclarationUserControl)CustomsBrokerageUserControl.DeclarationUserControlForTesting;
		}
	}
}
