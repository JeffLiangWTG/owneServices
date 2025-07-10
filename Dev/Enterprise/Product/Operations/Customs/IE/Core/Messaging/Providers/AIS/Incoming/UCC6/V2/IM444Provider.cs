using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM444;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Messaging.AIS;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM444Provider
	{
		public IM444Provider(Im444 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im444 xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ImportOperation?.Lrn;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZString Code => xmlObject.ControlResult?.Code;

		public ZDate Date => (xmlObject.ControlResult?.Date).ConvertToZDate();

		public ZString Remarks => xmlObject.ControlResult?.Remarks;

		public ZString PendingSamplingResults => xmlObject.ControlResult?.PendingSamplingResults;

		public IReadOnlyCollection<ItemControlResultsProvider> ControlResults => controlResults ?? (controlResults = xmlObject.ControlResults?.Select(x => new ItemControlResultsProvider(x)).ToArray() ?? Array.Empty<ItemControlResultsProvider>());
		IReadOnlyCollection<ItemControlResultsProvider> controlResults;
	}
}
