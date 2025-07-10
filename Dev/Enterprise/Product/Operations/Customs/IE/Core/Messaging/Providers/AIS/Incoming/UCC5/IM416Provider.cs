using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM416;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class IM416Provider
	{
		public IM416Provider(Im416 xmlObject)
		{
			this.xmlObject = xmlObject;
			declaration = Argument.NotNull(xmlObject.Declaration, nameof(xmlObject.Declaration));
		}
		readonly Im416 xmlObject;
		readonly DeclarationType declaration;

		public ZString AdditionalDeclarationType => declaration.AdditionalDeclarationType12;

		public ZString LocalReferenceNumber => declaration.Lrn25;

		public ZDateTime RejectionDate => DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(declaration.RejectionDate);

		public ZString RejectionMotivationText => declaration.RejectionMotivationText;

		public IReadOnlyCollection<FunctionalErrorTypeProvider> FunctionalErrors => functionalErrors ??= xmlObject.FunctionalError?.Select(x => new FunctionalErrorTypeProvider(x)).ToArray() ?? Array.Empty<FunctionalErrorTypeProvider>();
		IReadOnlyCollection<FunctionalErrorTypeProvider> functionalErrors;
	}
}
