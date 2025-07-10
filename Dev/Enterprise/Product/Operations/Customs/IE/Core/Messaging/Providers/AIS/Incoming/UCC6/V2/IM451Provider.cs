using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM451;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Messaging.AIS;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM451Provider : IIM451Provider
	{
		public IM451Provider(Im451 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im451 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZString LocalReferenceNumber => xmlObject.ImportOperation?.Lrn;

		public ZString DeclarationType => xmlObject.ImportOperation?.DeclarationType;

		public ZString AdditionalDeclarationType => xmlObject.ImportOperation?.AdditionalDeclarationType;

		public ZDate DecisionDate => (xmlObject.ImportOperation?.DecisionDate).ConvertToZDate();

		public ZString DecisionReason => xmlObject.ImportOperation?.DecisionReason;

		public ZString PreferredPaymentMethod => xmlObject.ImportOperation?.PreferredPaymentMethod;

		public ZString Remarks => xmlObject.ImportOperation?.Remarks;

		public IReadOnlyCollection<ItemControlResultsProvider> ControlResults => controlResults ?? (controlResults = xmlObject.ControlResults?.Select(x => new ItemControlResultsProvider(x)).ToArray() ?? Array.Empty<ItemControlResultsProvider>());
		IReadOnlyCollection<ItemControlResultsProvider> controlResults;
	}
}
