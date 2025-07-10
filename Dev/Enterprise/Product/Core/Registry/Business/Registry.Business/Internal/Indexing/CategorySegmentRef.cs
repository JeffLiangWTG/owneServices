using System;
using System.Collections.Generic;
using System.Diagnostics;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Internal.Indexing
{
	[DebuggerDisplay("Ref: {SetType.Name,nq}.{PropertyName,nq}.Categories[{CategoryIndex}][{SegmentIndex}]")]
	class CategorySegmentRef
	{
		public CategorySegmentRef(Type setType, string propertyName, int categoryIndex, int segmentIndex)
		{
			SetType = setType;
			PropertyName = propertyName;
			CategoryIndex = categoryIndex;
			SegmentIndex = segmentIndex;
		}

		public string GetValue(IReadOnlyDictionary<Type, RegistryItemSet> sets)
		{
			return sets[SetType].GetItemByPropertyName(PropertyName).GetCategorySegments(CategoryIndex)[SegmentIndex];
		}

		public Type SetType { get; }
		public string PropertyName { get; }
		public int CategoryIndex { get; }
		public int SegmentIndex { get; }
	}
}
