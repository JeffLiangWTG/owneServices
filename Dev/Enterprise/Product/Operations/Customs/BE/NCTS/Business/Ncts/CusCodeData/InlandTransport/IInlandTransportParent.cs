using Enterprise.Customs.Business;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.BE.NCTS.Business;

public interface IInlandTransportParent : ICusCodeDataTypeSupporter
{
	InlandTransportCollection InlandTransports { get; }
	ShortSequenceNumberGenerator InlandTransportLineNumberGenerator { get; }
}
