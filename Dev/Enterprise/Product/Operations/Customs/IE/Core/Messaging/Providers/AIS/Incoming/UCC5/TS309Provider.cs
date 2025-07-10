using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS309;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class TS309Provider
	{
		public TS309Provider(Ts309 xmlObject)
		{
			this.xmlObject = Argument.NotNull(xmlObject, nameof(xmlObject));
			declaration = Argument.NotNull(xmlObject.Declaration, nameof(xmlObject.Declaration));
		}
		readonly Ts309 xmlObject;
		readonly DeclarationType declaration;

		public ZString MovementReferenceNumber => declaration.Mrn;

		public ZBool InvalidationDecision => declaration.InvalidationDecision;

		public ZBool InvalidationInitiatedByCustoms => declaration.InvalidationInitiatedByCustoms;

		public ZString InvalidationJustification => declaration.InvalidationJustification;

		public ZDateTime DateOfInvalidationDecision => DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(declaration.DateOfInvalidationDecision);

		public ZDateTime DateOfInvalidationRequest => DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(declaration.DateOfInvalidationRequest);

		public ZDateTime DateOfInvalidation => DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(declaration.DateOfInvalidation);

		public IReadOnlyCollection<FunctionalErrorTypeProvider> FunctionalErrors => functionalErrors ??= xmlObject.FunctionalError?.Select(x => new FunctionalErrorTypeProvider(x)).ToArray() ?? Array.Empty<FunctionalErrorTypeProvider>();
		IReadOnlyCollection<FunctionalErrorTypeProvider> functionalErrors;
	}
}
