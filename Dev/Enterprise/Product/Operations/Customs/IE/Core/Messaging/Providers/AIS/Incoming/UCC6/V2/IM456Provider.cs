using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM456;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM456Provider
	{
		public IM456Provider(Im456 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im456 xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ImportOperation?.Lrn;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZString CustomsRegistrationNumber => xmlObject.ImportOperation?.CustomsRegistrationNumber;

		public ZString BusinessRejectionType => xmlObject.ImportOperation?.BusinessRejectionType;

		public ZDateTime RejectionDateAndTime => (xmlObject.ImportOperation?.RejectionDateAndTime).ConvertToZDateTime();

		public ZString RejectionCode => xmlObject.ImportOperation?.RejectionCode;

		public ZString RejectionReason => xmlObject.ImportOperation?.RejectionReason;

		public IReadOnlyCollection<MFunctionalError01Provider> FunctionalErrors => functionalErrors ?? (functionalErrors = xmlObject.FunctionalError?.Select(x => new MFunctionalError01Provider(x)).ToArray() ?? Array.Empty<MFunctionalError01Provider>());
		IReadOnlyCollection<MFunctionalError01Provider> functionalErrors;
	}
}
