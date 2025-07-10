using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	#region SuppressResourceStringsCheckRegion

	[DataContract]
	public class MTDErrorInfo
	{
		[DataMember]
		public string code { get; set; }

		[DataMember]
		public string message { get; set; }

		[DataMember(IsRequired = false)]
		public string path { get; set; }

		[DataMember(IsRequired = false)]
		public long reactivationTimestamp { get; set; }

		[DataMember(IsRequired = false)]
		public IEnumerable<MTDErrorInfo> errors { get; set; }

		public bool IsInvalidDateRangeError => CheckErrorType(InvalidDateRangeErrorCodes);

		public bool IsAuthorizationRelatedError => CheckErrorType(AuthenticationErrorCodes);

		public bool IsAccessTokenRelatedError => CheckErrorType(new[] { "CLIENT_OR_AGENT_NOT_AUTHORISED" });

		public override string ToString()
		{
			return FormattableString.Invariant($"Error Code: {code}, Message: {message}, Request path: {path}\r\n{(errors != null ? string.Join("\r\n", errors.Select(e => e.ToString()).ToArray()) : string.Empty)}");
		}

		bool CheckErrorType(string[] errorTypes)
			=> errorTypes.Contains(code) ||
			errorTypes.Contains(message) ||
			(errors?.Any(e => e.CheckErrorType(errorTypes)) ?? false);

		string[] InvalidDateRangeErrorCodes => new string[] { INVALID_DATE_FROM, INVALID_DATE_TO, INVALID_DATE_RANGE };

		string[] AuthenticationErrorCodes => new string[] { CLIENT_OR_AGENT_NOT_AUTHORISED, VRN_INVALID };

		const string INVALID_DATE_FROM = "INVALID_DATE_FROM";
		const string INVALID_DATE_TO = "INVALID_DATE_TO";
		const string INVALID_DATE_RANGE = "INVALID_DATE_RANGE";
		const string CLIENT_OR_AGENT_NOT_AUTHORISED = "CLIENT_OR_AGENT_NOT_AUTHORISED";
		const string VRN_INVALID = "VRN_INVALID";
	}

	#endregion
}
