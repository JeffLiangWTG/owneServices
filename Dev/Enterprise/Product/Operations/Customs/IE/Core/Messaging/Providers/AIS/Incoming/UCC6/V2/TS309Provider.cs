using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS309;

using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class TS309Provider
	{
		public TS309Provider(Ts309 xmlObject)
		{
			this.xmlObject = Argument.NotNull(xmlObject, nameof(xmlObject));
		}
		readonly Ts309 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration?.Mrn;

		public ZBool InvalidationDecision => xmlObject.Declaration?.InvalidationDecision ?? false;

		public ZBool InvalidationInitiatedByCustoms => xmlObject.Declaration?.InvalidationInitiatedByCustoms ?? false;

		public ZString InvalidationJustification => xmlObject.Declaration?.InvalidationJustification;

		public ZDateTime DateOfInvalidationDecision => (xmlObject.Declaration?.DateOfInvalidationDecision).ConvertToZDateTime();

		public ZDateTime DateOfInvalidationRequest => (xmlObject.Declaration?.DateOfInvalidationRequest).ConvertToZDateTime();

		public ZDateTime DateOfInvalidation => (xmlObject.Declaration?.DateOfInvalidation).ConvertToZDateTime();

		public IReadOnlyCollection<MFunctionalError01Provider> FunctionalErrors => functionalErrors ?? (functionalErrors = GetFunctionalErrors());
		IReadOnlyCollection<MFunctionalError01Provider> functionalErrors;

		IReadOnlyCollection<MFunctionalError01Provider> GetFunctionalErrors() => xmlObject.FunctionalError?.Select(e => new MFunctionalError01Provider(e)).ToArray() ?? Array.Empty<MFunctionalError01Provider>();
	}
}
