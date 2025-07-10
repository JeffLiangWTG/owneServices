using System;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class NewBoardAcceptabilityBand : IAcceptabilityBandOverride
	{
		public NewBoardAcceptabilityBand(
			Guid pk,
			string name,
			string displayName,
			short seq,
			bool inHeading,
			bool asTile,
			bool filterByGroup,
			bool filterBySection,
			int cautionMin,
			int goodMin,
			int excellentMin,
			int excellentMax,
			int goodMax,
			int cautionMax,
			int maxItems)
		{
			AcceptabilityBandPK = pk;
			DisplayName = !string.IsNullOrEmpty(displayName) ? displayName : name;
			DisplaySequence = seq;
			ShouldShowInHeading = inHeading;
			ShouldShowAsTile = asTile;
			IsFilteringByReleaseGroup = filterByGroup;
			IsFilteringBySection = filterBySection;
			BoundaryValues = new AcceptabilityBandBoundaryValues(cautionMin, goodMin, excellentMin, excellentMax, goodMax, cautionMax);
			MaximumItems = maxItems;
		}

		public ZGuid AcceptabilityBandPK { get; }
		public ZString DisplayName { get; }
		public ZShort DisplaySequence { get; }
		public bool ShouldShowInHeading { get; }
		public bool ShouldShowAsTile { get; }
		public bool IsFilteringByReleaseGroup { get; }
		public bool IsFilteringBySection { get; }
		public AcceptabilityBandBoundaryValues BoundaryValues { get; }
		public ZInt MaximumItems { get; }
	}
}
