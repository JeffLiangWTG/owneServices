using System;

namespace CargoWise.EntityFramework
{
	[Flags]
	public enum TestBusinessObjectKind
	{
		NoData = 1 << 0,
		MinimumRequiredToSave = 1 << 1,
		PopulateDependentCollections = 1 << 2,
		PopulateManyToManyCollections = 1 << 9,
		PopulateRelatedObjects = 1 << 3,
		PopulateRelationsDeeply = 1 << 8,
		PopulateStrings = 1 << 4,
		PopulateNumbers = 1 << 7,
		PopulatePortCodes = 1 << 5,
		PopulateDates = 1 << 6,

		PopulateAllDependentAndRelatedObjectsDeeply = PopulateDependentCollections | PopulateManyToManyCollections | PopulateRelatedObjects | PopulateRelationsDeeply,
		All = 0xFFFFFFF,
	}
}
