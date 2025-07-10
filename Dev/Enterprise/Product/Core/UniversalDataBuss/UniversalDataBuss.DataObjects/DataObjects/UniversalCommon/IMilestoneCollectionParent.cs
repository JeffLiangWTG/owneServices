using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	public interface IMilestoneCollectionParent
	{
		List<Milestone> MilestoneCollection { get; }
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		bool SetMilestoneCollection(Func<List<Milestone>> getter);
	}
}
