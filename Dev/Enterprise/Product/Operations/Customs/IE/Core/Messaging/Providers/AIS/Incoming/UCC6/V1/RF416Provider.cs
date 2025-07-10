using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.RF416;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1
{
	public class RF416Provider
	{
		public RF416Provider(Rf416 xmlObject)
		{
			this.xmlObject = Argument.NotNull(xmlObject, nameof(xmlObject));
		}
		readonly Rf416 xmlObject;

		public ZString ApplicationReferenceId => Header?.ApplicationReferenceId;
		public ZString RejectionDate => Header?.RejectionDate;
		public ZString RejectionReason => Header?.RejectionReason;
		public ZString DecisionTakingCustomsAuthority => Header?.DecisionTakingCustomsAuthority;
		public ZString ApplicantOrHolderIdentification => Parties?.Applicant;
		public ZString RepresentativeIdentification => Parties?.RepresentativeIdentification;
		public IReadOnlyCollection<FunctionalErrorTypeProvider> FunctionalErrors => functionalErrors ??= GetFunctionalErrors();
		IReadOnlyCollection<FunctionalErrorTypeProvider> functionalErrors;

		HeaderType Header => xmlObject.Header;
		PartiesType Parties => xmlObject.Parties;
		IReadOnlyCollection<FunctionalErrorTypeProvider> GetFunctionalErrors() => xmlObject.FunctionalError?.Select(e => new FunctionalErrorTypeProvider(e)).ToArray() ?? Array.Empty<FunctionalErrorTypeProvider>();
	}
}
