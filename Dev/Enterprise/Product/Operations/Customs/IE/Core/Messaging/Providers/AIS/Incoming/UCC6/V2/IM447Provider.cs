using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM447;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Messaging.AIS;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM447Provider
	{
		public IM447Provider(Im447 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im447 xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ImportOperation?.Lrn;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZString Code => xmlObject.ControlResult?.Code;

		public ZDate Date => (xmlObject.ControlResult?.Date).ConvertToZDate();

		public ZString Remarks => xmlObject.ControlResult?.Remarks;

		public ZString SupportingDocumentsProvided => xmlObject.ControlResult?.SupportingDocumentsProvided;

		public IReadOnlyCollection<ItemControlResultsProvider> ControlResults => controlResults ?? (controlResults = xmlObject.ControlResults?.Select(x => new ItemControlResultsProvider(x)).ToArray() ?? Array.Empty<ItemControlResultsProvider>());
		IReadOnlyCollection<ItemControlResultsProvider> controlResults;
	}
}
