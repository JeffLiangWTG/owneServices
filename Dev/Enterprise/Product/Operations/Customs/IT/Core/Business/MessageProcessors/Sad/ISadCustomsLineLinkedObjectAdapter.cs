using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface ISadCustomsLineLinkedObjectAdapter
{
	ZInt LineNo { get; }
	ZString NBStatus { get; }
	void SetNBStatus(ZString nbStatus);
}
