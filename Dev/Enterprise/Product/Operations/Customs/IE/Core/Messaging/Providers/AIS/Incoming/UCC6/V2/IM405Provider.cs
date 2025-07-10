using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM405;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM405Provider : IIM405Provider
	{
		public IM405Provider(Im405 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im405 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZDateTime AmendmentRejectionDate => (xmlObject.ImportOperation?.AmendmentRejectionDate).ConvertToZDateTime();

		public ZString AmendmentRejectionMotivationText => xmlObject.ImportOperation?.AmendmentRejectionMotivationText;

		public ZString Remarks => xmlObject.ImportOperation?.Remarks;

		public IReadOnlyCollection<MFunctionalError01Provider> FunctionalErrors => functionalErrors ?? (functionalErrors = xmlObject.FunctionalError?.Select(x => new MFunctionalError01Provider(x)).ToArray() ?? Array.Empty<MFunctionalError01Provider>());
		IReadOnlyCollection<MFunctionalError01Provider> functionalErrors;
	}
}
