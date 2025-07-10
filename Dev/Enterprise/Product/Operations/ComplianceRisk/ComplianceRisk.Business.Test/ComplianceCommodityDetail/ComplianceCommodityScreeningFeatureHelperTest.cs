using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Moq;
using static CargoWise.Definitions.LicenceFeatureCodeList.Codes;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class ComplianceCommodityScreeningFeatureHelperTest : TestCaseWithFactory
	{
		public void TestIsCommodityRiskAssessable_JobIsInternational()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_ParentID = shipment.PK;

			var complianceBizO = new ComplianceRiskBusinessObject(shipment, complianceRiskStatus);
			AssertEquals(true, complianceBizO.IsCommodityRiskAssessable());
		}

		public void TestIsCommodityRiskAssessable_JobIsDomestic()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBND";

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_ParentID = shipment.PK;

			var complianceBizO = new ComplianceRiskBusinessObject(shipment, complianceRiskStatus);
			AssertEquals(false, complianceBizO.IsCommodityRiskAssessable());
		}

		public void TestIsCommodityRiskAssessable_CommodityScreeningFeature()
		{
			AssertIsCommodityRiskAssessableWithCommodityScreeningFeature(false);
			AssertIsCommodityRiskAssessableWithCommodityScreeningFeature(true);

			void AssertIsCommodityRiskAssessableWithCommodityScreeningFeature(bool enableCommodityScreening)
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "NZAKL";

				var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
				complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
				complianceRiskStatus.COR_ParentID = shipment.PK;

				using (ComplianceRiskFeatureControlHelper.GetIngoreComplianceWiseCommodityScreeningEnableForTest())
				using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityScreeningMocksForTest(enableCommodityScreening))
				{
					var complianceBizO = new ComplianceRiskBusinessObject(shipment, complianceRiskStatus);
					AssertEquals(enableCommodityScreening, complianceBizO.IsCommodityRiskAssessable());
				}
			}
		}

		public void TestIsCommodityRiskAssessable_AssessmentBefore()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_ParentID = shipment.PK;
			var complianceBizO = new ComplianceRiskBusinessObject(shipment, complianceRiskStatus);

			using (ComplianceRiskFeatureControlHelper.GetIngoreComplianceWiseCommodityScreeningEnableForTest())
			using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityScreeningMocksForTest(false))
			{
				AssertEquals(false, complianceBizO.IsCommodityRiskAssessable());
				AssertEquals(false, complianceBizO.CommodityScreeningEnabled());

				complianceRiskStatus.InitializeAssessmentWorkflow();
				AssertEquals(true, complianceBizO.IsCommodityRiskAssessable());
				AssertEquals(true, complianceBizO.CommodityScreeningEnabled());
			}

			var consolBusinessObject = (BusinessObject)Factory.New<IForwardingConsol>();
			var consol = (ForwardingConsol)consolBusinessObject;
			consol.JK_RL_NKLoadPort = "AU2CO";
			consol.JK_RL_NKDischargePort = "US32M";
			var consolRiskStatus = Factory.New<ComplianceRiskStatus>();
			consolRiskStatus.COR_ParentID = consol.PK;
			consolRiskStatus.COR_ParentTableCode = consol.TablePrefix;
			var consolComplianceBizO = new ComplianceRiskBusinessObject(consol, consolRiskStatus);

			using (ComplianceRiskFeatureControlHelper.GetIngoreComplianceWiseCommodityScreeningEnableForTest())
			using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityScreeningMocksForTest(false))
			{
				AssertEquals(false, consolComplianceBizO.IsCommodityRiskAssessable());
				AssertEquals(false, consolComplianceBizO.CommodityScreeningEnabled());

				consol.Shipments.Add(shipment);
				AssertEquals(true, consolComplianceBizO.IsCommodityRiskAssessable());
				AssertEquals(true, consolComplianceBizO.CommodityScreeningEnabled());

				consol.JK_RL_NKDischargePort = "AUBNE";
				AssertEquals(false, consolComplianceBizO.IsCommodityRiskAssessable());
				AssertEquals(true, consolComplianceBizO.CommodityScreeningEnabled());
			}
		}

		public void TestIsCommodityRiskAssessableCommodityScreeningEnabledForDeclaration()
		{
			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "NZAKL";
			var consolRiskStatus = Factory.New<ComplianceRiskStatus>();
			consolRiskStatus.COR_ParentID = declaration.PK;
			consolRiskStatus.COR_ParentTableCode = "JE";

			var bizO = new ComplianceRiskBusinessObject(declaration, consolRiskStatus);
			using (ComplianceRiskFeatureControlHelper.GetIngoreComplianceWiseCommodityScreeningEnableForTest())
			{
				AssertIsCommodityRiskAssessableCommodityScreeningEnabled(true);
				AssertIsCommodityRiskAssessableCommodityScreeningEnabled(false);
			}

			void AssertIsCommodityRiskAssessableCommodityScreeningEnabled(bool enabled)
			{
				using (SetupComplianceWiseFeatureMocksForTest(new[] { (ComplianceWiseCommodityScreening, true), (ComplianceWiseCustomsDeclarationModule, true), (ComplianceWiseCustomsDeclarationManageRiskStatusOnCommercialInvoice, enabled) }))
				{
					AssertEquals(enabled, bizO.IsCommodityRiskAssessable());
					AssertEquals(enabled, bizO.CommodityScreeningEnabled());
				}
			}
		}

		public void TestComplianceRiskAssessmentFeatureEnabledForJob()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "NZAKL";

			using (ComplianceRiskFeatureControlHelper.GetIngoreComplianceWiseCommodityScreeningEnableForTest())
			{
				using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityScreeningMocksForTest(false))
				{
					AssertComplianceRiskAssessmentFeatureEnabledForJob(shipmentEnabled: false, declarationEnabled: false);
				}

				using (SetupComplianceWiseFeatureMocksForTest(new[] { (ComplianceWiseCommodityScreening, true), (ComplianceWiseCustomsDeclarationModule, true), (ComplianceWiseCustomsDeclarationManageRiskStatusOnCommercialInvoice, true) }))
				{
					AssertComplianceRiskAssessmentFeatureEnabledForJob(shipmentEnabled: true, declarationEnabled: true);
				}

				using (SetupComplianceWiseFeatureMocksForTest(new[] { (ComplianceWiseCommodityScreening, true), (ComplianceWiseCustomsDeclarationModule, true), (ComplianceWiseCustomsDeclarationManageRiskStatusOnCommercialInvoice, false) }))
				{
					AssertComplianceRiskAssessmentFeatureEnabledForJob(shipmentEnabled: true, declarationEnabled: false);
				}

				void AssertComplianceRiskAssessmentFeatureEnabledForJob(bool shipmentEnabled, bool declarationEnabled)
				{
					AssertEquals(shipmentEnabled, ComplianceCommodityScreeningFeatureHelper.ComplianceRiskAssessmentFeatureEnabledForJob(shipment as IBusiness));
					AssertEquals(shipmentEnabled, ComplianceCommodityScreeningFeatureHelper.ComplianceRiskAssessmentFeatureEnabledForJob(shipment as IComplianceCommodityRiskStatusProvider));

					AssertEquals(declarationEnabled, ComplianceCommodityScreeningFeatureHelper.ComplianceRiskAssessmentFeatureEnabledForJob(declaration));
					AssertEquals(declarationEnabled, ComplianceCommodityScreeningFeatureHelper.ComplianceRiskAssessmentFeatureEnabledForJob(declaration as IComplianceCommodityRiskStatusProvider));
				}
			}
		}

		static IDisposable SetupComplianceWiseFeatureMocksForTest(params (string Code, bool Enabled)[] featureInfo)
		{
			var complianceWiseFeatureRule1 = new ComplianceRiskFeatureControlRule { Enabled = true };
			var complianceWiseFeatureRule2 = new ComplianceRiskFeatureControlRule { Enabled = false };
			var featureControlMock = new Mock<IFeatureControlManager>();

			foreach (var feature in featureInfo)
			{
				var featureDataMock = new Mock<IFeatureData>();
				if (feature.Enabled)
				{
					featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule1)).Returns(true);
				}
				else
				{
					featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule2)).Returns(true);
				}

				featureControlMock.Setup(x => x.GetFeatureDataAsync(feature.Code, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
			}

			return ObjectFactory.Substitute(featureControlMock.Object);
		}
	}
}
