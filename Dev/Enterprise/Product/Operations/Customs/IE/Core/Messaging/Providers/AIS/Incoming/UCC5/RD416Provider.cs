using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.RD416;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public sealed class RD416Provider
	{
		public RD416Provider(Rd416 xmlObject)
		{
			this.xmlObject = Argument.NotNull(xmlObject, nameof(xmlObject));
		}

		readonly Rd416 xmlObject;

		public ZString ApplicationReferenceId => Header?.ApplicationReferenceId;
		public ZDateTime RejectionDate
		{
			get
			{
				const string RejectionDateFormat = "yyyyMMdd";
				ZDateTime.TryParseExact(Header?.RejectionDate, out var referenceDateTime, RejectionDateFormat);
				return referenceDateTime;
			}
		}
		public ZString RejectionReason => Header?.RejectionReason;
		public ZString Applicant => Header?.Applicant;
		public IReadOnlyCollection<FunctionalErrorTypeProvider> FunctionalErrors => functionalErrors ??= GetFunctionalErrors();
		IReadOnlyCollection<FunctionalErrorTypeProvider> functionalErrors;

		HeaderType Header => xmlObject.Header;
		IReadOnlyCollection<FunctionalErrorTypeProvider> GetFunctionalErrors() => xmlObject.FunctionalError?.Select(e => new FunctionalErrorTypeProvider(e)).ToArray() ?? Array.Empty<FunctionalErrorTypeProvider>();
	}
}
