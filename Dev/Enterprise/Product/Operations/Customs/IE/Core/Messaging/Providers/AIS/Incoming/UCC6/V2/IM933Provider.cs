using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM933;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM933Provider : IIM933Provider
	{
		public IM933Provider(Im933 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im933 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZDateTime NotificationRejectionDate => (xmlObject.ImportOperation?.RejectionDate).ConvertToZDateTime();

		public ZString NotificationRejectionReason => xmlObject.ImportOperation?.RejectionReason;

		public IReadOnlyCollection<MFunctionalError01Provider> FunctionalErrors => functionalErrors ?? (functionalErrors = xmlObject.FunctionalError?.Select(x => new MFunctionalError01Provider(x)).ToArray() ?? Array.Empty<MFunctionalError01Provider>());
		IReadOnlyCollection<MFunctionalError01Provider> functionalErrors;
	}
}
