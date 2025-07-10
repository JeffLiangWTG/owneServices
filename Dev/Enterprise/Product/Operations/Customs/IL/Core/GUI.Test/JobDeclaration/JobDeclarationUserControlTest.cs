using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(JobDeclarationUserControl))]
	class JobDeclarationUserControlTest : BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
	{
		public void TestAddressFormatters()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				var address = Factory.New<OrgAddress>();
				AssertType<ILSupplierAddressFormatter>(userControl.SupplierOrganisationControl.OrgAddressFormatter.Invoke(Factory, address));
				AssertType<ILImporterAddressFormatter>(userControl.ImporterOrganisationControl.OrgAddressFormatter.Invoke(Factory, address));
			}
		}

		public void TestContainerModeUserControlVisible()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				var control = (ZDropEdit)userControl.Controls.Find("JE_ContainerModeBoundDropDownEdit", true).FirstOrDefault();
				AssertEquals(true, control.Visible);
			}
		}

		public void TestServiceUserControlVisible()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				var control = (ZCodeFindBox)userControl.Controls.Find("JE_RS_NKServiceLevelBoundFindBox", true).FirstOrDefault();
				AssertEquals(true, control.Visible);
			}
		}

		public void TestCargoTypeCodeUserControlVisible()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				var control = (ZDropEdit)userControl.Controls.Find("TransportMeansDropEdit", true).FirstOrDefault();
				AssertEquals(true, control.Visible);
			}
		}

		public void TestIssueDateTimeUserControlVisible()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				var control = (ZDateEdit)userControl.Controls.Find("JE_MasterBillIssuedDateDateEdit", true).FirstOrDefault();
				AssertEquals(true, control.Visible);
			}
		}

		public void TestGoodsLocationUserControlVisible()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				var control = (ZCodeFindBox)userControl.Controls.Find("LocationOfGoodsCodeFindBox", true).FirstOrDefault();
				AssertEquals(true, control.Visible);
			}
		}

		public void TestQuantityUserControlVisible()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				var control = (ZCalcDropEdit)userControl.Controls.Find("TotalNoOfPacksCalcDropEdit", true).FirstOrDefault();
				AssertEquals(true, control.Visible);
			}
		}

		public void TestCustomsOfficeUserControlVisible()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				var control = (ZCodeFindBox)userControl.Controls.Find("JE_CustomsOfficeFindBox", true).FirstOrDefault();
				AssertEquals(true, control.Visible);
			}
		}

		public void TestScreeningStatusUserControlVisible()
		{
			AssertScreeningStatusVisible(false, true);
			AssertScreeningStatusVisible(true, false);

			void AssertScreeningStatusVisible(bool enableCompliance, bool expectedVisible)
			{
				var declaration = Factory.New<JobDeclaration>();

				var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = true };
				var featureDataMock = new Mock<IFeatureData>();
				var featureControlMock = new Mock<IFeatureControlManager>();
				featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(enableCompliance);
				featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
				using (ObjectFactory.Substitute(featureControlMock.Object))
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					var screeningStatusDropEdit = form.Controls.Find("ScreeningStatusDropEdit", true)[0] as ZDropEdit;
					var screenButton = form.Controls.Find("ScreenButton", true)[0] as ZButton;
					AssertEquals(false, screenButton.Visible);
					AssertEquals(expectedVisible, screeningStatusDropEdit.Visible);
				}
			}
		}

		public void TestManifestUserControlVisible_WhenSeaRoaOnly()
		{
			AssertControlVisibilityInTransportMode(Core.Constants.TransportModes.Air, "JE_ManifestNumberTextBox", false);
			AssertControlVisibilityInTransportMode(Core.Constants.TransportModes.Sea, "JE_ManifestNumberTextBox", true);
			AssertControlVisibilityInTransportMode(Core.Constants.TransportModes.Road, "JE_ManifestNumberTextBox", true);
		}

		public void TestJE_MasterBillForSeaBoundTextBoxControlVisible_WhenSeaRoaOnly()
		{
			AssertControlVisibilityInTransportMode(Core.Constants.TransportModes.Air, "JE_MasterBillForSeaBoundTextBox", false);
			AssertControlVisibilityInTransportMode(Core.Constants.TransportModes.Sea, "JE_MasterBillForSeaBoundTextBox", true);
			AssertControlVisibilityInTransportMode(Core.Constants.TransportModes.Road, "JE_MasterBillForSeaBoundTextBox", true);
		}

		public void TestJE_MasterBillForAirBoundTextBoxControlVisible_WhenAirOnly()
		{
			AssertControlVisibilityInTransportMode(Core.Constants.TransportModes.Air, "JE_MasterBillForAirBoundTextBox", true);
			AssertControlVisibilityInTransportMode(Core.Constants.TransportModes.Sea, "JE_MasterBillForAirBoundTextBox", false);
			AssertControlVisibilityInTransportMode(Core.Constants.TransportModes.Road, "JE_MasterBillForAirBoundTextBox", false);
		}

		public void TestVesselUserControlVisible_WhenSeaRoaOnly()
		{
			AssertControlVisibilityInTransportMode(Core.Constants.TransportModes.Air, "VesselFindBox", false);
			AssertControlVisibilityInTransportMode(Core.Constants.TransportModes.Sea, "VesselFindBox", true);
			AssertControlVisibilityInTransportMode(Core.Constants.TransportModes.Road, "VesselFindBox", true);
		}

		public void TestOwnerReferenceUserControlNotVisible()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				var control = (ZDropEdit)userControl.Controls.Find("ScreeningStatusDropEdit", true).FirstOrDefault();
				AssertEquals(true, control.Visible);
			}
		}

		public void TestFolioUserControlNotVisible()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				var control = (ZDropEdit)userControl.Controls.Find("ScreeningStatusDropEdit", true).FirstOrDefault();
				AssertEquals(true, control.Visible);
			}
		}

		public void TestUnitsUserControlNotVisible()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				var control = (ZDropEdit)userControl.Controls.Find("ScreeningStatusDropEdit", true).FirstOrDefault();
				AssertEquals(true, control.Visible);
			}
		}

		public void TestNoPackagesUserControlNotVisible()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				var control = (ZDropEdit)userControl.Controls.Find("ScreeningStatusDropEdit", true).FirstOrDefault();
				AssertEquals(true, control.Visible);
			}
		}

		public void TestDeclarantOfficeAddressControlVisible()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				var tabPage = userControl.Controls.Find("OrganisationsTabPage", true).FirstOrDefault();
				AssertNotNull(tabPage);
				tabPage.Visible = true;

				var control = (ZAddressControl)userControl.Controls.Find("DeclarantOfficeAddressControl", true).FirstOrDefault();
				AssertEquals(true, control.Visible);
			}
		}

		public void TestRepresentativeAddressControlVisible()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				var tabPage = userControl.Controls.Find("OrganisationsTabPage", true).FirstOrDefault();
				AssertNotNull(tabPage);
				tabPage.Visible = true;

				var control = (ZAddressControl)userControl.Controls.Find("RepresentativeAddressControl", true).FirstOrDefault();
				AssertEquals(true, control.Visible);
			}
		}

		public void TestSellerAddressControlVisible()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				var tabPage = userControl.Controls.Find("OrganisationsTabPage", true).FirstOrDefault();
				AssertNotNull(tabPage);
				tabPage.Visible = true;

				var control = (ZAddressControl)userControl.Controls.Find("SellerAddressControl", true).FirstOrDefault();
				AssertEquals(true, control.Visible);
			}
		}

		public void TestLocationOfGoodsCodeFindBox()
		{
			using (var userControl = new JobDeclarationUserControl())
			{
				CombineAssertions(() =>
				{
					AssertType<ZCodeFindBox>("Type", userControl.LocationOfGoodsCodeFindBox);
					AssertEquals("BindTo", nameof(JobDeclaration.JE_LocationOfGoods), userControl.LocationOfGoodsCodeFindBox.BindTo);
					AssertEquals("ModuleID", ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList, userControl.LocationOfGoodsCodeFindBox.ModuleID);
				});
			}
		}

		void AssertControlVisibilityInTransportMode(ZString transportMode, string controlName, bool expectVisible)
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_TransportMode = transportMode;
			using var zForm = new ZForm(jobDeclaration);
			using var userControl = new JobDeclarationUserControl();
			userControl.JobDeclaration = jobDeclaration;
			zForm.Controls.Add(userControl);
			zForm.Show();

			var control = userControl.Controls.Find(controlName, true).FirstOrDefault();
			AssertNotNull(controlName, control);
			AssertEquals(expectVisible, control.Visible);
		}
	}
}
