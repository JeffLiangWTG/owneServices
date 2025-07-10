using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS376;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class TS376Provider
	{
		public TS376Provider(Ts376 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Ts376 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration?.Mrn;

		public ZDateTime RejectionDate => (xmlObject.Declaration?.RejectionDate).ConvertToZDateTime();

		public ZString RejectionMotivationText => xmlObject.Declaration?.RejectionMotivationText;

		public IReadOnlyCollection<MFunctionalError01Provider> FunctionalErrors => functionalErrors ?? (functionalErrors = xmlObject.FunctionalError?.Select(x => new MFunctionalError01Provider(x)).ToArray() ?? Array.Empty<MFunctionalError01Provider>());
		IReadOnlyCollection<MFunctionalError01Provider> functionalErrors;
	}
}
