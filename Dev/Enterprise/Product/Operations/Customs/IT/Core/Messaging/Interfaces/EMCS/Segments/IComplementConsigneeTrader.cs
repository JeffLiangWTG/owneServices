using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public interface IComplementConsigneeTrader
{
	ZString MemberStateCode { get; }

	ZString SerialNumberOfCertificateOfExemption { get; }
}
