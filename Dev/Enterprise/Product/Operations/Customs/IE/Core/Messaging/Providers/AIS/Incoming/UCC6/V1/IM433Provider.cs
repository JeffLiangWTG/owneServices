using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM433;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1
{
	public class IM433Provider : IIM933Provider
	{
		public IM433Provider(Im433 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im433 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration?.Mrn;

		public ZDateTime NotificationRejectionDate
		{
			get
			{
				new ZString(xmlObject.Declaration.RejectionDate).TryParseToDate(out var rejectionDate);
				return rejectionDate;
			}
		}

		public ZString NotificationRejectionReason => xmlObject.Declaration?.RejectionReason;

		public IReadOnlyCollection<FunctionalErrorTypeProvider> FunctionalErrors => functionalErrors ?? (functionalErrors = xmlObject.FunctionalError?.Select(x => new FunctionalErrorTypeProvider(x)).ToArray() ?? Array.Empty<FunctionalErrorTypeProvider>());
		IReadOnlyCollection<FunctionalErrorTypeProvider> functionalErrors;
	}
}
