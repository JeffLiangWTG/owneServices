using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM433;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class IM433Provider
	{
		public IM433Provider(Im433 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im433 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration.Mrn;

		public ZDateTime RejectionDate
		{
			get
			{
				new ZString(xmlObject.Declaration.RejectionDate).TryParseToDate(out var rejectionDate);
				return rejectionDate;
			}
		}

		public ZString RejectionReason => xmlObject.Declaration.RejectionReason;

		public IReadOnlyCollection<FunctionalErrorTypeProvider> FunctionalErrors => functionalErrors ??= xmlObject.FunctionalError?.Select(x => new FunctionalErrorTypeProvider(x)).ToArray() ?? Array.Empty<FunctionalErrorTypeProvider>();
		IReadOnlyCollection<FunctionalErrorTypeProvider> functionalErrors;
	}
}
