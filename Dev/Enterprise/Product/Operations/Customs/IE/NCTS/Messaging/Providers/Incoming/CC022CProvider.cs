using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC022C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC022CProvider
	{
		public CC022CProvider(Cc022CType xmlObject)
		{
			this.xmlObject = xmlObject;
			this.transitOperation = this.xmlObject.TransitOperation;
		}
		readonly Cc022CType xmlObject;
		readonly TransitOperationType09 transitOperation;

		public ZString MRN => transitOperation?.Mrn;

		public ZDateTime AmendmentNotificationDateAndTime => (transitOperation?.AmendmentNotificationDateAndTime).ConvertToZDateTime();

		public IReadOnlyCollection<CC022CFunctionalErrorProvider> FunctionalErrors => functionalErrorCached ?? (functionalErrorCached = xmlObject.FunctionalError?.Select(x => new CC022CFunctionalErrorProvider(x)).ToArray() ?? Array.Empty<CC022CFunctionalErrorProvider>());
		IReadOnlyCollection<CC022CFunctionalErrorProvider> functionalErrorCached;
	}
}
