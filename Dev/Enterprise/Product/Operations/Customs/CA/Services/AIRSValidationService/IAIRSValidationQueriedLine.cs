using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Services
{
	public interface IAIRSValidationQueriedLine
	{
		string CommodityGroup { get; }
		string Commodity { get; }
		string HSNumber { get; }
		string AirsCode { get; }
		string OriginCountry { get; }
		string OriginState { get; }
		string EndUse { get; }
		string Miscellaneous { get; }

		bool ValidationCompleted { get; set; }
		string ValidationResponse { get; set; }
		ZString ValidationFaultMessage { get; set; }
		ValidationFaultMessageType ValidationFaultMessageType { get; set; }
		List<ZGuid> InvoiceLinePKs { get; }
		IEnumerable<IAIRSValidationQueriedLineRegistration> Registrations { get; }
	}

	public interface IAIRSValidationQueriedLineRegistration
	{
		string RegistrationId { get; }
		AIRSValidationQueriedLineRegistrationTypes RegistrationType { get; }
	}

	public enum AIRSValidationQueriedLineRegistrationTypes
	{
		Normal,
		MaterializedLpco,
		DematerializedLpco
	}

	public enum ValidationFaultMessageType
	{
		None = 0,
		QueryAborted = 1,
		HttpError = 2,
		SOAPError = 3
	}
}
