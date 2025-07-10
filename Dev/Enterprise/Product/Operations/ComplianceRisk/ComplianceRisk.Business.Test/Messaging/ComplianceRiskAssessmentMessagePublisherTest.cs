using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.Business.Messaging;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceRiskAssessmentMessagePublisher))]
	public class ComplianceRiskAssessmentMessagePublisherTest : TestCaseWithFactory
	{
		public void TestPublishMaterialChange()
		{
			var job = Factory.New<ForwardingShipment>();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ComplianceRiskAssessmentMessagePublisher.Publish(job);
			}

			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CPWRequestMessage)
				.AddToFilter(EDIMessageSchema.EM_MessageType, ComplianceRiskAssessmentMessagePublisher.MaterialChange)
				.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.ComplianceRiskAssessment)
				.AddToFilter(EDIMessageSchema.EM_LinkTable, job.TableName)
				.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, job.PK)
				.AddToFilter(EDIMessageSchema.EM_SystemCreateUser, GlbStaff.CurrentUser.GS_Code);

			AssertEquals(1, Factory.Load<EDIMessage>(query).Length);
		}
	}
}

