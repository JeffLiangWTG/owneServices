using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging;

public interface IIM099Provider
{
	ZString LocalReferenceNumber { get; }

	ZDateTime DateLimitOfResponse { get; }

	ZString Remarks { get; }

	ZString CustomsOfficeLodgement { get; }
}
