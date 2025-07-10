using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_2.RF416;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class RF416Provider
	{
		public RF416Provider(Rf416 xmlObject)
		{
			this.xmlObject = Argument.NotNull(xmlObject, nameof(xmlObject));
		}
		readonly Rf416 xmlObject;

		public ZString ApplicationReferenceId => Header?.ApplicationReferenceId;

		public ZDateTime RejectionDate
		{
			get
			{
				var requestDate = new ZDateTime();
				Header?.RejectionDate.TryParseToDate(out requestDate);
				return requestDate;
			}
		}

		public ZString RejectionReason => Header?.RejectionReason;

		public ZString DecisionTakingCustomsAuthority => Header?.DecisionTakingCustomsAuthority;

		public ZString ApplicantOrHolderIdentification => Parties?.Applicant32;

		public ZString RepresentativeIdentification => Parties?.RepresentativeIdentification34;

		public IReadOnlyCollection<FunctionalErrorTypeProvider> FunctionalErrors => functionalErrors ??= GetFunctionalErrors();
		IReadOnlyCollection<FunctionalErrorTypeProvider> functionalErrors;

		HeaderType Header => xmlObject.Header;

		Rf416Parties Parties => xmlObject.Parties;

		IReadOnlyCollection<FunctionalErrorTypeProvider> GetFunctionalErrors() => xmlObject.FunctionalError?.Select(e => new FunctionalErrorTypeProvider(e)).ToArray() ?? Array.Empty<FunctionalErrorTypeProvider>();
	}
}
