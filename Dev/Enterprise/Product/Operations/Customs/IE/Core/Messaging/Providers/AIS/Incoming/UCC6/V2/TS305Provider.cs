using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS305;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class TS305Provider
	{
		public TS305Provider(Ts305 xmlObject)
		{
			this.xmlObject = xmlObject;
		}

		readonly Ts305 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration?.Mrn;

		public ZDateTime RejectionDate => (xmlObject.Declaration?.AmendmentRejectionDate).ConvertToZDateTime();

		public ZString RejectionReason => xmlObject.Declaration?.AmendmentRejectionReason;

		public IReadOnlyCollection<MFunctionalError01Provider> FunctionalErrors
			=> functionalErrors
			   ?? (functionalErrors = xmlObject.FunctionalError
				   ?.Select(x => new MFunctionalError01Provider(x)).ToArray() ?? Array.Empty<MFunctionalError01Provider>());
		IReadOnlyCollection<MFunctionalError01Provider> functionalErrors;
	}
}
