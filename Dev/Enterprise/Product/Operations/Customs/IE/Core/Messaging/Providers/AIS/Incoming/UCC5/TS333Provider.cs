using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS333;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class TS333Provider
	{
		public TS333Provider(Ts333 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Ts333 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration.Mrn;

		public ZString RejectionReason => xmlObject.Declaration.RejectionReason;

		public ZDateTime RejectionDate
		{
			get
			{
				new ZString(xmlObject.Declaration.RejectionDate).TryParseToDate(out var date);
				return date;
			}
		}

		public IReadOnlyCollection<FunctionalErrorTypeProvider> FunctionalErrors => functionalErrors ??= xmlObject.FunctionalError?.Select(x => new FunctionalErrorTypeProvider(x)).ToArray() ?? Array.Empty<FunctionalErrorTypeProvider>();
		IReadOnlyCollection<FunctionalErrorTypeProvider> functionalErrors;
	}
}
