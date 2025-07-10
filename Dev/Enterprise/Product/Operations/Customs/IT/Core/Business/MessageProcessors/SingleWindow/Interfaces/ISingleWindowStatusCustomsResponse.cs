using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface ISingleWindowStatusCustomsResponse
{
	ZString ControlChannel { get; }
	ZString ReleaseCode { get; }
	ZDateTime ReleaseDate { get; }
}
