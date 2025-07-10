using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public interface IAcceptabilityBandOverride
	{
		ZGuid AcceptabilityBandPK { get; }

		ZString DisplayName { get; }

		ZShort DisplaySequence { get; }

		bool ShouldShowInHeading { get; }

		bool ShouldShowAsTile { get; }

		bool IsFilteringByReleaseGroup { get; }

		bool IsFilteringBySection { get; }

		AcceptabilityBandBoundaryValues BoundaryValues { get; }

		ZInt MaximumItems { get; }
	}
}
