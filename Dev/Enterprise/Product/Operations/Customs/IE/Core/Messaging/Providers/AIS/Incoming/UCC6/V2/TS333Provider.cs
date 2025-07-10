using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS333;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class TS333Provider
	{
		public TS333Provider(Ts333 xmlObject)
		{
			this.xmlObject = xmlObject;
		}

		readonly Ts333 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration?.Mrn;

		public ZDateTime RejectionDate => (xmlObject.Declaration?.RejectionDate).ConvertToZDateTime();

		public ZString RejectionReason => xmlObject.Declaration?.RejectionReason;

		public IReadOnlyCollection<MFunctionalError01Provider> FunctionalErrors
			=> functionalErrors
				?? (functionalErrors = xmlObject.FunctionalError
					?.Select(x => new MFunctionalError01Provider(x)).ToArray() ?? Array.Empty<MFunctionalError01Provider>());
		IReadOnlyCollection<MFunctionalError01Provider> functionalErrors;
	}
}
