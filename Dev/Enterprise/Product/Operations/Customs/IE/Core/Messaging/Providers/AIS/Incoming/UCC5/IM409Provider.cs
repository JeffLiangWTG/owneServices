using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM409;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class IM409Provider
	{
		public IM409Provider(Im409 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im409 xmlObject;

		public ZBool InvalidationDecision => xmlObject.Declaration.InvalidationDecision;

		public ZString MovementReferenceNumber => xmlObject.Declaration?.Mrn;

		public ZBool InvalidationInitiatedByCustoms => xmlObject.Declaration?.InvalidationInitiatedByCustoms ?? false;

		public ZString InvalidationJustification => xmlObject.Declaration?.InvalidationJustification;

		public ZDateTime DateOfInvalidationDecision
		{
			get
			{
				var amendmentRejectionDate = ZDateTime.Empty;
				xmlObject.Declaration?.DateOfInvalidationDecision.TryParseToDate(out amendmentRejectionDate);
				return amendmentRejectionDate;
			}
		}

		public ZDateTime DateOfInvalidationRequest
		{
			get
			{
				var amendmentRejectionDate = ZDateTime.Empty;
				xmlObject.Declaration?.DateOfInvalidationRequest.TryParseToDate(out amendmentRejectionDate);
				return amendmentRejectionDate;
			}
		}

		public ZDateTime DateOfInvalidation
		{
			get
			{
				var amendmentRejectionDate = ZDateTime.Empty;
				xmlObject.Declaration?.DateOfInvalidation.TryParseToDate(out amendmentRejectionDate);
				return amendmentRejectionDate;
			}
		}

		public IReadOnlyCollection<FunctionalErrorTypeProvider> FunctionalErrors => functionalErrors ??= xmlObject.FunctionalError?.Select(x => new FunctionalErrorTypeProvider(x)).ToArray() ?? Array.Empty<FunctionalErrorTypeProvider>();
		IReadOnlyCollection<FunctionalErrorTypeProvider> functionalErrors;
	}
}
