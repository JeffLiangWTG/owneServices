using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM409;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1
{
	public class IM409Provider : IIM409Provider
	{
		public IM409Provider(Im409 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im409 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration?.Mrn;

		public ZBool InvalidationDecision => xmlObject.Declaration?.InvalidationDecision ?? false;

		public ZBool InvalidationInitiatedByCustoms => xmlObject.Declaration?.InvalidationInitiatedByCustoms ?? false;

		public ZString InvalidationJustification => xmlObject.Declaration?.InvalidationJustification;

		public ZDateTime DateOfInvalidationDecision => DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(xmlObject.Declaration?.DateOfInvalidationDecision);

		public ZDateTime DateOfInvalidationRequest => DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(xmlObject.Declaration?.DateOfInvalidationRequest);

		public ZDateTime DateOfInvalidation => DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(xmlObject.Declaration?.DateOfInvalidation);

		public IReadOnlyCollection<FunctionalErrorTypeProvider> FunctionalErrors => functionalErrors ?? (functionalErrors = xmlObject.FunctionalError?.Select(x => new FunctionalErrorTypeProvider(x)).ToArray() ?? Array.Empty<FunctionalErrorTypeProvider>());
		IReadOnlyCollection<FunctionalErrorTypeProvider> functionalErrors;
	}
}
