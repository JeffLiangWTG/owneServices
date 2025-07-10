using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class ComplianceWorkflowInitiationHelperTest : ComplianceRiskHelperTest
	{
		public void TestInitializeComplianceWorkflowPopupIfNeeded()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			var creditControlledBusinessObject = shipment as ICreditControlledBusinessObject;
			var complianceRiskStatusProvider = shipment as IComplianceItemRiskStatusProvider;
			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			{
				ComplianceWorkflowInitiationHelper.InitializeComplianceWorkflowPopupIfNeeded(creditControlledBusinessObject);
				AssertNull(complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded);
			}
			
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				ComplianceWorkflowInitiationHelper.InitializeComplianceWorkflowPopupIfNeeded(creditControlledBusinessObject);
				AssertNotNull(complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded);

				complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded = null;
				complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Clear;
				ComplianceWorkflowInitiationHelper.InitializeComplianceWorkflowPopupIfNeeded(creditControlledBusinessObject);
				AssertNull(complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded);
			}
		}

		public void TestInitializeComplianceWorkflowPopupIfNeeded_AssessmentInitiatedOrDeclined()
		{
			AssertAssessmentInitiatedOrDeclined(true);
			AssertAssessmentInitiatedOrDeclined(false);

			void AssertAssessmentInitiatedOrDeclined(bool isInitiated)
			{
				var shipment = CreateNewShipment;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";
				var creditControlledBusinessObject = shipment as ICreditControlledBusinessObject;
				var complianceRiskStatusProvider = shipment as IComplianceItemRiskStatusProvider;
				var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);

				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(true)))
				{
					ComplianceWorkflowInitiationHelper.InitializeComplianceWorkflowPopupIfNeeded(creditControlledBusinessObject);
					AssertNotNull(complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded);

					complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded = null;

					if (isInitiated)
					{
						ComplianceRiskStatusWorkflowInitializer.InitializeAssessmentWorkflow(complianceRiskStatus);
					}
					else
					{
						ComplianceRiskStatusWorkflowInitializer.DeclinedAssessmentWorkflow(complianceRiskStatus);
					}

					ComplianceWorkflowInitiationHelper.InitializeComplianceWorkflowPopupIfNeeded(creditControlledBusinessObject);
					AssertNull(complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded);
				}
			}
		}

		public void TestInitializeComplianceWorkflowPopupIfNeeded_CommodityScreeningFeatureEnableOrNot()
		{
			AssertInitializeComplianceWorkflowPopupIfNeeded(false);
			AssertInitializeComplianceWorkflowPopupIfNeeded(true);

			void AssertInitializeComplianceWorkflowPopupIfNeeded(bool enableCommodityScreening)
			{
				var shipment = CreateNewShipment;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";
				var creditControlledBusinessObject = shipment as ICreditControlledBusinessObject;
				var complianceRiskStatusProvider = shipment as IComplianceItemRiskStatusProvider;
				var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
				using (ComplianceRiskFeatureControlHelper.GetIngoreComplianceWiseCommodityScreeningEnableForTest())
				using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityScreeningMocksForTest(enableCommodityScreening))
				{
					ComplianceWorkflowInitiationHelper.InitializeComplianceWorkflowPopupIfNeeded(creditControlledBusinessObject);
					AssertEquals(enableCommodityScreening, complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded != null);
				}
			}
		}

		public void TestInitializeComplianceWorkflowPopupIfNeeded_ForCommodityRisk()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			var creditControlledBusinessObject = shipment as ICreditControlledBusinessObject;
			var complianceRiskStatusProvider = shipment as IComplianceItemRiskStatusProvider;
			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				AssertNull(GetWorkflowPopup(ComplianceRiskStatusCodeList.Codes.Clear));

				AssertNotNull(GetWorkflowPopup(ComplianceRiskStatusCodeList.Codes.Incomplete));

				AssertNotNull(GetWorkflowPopup(ComplianceRiskStatusCodeList.Codes.PotentialRisk));
			}

			Func<DocumentDeliveryResultForComplianceWorkflow> GetWorkflowPopup(ZString commodityRisk)
			{
				complianceRiskStatus.COR_CommodityRisk = commodityRisk;
				complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded = null;
				ComplianceWorkflowInitiationHelper.InitializeComplianceWorkflowPopupIfNeeded(creditControlledBusinessObject);
				return complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded;
			}
		}

		public void TestCommodityRiskStatusSetNotAssessed_InitializeComplianceWorkflowPopupNotShown()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			var creditControlledBusinessObject = shipment as ICreditControlledBusinessObject;
			var complianceRiskStatusProvider = shipment as IComplianceItemRiskStatusProvider;
			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);

			using (OrganisationsDataRegistry.Instance.AllowComplianceCommodityRiskAssessment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
								ComplianceWiseRegistryHelper.SetValue(true)))
			{
				ComplianceWorkflowInitiationHelper.InitializeComplianceWorkflowPopupIfNeeded(creditControlledBusinessObject);
				AssertNotNull(complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded);

				complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.NotAssessed;
				complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded = null;
				ComplianceWorkflowInitiationHelper.InitializeComplianceWorkflowPopupIfNeeded(creditControlledBusinessObject);
				AssertNull(complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded);
			}
		}

		public void TestInitializeComplianceWorkflowPopupIfNeeded_WhenDeclineComplianceAssessmentSecurityRightsIsFalse()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			var creditControlledBusinessObject = shipment as ICreditControlledBusinessObject;
			var complianceRiskStatusProvider = shipment as IComplianceItemRiskStatusProvider;
			var plugIn = new ComplianceRiskPlugInBusinessObject((IBusiness)shipment);
			var complianceRiskStatus = plugIn.ComplianceRiskStatus;

			var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";
			commodity.CCD_RN_NKOrigin = "AU";

			complianceRiskStatus.SupportedCountriesCheckResponseModel = new SupportedCountriesCheckResponseModel
			{
				CommodityLevel = new CommodityLevelModel
				{
					Export = new[] { "AU" },
					Import = new[] { "AU" },
					OriginOfGoods = new[] { "AU" }
				}
			};

			var securityCore = CreateNewSecurityCore;
			securityCore.ShipmentsComplianceAllowComplianceAssessment.IsAllowed = false;
			securityCore.ShipmentsComplianceDeclineComplianceAssessment.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (var form = new FormForTest(shipment as BusinessObject))
			{
				ComplianceWorkflowInitiationHelper.InitializeComplianceWorkflowPopupIfNeeded(creditControlledBusinessObject);
				AssertNotNull(complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded);

				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				var result = complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded();
				AssertEquals(DocumentDeliveryResultForComplianceWorkflow.StopDocumentDelivery, result);
				AssertEquals("ComplianceRiskTabPage", form.TabPageName);

				form.TabPageName = string.Empty;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				result = complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded();
				AssertEquals(DocumentDeliveryResultForComplianceWorkflow.StopDocumentDelivery, result);
				AssertEquals(string.Empty, form.TabPageName);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				result = complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded();
				AssertEquals(DocumentDeliveryResultForComplianceWorkflow.StopDocumentDelivery, result);
				AssertEquals(string.Empty, form.TabPageName);

				var dbConnection = ((CargoWise.Data.IDbConnected)Factory).Connection;
				try
				{
					dbConnection.BeginTransaction();
					result = complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded();
					AssertEquals("Continue Document Delivery When IsInTransaction", DocumentDeliveryResultForComplianceWorkflow.ContinueDocumentDelivery, result);
					AssertEquals(string.Empty, form.TabPageName);
				}
				finally
				{
					dbConnection.RollbackTransaction();
				}
			}
		}

		public void TestInitializeComplianceWorkflowPopupIfNeeded_WhenDeclineComplianceAssessmentSecurityRightsIsTrue()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			var creditControlledBusinessObject = shipment as ICreditControlledBusinessObject;
			var complianceRiskStatusProvider = shipment as IComplianceItemRiskStatusProvider;
			var plugIn = new ComplianceRiskPlugInBusinessObject((IBusiness)shipment);
			var complianceRiskStatus = plugIn.ComplianceRiskStatus;

			var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";
			commodity.CCD_RN_NKOrigin = "AU";

			complianceRiskStatus.SupportedCountriesCheckResponseModel = new SupportedCountriesCheckResponseModel
			{
				CommodityLevel = new CommodityLevelModel
				{
					Export = new[] { "AU" },
					Import = new[] { "AU" },
					OriginOfGoods = new[] { "AU" }
				}
			};

			var securityCore = CreateNewSecurityCore;
			securityCore.ShipmentsComplianceDeclineComplianceAssessment.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				ComplianceWorkflowInitiationHelper.InitializeComplianceWorkflowPopupIfNeeded(creditControlledBusinessObject);
				AssertNotNull(complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded);

				using (var form = new FormForTest(shipment as BusinessObject))
				{
					form.Show();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;

					CombineAssertions("Security: Decline Compliance Assessment should not show error message", () =>
					{
						var result = complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded();
						AssertEquals(DocumentDeliveryResultForComplianceWorkflow.StopDocumentDelivery, result);
						AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					});
				}
			}
		}

		public void TestInitializeComplianceWorkflowPopupIfNeeded_WhenShouldNotDoAssessmentByBorderWise()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			var creditControlledBusinessObject = shipment as ICreditControlledBusinessObject;
			var complianceRiskStatusProvider = shipment as IComplianceItemRiskStatusProvider;
			var plugIn = new ComplianceRiskPlugInBusinessObject((IBusiness)shipment);
			var complianceRiskStatus = plugIn.ComplianceRiskStatus;

			var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";
			commodity.CCD_RN_NKOrigin = "AU";

			complianceRiskStatus.SupportedCountriesCheckResponseModel = new SupportedCountriesCheckResponseModel
			{
				CommodityLevel = new CommodityLevelModel
				{
					Export = new[] { "CN" },
					Import = new[] { "CN" },
					OriginOfGoods = new[] { "CN" }
				}
			};

			var securityCore = CreateNewSecurityCore;
			securityCore.ShipmentsComplianceDeclineComplianceAssessment.IsAllowed = true;
			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				ComplianceWorkflowInitiationHelper.InitializeComplianceWorkflowPopupIfNeeded(creditControlledBusinessObject);
				AssertNotNull(complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded);

				using (var form = new FormForTest(shipment as BusinessObject))
				{
					form.Show();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;

					CombineAssertions("Security: Decline Compliance Assessment should not show error message", () =>
					{
						var result = complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded();
						AssertEquals(DocumentDeliveryResultForComplianceWorkflow.ContinueDocumentDelivery, result);
						AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					});
				}
			}
		}

		public void TestComplianceWorkflowInitiationHelper_MultipleRestrictedDocuments()
		{
			TestComplianceWorkflowInitiationHelper_CommodityRiskUpdated(true, DocumentDeliveryResultForComplianceWorkflow.StopDocumentDelivery, "Compliance Assessment initiated, so we want to stop document delivery");
			TestComplianceWorkflowInitiationHelper_CommodityRiskUpdated(false, DocumentDeliveryResultForComplianceWorkflow.ContinueDocumentDelivery, "Compliance Assessment declined, so we want to continue document delivery");

			void TestComplianceWorkflowInitiationHelper_CommodityRiskUpdated(bool isInitiated, DocumentDeliveryResultForComplianceWorkflow documentDeliveryResultAfterUpdate, string documentDeliveryMessage)
			{
				var shipment = CreateNewShipment;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "SGSIN";
				var creditControlledBusinessObject = shipment as ICreditControlledBusinessObject;
				var complianceRiskStatusProvider = shipment as IComplianceItemRiskStatusProvider;
				var plugIn = new ComplianceRiskPlugInBusinessObject((IBusiness)shipment);
				var complianceRiskStatus = plugIn.ComplianceRiskStatus;

				var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
				commodity.CCD_HarmonizedCode = "123456";
				commodity.CCD_RN_NKOrigin = "AU";

				complianceRiskStatus.SupportedCountriesCheckResponseModel = new SupportedCountriesCheckResponseModel
				{
					CommodityLevel = new CommodityLevelModel
					{
						Export = new[] { "AU" },
						Import = new[] { "AU" },
						OriginOfGoods = new[] { "AU" }
					}
				};

				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(true)))
				using (var form = new FormForTest(shipment as BusinessObject))
				{
					ComplianceWorkflowInitiationHelper.InitializeComplianceWorkflowPopupIfNeeded(creditControlledBusinessObject);
					AssertNotNull(complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded);

					form.Show();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
					var result = complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded();

					if (isInitiated)
					{
						ComplianceRiskStatusWorkflowInitializer.InitializeAssessmentWorkflow(complianceRiskStatus);
					}
					else
					{
						ComplianceRiskStatusWorkflowInitializer.DeclinedAssessmentWorkflow(complianceRiskStatus);
					}

					AssertEquals("Should not proceed to document delivery after compliance workflow popup", DocumentDeliveryResultForComplianceWorkflow.StopDocumentDelivery, result);

					result = complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded();
					AssertEquals(isInitiated, complianceRiskStatus.IsAssessmentInitialized);
					AssertEquals(!isInitiated, complianceRiskStatus.IsAssessmentDeclined);
					AssertEquals(documentDeliveryMessage, documentDeliveryResultAfterUpdate, result);
				}
			}
		}

		public void TestInitializeComplianceWorkflowPopupIfNeeded_StopDocumentDeliveryWhenInitiated()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";

			var creditControlledBusinessObject = shipment as ICreditControlledBusinessObject;
			var complianceRiskStatusProvider = shipment as IComplianceItemRiskStatusProvider;
			var plugIn = new ComplianceRiskPlugInBusinessObject((IBusiness)shipment);
			var complianceRiskStatus = plugIn.ComplianceRiskStatus;

			var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";
			commodity.CCD_RN_NKOrigin = "AU";

			complianceRiskStatus.SupportedCountriesCheckResponseModel = new SupportedCountriesCheckResponseModel
			{
				CommodityLevel = new CommodityLevelModel
				{
					Export = new[] { "AU" },
					Import = new[] { "AU" },
					OriginOfGoods = new[] { "AU" }
				}
			};

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (var form = new FormForTest(shipment as BusinessObject))
			{
				ComplianceWorkflowInitiationHelper.InitializeComplianceWorkflowPopupIfNeeded(creditControlledBusinessObject);

				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;

				var result = complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded();
				AssertEquals(DocumentDeliveryResultForComplianceWorkflow.StopDocumentDelivery, result);
			}
		}

		public void TestInitializeComplianceWorkflowPopupIfNeeded_DocumentDeliveryStateWhenDeclined()
		{
			TestInitializeComplianceWorkflowPopupIfNeeded_CorrectDocumentDeliveryState(false, false, DocumentDeliveryResultForComplianceWorkflow.StopDocumentDelivery);
			TestInitializeComplianceWorkflowPopupIfNeeded_CorrectDocumentDeliveryState(false, true, DocumentDeliveryResultForComplianceWorkflow.ContinueDocumentDelivery);
			TestInitializeComplianceWorkflowPopupIfNeeded_CorrectDocumentDeliveryState(true, false, DocumentDeliveryResultForComplianceWorkflow.ContinueDocumentDelivery);

			void TestInitializeComplianceWorkflowPopupIfNeeded_CorrectDocumentDeliveryState(bool allowAssessment, bool declineAssessment, DocumentDeliveryResultForComplianceWorkflow expectedDeliveryState)
			{
				var shipment = CreateNewShipment;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "SGSIN";

				var creditControlledBusinessObject = shipment as ICreditControlledBusinessObject;
				var complianceRiskStatusProvider = shipment as IComplianceItemRiskStatusProvider;
				var plugIn = new ComplianceRiskPlugInBusinessObject((IBusiness)shipment);
				var complianceRiskStatus = plugIn.ComplianceRiskStatus;

				var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
				commodity.CCD_HarmonizedCode = "123456";
				commodity.CCD_RN_NKOrigin = "AU";

				complianceRiskStatus.SupportedCountriesCheckResponseModel = new SupportedCountriesCheckResponseModel
				{
					CommodityLevel = new CommodityLevelModel
					{
						Export = new[] { "AU" },
						Import = new[] { "AU" },
						OriginOfGoods = new[] { "AU" }
					}
				};

				var securityCore = CreateNewSecurityCore;
				securityCore.ShipmentsComplianceAllowComplianceAssessment.IsAllowed = allowAssessment;
				securityCore.ShipmentsComplianceDeclineComplianceAssessment.IsAllowed = declineAssessment;

				using (Env.SetTemporarySecurityInstanceForTest(securityCore))
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(true)))
				using (var form = new FormForTest(shipment as BusinessObject))
				{
					ComplianceWorkflowInitiationHelper.InitializeComplianceWorkflowPopupIfNeeded(creditControlledBusinessObject);

					form.Show();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;

					var result = complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded();
					AssertEquals(expectedDeliveryState, result);
				}
			}
		}

		public class FormForTest : ZForm, ISupportSwitchTabPage
		{
			public FormForTest(BusinessObject businessObject) : base(businessObject)
			{
			}

			public string TabPageName { get; set; }

			public void SwitchTabPage(string tabPageName)
			{
				TabPageName = tabPageName;
			}
		}

		IDisposable setAllowComplianceCommodityRiskAssessmentToTrue;
		protected override void SetUp()
		{
			base.SetUp();
			setAllowComplianceCommodityRiskAssessmentToTrue = OrganisationsDataRegistry.Instance.AllowComplianceCommodityRiskAssessment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			setAllowComplianceCommodityRiskAssessmentToTrue.Dispose();
		}
	}
}
