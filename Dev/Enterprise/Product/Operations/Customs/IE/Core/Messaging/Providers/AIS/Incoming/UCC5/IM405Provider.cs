using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM405;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class IM405Provider
	{
		public IM405Provider(Im405 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im405 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration?.Mrn;

		public ZDateTime AmendmentRejectionDate
		{
			get
			{
				var amendmentRejectionDate = ZDateTime.Empty;
				xmlObject.Declaration?.AmendmentRejectionDate.TryParseToDate(out amendmentRejectionDate);
				return amendmentRejectionDate;
			}
		}

		public ZString AmendmentRejectionMotivationText => xmlObject.Declaration?.AmendmentRejectionMotivationText;

		public ZString Remarks => xmlObject.Declaration?.Remarks;

		public IReadOnlyCollection<FunctionalErrorTypeProvider> FunctionalErrors => functionalErrors ??= xmlObject.FunctionalError?.Select(x => new FunctionalErrorTypeProvider(x)).ToArray() ?? Array.Empty<FunctionalErrorTypeProvider>();
		IReadOnlyCollection<FunctionalErrorTypeProvider> functionalErrors;
	}
}
