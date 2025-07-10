using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

public class ArchiveInformation : ITArchiveInformation
{
	public ArchiveInformation(CusSupportingInfo supportingInfo)
	{
	}

	public ZString ArchiveLocationIndicator { get => ZString.Empty; }
	public ZString ArchiveRelatedLocationContent { get => ZString.Empty; }
	public ZString ArchiveSupport { get => ZString.Empty; }
}
