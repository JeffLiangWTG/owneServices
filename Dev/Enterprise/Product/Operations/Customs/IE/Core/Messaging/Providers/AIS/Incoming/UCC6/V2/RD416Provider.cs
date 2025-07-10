using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_2.RD416;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class RD416Provider
	{
		public RD416Provider(Rd416Type xmlObject)
		{
			this.xmlObject = Argument.NotNull(xmlObject, nameof(xmlObject));
		}
		readonly Rd416Type xmlObject;

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
