using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS316;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class TS316Provider
	{
		public TS316Provider(Ts316 xmlObject)
		{
			this.xmlObject = xmlObject;
			declaration = Argument.NotNull(xmlObject.Declaration, nameof(xmlObject.Declaration));
		}
		readonly Ts316 xmlObject;
		readonly DeclarationType declaration;

		public ZString LRN => declaration.Lrn25;

		public ZDateTime RejectionDate
		{
			get
			{
				new ZString(declaration.RejectionDate).TryParseToDate(out var rejectionDate);
				return rejectionDate;
			}
		}

		public ZString RejectionMotivationText => declaration.RejectionMotivationText;

		public IReadOnlyCollection<FunctionalErrorTypeProvider> FunctionalErrors => functionalErrors ??= xmlObject.FunctionalError?.Select(x => new FunctionalErrorTypeProvider(x)).ToArray() ?? Array.Empty<FunctionalErrorTypeProvider>();
		IReadOnlyCollection<FunctionalErrorTypeProvider> functionalErrors;
	}
}
