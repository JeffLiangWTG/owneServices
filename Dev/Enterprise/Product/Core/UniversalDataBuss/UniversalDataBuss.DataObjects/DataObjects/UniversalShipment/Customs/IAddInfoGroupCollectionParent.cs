using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	public interface IAddInfoGroupCollectionParent
	{
		List<AddInfoGroup> AddInfoGroupCollection { get; }
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		bool SetAddInfoGroupCollection(Func<List<AddInfoGroup>> value);
	}
}
