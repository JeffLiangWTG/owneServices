using System;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// A relationship on a collection that results in no records.
	/// </summary>
	public class NoResultRelationship : CollectionRelationship
	{
		public NoResultRelationship(Type elementType)
			: base(elementType, ZQuery.NoResultQuery)
		{
		}
	}
}
