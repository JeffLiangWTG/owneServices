using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM405;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1
{
	public class IM405Provider : IIM405Provider
	{
		public IM405Provider(Im405 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im405 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration?.Mrn;

		public ZDateTime AmendmentRejectionDate => DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(xmlObject.Declaration?.AmendmentRejectionDate);

		public ZString AmendmentRejectionMotivationText => xmlObject.Declaration?.AmendmentRejectionMotivationText;

		public ZString Remarks => xmlObject.Declaration?.Remarks;

		public IReadOnlyCollection<FunctionalErrorTypeProvider> FunctionalErrors => functionalErrors ?? (functionalErrors = xmlObject.FunctionalError?.Select(x => new FunctionalErrorTypeProvider(x)).ToArray() ?? Array.Empty<FunctionalErrorTypeProvider>());
		IReadOnlyCollection<FunctionalErrorTypeProvider> functionalErrors;
	}
}
