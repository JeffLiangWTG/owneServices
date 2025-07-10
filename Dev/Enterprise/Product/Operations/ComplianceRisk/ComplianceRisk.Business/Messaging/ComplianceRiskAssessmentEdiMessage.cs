using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.ComplianceRisk.Business.Messaging;

public class ComplianceRiskAssessmentEdiMessage : EDIMessage
{
	public ComplianceRiskAssessmentEdiMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();

		EM_ApplicationCode = ApplicationCodeList.Codes.CPWRequestMessage;
		EM_MessageType = ComplianceRiskAssessmentMessagePublisher.MaterialChange;
		EM_MessageSubType = EDIMessageSubTypeList.Codes.ComplianceRiskAssessment;
	}

	protected override string GetMessageReferenceNumber()
	{
		return $"CRA{GetHashCode()}";
	}

	protected override IEnumerable<Type> GetAdditionalRegisteredLinkedObjectTypes()
	{
		return new List<Type>(base.GetAdditionalRegisteredLinkedObjectTypes())
		{
			Type.GetType("Enterprise.Freight.Forwarding.Business.ForwardingShipment, Enterprise.Freight.Forwarding.Business"),
			Type.GetType("Enterprise.Freight.Forwarding.Business.ForwardingConsol, Enterprise.Freight.Forwarding.Business"),
			Type.GetType("Enterprise.Freight.QuotedBookings.Business.ViewQuotedBooking, Enterprise.Freight.QuotedBookings.Business"),
		};
	}
}
