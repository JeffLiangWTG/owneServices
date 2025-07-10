using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM416;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1
{
	public class IM416Provider : IIM416Provider
	{
		public IM416Provider(Im416 xmlObject)
		{
			this.xmlObject = xmlObject;
		}

		readonly Im416 xmlObject;

		public ZString AdditionalDeclarationType => xmlObject.Declaration?.Additionaldeclarationtype;

		public ZString LocalReferenceNumber => xmlObject.Declaration?.Lrn;

		public ZDateTime RejectionDate => DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(xmlObject.Declaration?.RejectionDate);

		public ZString RejectionMotivationText => xmlObject.Declaration?.RejectionMotivationText;

		public ZBool HasFunctionalErrors => xmlObject.FunctionalError?.Count > 0;

		public IReadOnlyCollection<FunctionalErrorTypeProvider> FunctionalErrors => functionalErrors ??= xmlObject.FunctionalError?.Select(x => new FunctionalErrorTypeProvider(x)).ToArray() ?? Array.Empty<FunctionalErrorTypeProvider>();
		IReadOnlyCollection<FunctionalErrorTypeProvider> functionalErrors;

		public string EntryStatus => AISEntryStatusList.Codes.Rejected;
	}
}
