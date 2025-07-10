using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM409;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM409Provider : IIM409Provider
	{
		public IM409Provider(Im409 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im409 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZBool InvalidationDecision => xmlObject.ImportOperation?.InvalidationDecision ?? false;

		public ZBool InvalidationInitiatedByCustoms => xmlObject.ImportOperation?.InvalidationInitiatedByCustoms ?? false;

		public ZString InvalidationJustification => xmlObject.ImportOperation?.InvalidationJustification;

		public ZDateTime DateOfInvalidationDecision => (xmlObject.ImportOperation?.DateOfInvalidationDecision).ConvertToZDateTime();

		public ZDateTime DateOfInvalidationRequest => (xmlObject.ImportOperation?.DateOfInvalidationRequest).ConvertToZDateTime();

		public ZDateTime DateOfInvalidation => (xmlObject.ImportOperation?.DateOfInvalidation).ConvertToZDateTime();

		public IReadOnlyCollection<MFunctionalError01Provider> FunctionalErrors => functionalErrors ?? (functionalErrors = xmlObject.FunctionalError?.Select(x => new MFunctionalError01Provider(x)).ToArray() ?? Array.Empty<MFunctionalError01Provider>());
		IReadOnlyCollection<MFunctionalError01Provider> functionalErrors;
	}
}
