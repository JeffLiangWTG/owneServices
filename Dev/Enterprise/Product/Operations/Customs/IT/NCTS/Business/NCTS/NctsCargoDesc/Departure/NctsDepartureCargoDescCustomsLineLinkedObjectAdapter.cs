using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsDepartureCargoDescCustomsLineLinkedObjectAdapter : ISadCustomsLineLinkedObjectAdapter
{
	public NctsDepartureCargoDescCustomsLineLinkedObjectAdapter(NctsDepartureCargoDesc nctsDepartureCargoDesc)
	{
		this.nctsDepartureCargoDesc = Argument.NotNull(nctsDepartureCargoDesc, nameof(nctsDepartureCargoDesc));
	}
	readonly NctsDepartureCargoDesc nctsDepartureCargoDesc;

	ZInt ISadCustomsLineLinkedObjectAdapter.LineNo => nctsDepartureCargoDesc.BY_LineNo;

	ZString ISadCustomsLineLinkedObjectAdapter.NBStatus => nctsDepartureCargoDesc.BY_Status;

	void ISadCustomsLineLinkedObjectAdapter.SetNBStatus(ZString nbStatus) => nctsDepartureCargoDesc.BY_Status = nbStatus;
}
