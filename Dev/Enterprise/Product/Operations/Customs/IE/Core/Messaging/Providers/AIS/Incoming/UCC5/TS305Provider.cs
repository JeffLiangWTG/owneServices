using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS305;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class TS305Provider
	{
		public TS305Provider(Ts305 xmlObject)
		{
			this.xmlObject = xmlObject;
			declaration = Argument.NotNull(xmlObject.Declaration, nameof(xmlObject.Declaration));
		}
		readonly Ts305 xmlObject;
		readonly DeclarationType declaration;

		public ZString MovementReferenceNumber => declaration.Mrn;

		public ZDateTime AmendmentRejectionDate => DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(declaration.AmendmentRejectionDate);

		public ZString RejectionReason => declaration.AmendmentRejectionReason;

		public IReadOnlyCollection<FunctionalErrorTypeProvider> FunctionalErrors => functionalErrors ??= xmlObject.FunctionalError?.Select(x => new FunctionalErrorTypeProvider(x)).ToArray() ?? Array.Empty<FunctionalErrorTypeProvider>();
		IReadOnlyCollection<FunctionalErrorTypeProvider> functionalErrors;
	}
}
