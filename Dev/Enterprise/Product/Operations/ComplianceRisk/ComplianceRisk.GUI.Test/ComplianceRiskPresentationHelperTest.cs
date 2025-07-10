using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class ComplianceRiskPresentationHelperTest : TestCaseWithFactory
	{
		public void TestInitializeCompliancePotentialRiskMessageBanner()
		{
			AssertBannerStatus(true, false, false);
			AssertBannerStatus(false, true, false);
			AssertBannerStatus(true, true, true);

			void AssertBannerStatus(bool freightEnabled, bool messageEnabled, bool bannerExists)
			{
				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(freightEnabled)))
				using (OrganisationsDataRegistry.Instance.EnableComplianceWarningMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, messageEnabled))
				using (var form = new ZForm(Factory.NewWithValidTestData<ForwardingShipment>()))
				{
					ComplianceRiskPresentationHelper.AddComplianceRiskWarningMessageBannerIfNeeded(form);
					form.Show();
					Application.DoEvents();

					if (bannerExists)
					{
						var messageBanner = (ZLabel)form.Controls.Find("CompliancePotentialRiskMessageBanner", true).Single();
						AssertEquals("d493820e-1940-45bc-b04e-11354f8ad7e7", messageBanner.CaptionResourceString.Key);
						AssertEquals("Job Compliance status is not Clear. View the Compliance Risk tab.", messageBanner.CaptionResourceString.Caption);
					}
					else
					{
						var result = form.Controls.Find("CompliancePotentialRiskMessageBanner", true);
						AssertEquals(0, result.Length);
					}
				}
			}
		}

		public void TestUpdateComplianceRiskWarningMessageBannerAfterPluginLoadedAndStatusResynchronize()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUMEL";
			var complianceRisk = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRisk.COR_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			complianceRisk.COR_ParentID = shipment.PK;
			Factory.Save();

			var risk = Factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, shipment.PK));
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, risk.COR_OverallRisk);

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
			using (OrganisationsDataRegistry.Instance.EnableComplianceWarningMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new DummyFormForComplianceWarningMessageBanner(shipment))
			{
				ComplianceRiskPresentationHelper.AddComplianceRiskWarningMessageBannerIfNeeded(form);
				form.Show();
				Application.DoEvents();
				var messageBanner = (ZLabel)form.Controls.Find("CompliancePotentialRiskMessageBanner", true).Single();
				AssertEquals(false, messageBanner.Visible);
			}
		}

		public void TestVisibleOffCompliancePotentialRiskMessageBanner()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceRisk = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRisk.COR_ParentTableCode = shipment.TablePrefix;
			complianceRisk.COR_ParentID = shipment.PK;
			complianceRisk.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;

			Factory.Save();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableComplianceWarningMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(shipment))
			{
				ComplianceRiskPresentationHelper.AddComplianceRiskWarningMessageBannerIfNeeded(form);
				form.Show();
				Application.DoEvents();

				var messageBanner = (ZLabel)form.Controls.Find("CompliancePotentialRiskMessageBanner", true).Single();
				Assert("Compliance overall risk is not Potential Risk", !messageBanner.Visible);
			}
		}

		public void TestVisibleOnCompliancePotentialRiskMessageBanner()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceRisk = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRisk.COR_ParentTableCode = shipment.TablePrefix;
			complianceRisk.COR_ParentID = shipment.PK;
			complianceRisk.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

			Factory.Save();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableComplianceWarningMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(shipment))
			{
				ComplianceRiskPresentationHelper.AddComplianceRiskWarningMessageBannerIfNeeded(form);
				form.Show();
				Application.DoEvents();

				var messageBanner = (ZLabel)form.Controls.Find("CompliancePotentialRiskMessageBanner", true).Single();
				Assert("Compliance overall risk is equal to Potential Risk", messageBanner.Visible);
			}
		}

		public void TestCompliancePotentialRiskMessageBannerShownInCorrectLocation()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceRisk = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRisk.COR_ParentTableCode = shipment.TablePrefix;
			complianceRisk.COR_ParentID = shipment.PK;
			complianceRisk.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

			Factory.Save();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableComplianceWarningMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(shipment))
			{
				form.CaptionRenderingEnabled = true;
				ComplianceRiskPresentationHelper.AddComplianceRiskWarningMessageBannerIfNeeded(form);
				var messageBanner = (ZLabel)form.Controls.Find("CompliancePotentialRiskMessageBanner", true).Single();
				form.Show();
				Application.DoEvents();

				var bannerPosition = form.MainStatusBar.Width - messageBanner.Width;
				var expectedPosition = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(bannerPosition - 30, 7, isInStandardDpi: false);

				AssertEquals("The banner should be shown in correct location.", expectedPosition, messageBanner.Location);
			}
		}

		#region Implementation

		class DummyFormForComplianceWarningMessageBanner : ZForm
		{
			public DummyFormForComplianceWarningMessageBanner(object dataSource) : base(dataSource)
			{
			}

			public ZTemplateTabControl TabControl { get; } = new ZTemplateTabControl();

			protected override ZTabControl TopLevelTabControl => TabControl;

			protected override void InitialiseForm()
			{
				base.InitialiseForm();

				var tabPage1 = new ZTabPage();
				tabPage1.Name = "DummyFirstTab";
				TopLevelTabControl.TabPages.Add(tabPage1);

				PlugIns.Add(ControllerIDs.ComplianceRiskPlugin);
				Controls.Add(TabControl);
			}
		}

		#endregion
	}
}
