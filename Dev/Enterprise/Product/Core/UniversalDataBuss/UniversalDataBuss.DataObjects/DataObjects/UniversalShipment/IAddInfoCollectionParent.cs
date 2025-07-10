using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	public interface IAddInfoCollectionParent
	{
		List<AddInfo> AddInfoCollection { get; }
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		bool SetAddInfoCollection(Func<List<AddInfo>> getter);
	}
}
