using System;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	public class ActiveCollectionRelationship
	{
		public ActiveCollectionRelationship(Type elementType, ICollectionRelationship relationship)
		{
			ElementType = Argument.NotNull(elementType, nameof(elementType));
			Relationship = Argument.NotNull(relationship, nameof(relationship));
		}

		public Type ElementType { get; }
		public ICollectionRelationship Relationship { get; }
	}
}
