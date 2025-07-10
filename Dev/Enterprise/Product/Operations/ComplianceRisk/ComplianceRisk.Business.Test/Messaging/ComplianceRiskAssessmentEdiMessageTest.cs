using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business.Messaging;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceRiskAssessmentEdiMessage))]
	public class ComplianceRiskAssessmentEdiMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var message = Factory.New<ComplianceRiskAssessmentEdiMessage>();
			Factory.Save();

			AssertEquals(ApplicationCodeList.Codes.CPWRequestMessage, message.EM_ApplicationCode);
			AssertEquals(ComplianceRiskAssessmentMessagePublisher.MaterialChange, message.EM_MessageType);
			AssertEquals(EDIMessageSubTypeList.Codes.ComplianceRiskAssessment, message.EM_MessageSubType);
			AssertEquals($"CRA{message.GetHashCode()}", message.EM_MessageNum);
		}

		public void TestGetAdditionalRegisteredLinkedObjectTypes()
		{
			var linkedObjectTypes = Factory.New<ComplianceRiskAssessmentEdiMessageForTest>().GetAdditionalRegisteredLinkedObjectTypes;
			AssertEquals(3, linkedObjectTypes.Count());
			AssertCollectionContains(typeof(ForwardingShipment), linkedObjectTypes);
			AssertCollectionContains(typeof(ForwardingConsol), linkedObjectTypes);
			AssertCollectionContains(typeof(ViewQuotedBooking), linkedObjectTypes);
		}
	}

	class ComplianceRiskAssessmentEdiMessageForTest : ComplianceRiskAssessmentEdiMessage
	{
		public ComplianceRiskAssessmentEdiMessageForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new IEnumerable<Type> GetAdditionalRegisteredLinkedObjectTypes => base.GetAdditionalRegisteredLinkedObjectTypes();
	}
}

