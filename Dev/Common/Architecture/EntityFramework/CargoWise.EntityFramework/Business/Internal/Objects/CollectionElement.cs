using System.Collections.Generic;
using System.ComponentModel;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// This class was formerly CollectionElement - now all partial classes to reduce memory footprint of reflection
	/// </summary>
	public abstract partial class BusinessObject : ZCustomTypeDescriptor
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		internal ICollection<BusinessObjectCollection> ParentCollections
		{
			get { return parentCollections; }
		}
		ICollection<BusinessObjectCollection> parentCollections = emptyCollection;
		static readonly ICollection<BusinessObjectCollection> emptyCollection = System.Array.Empty<BusinessObjectCollection>();

		internal void AddParentCollection(BusinessObjectCollection collection)
		{
			if (collection != null)
			{
				var length = parentCollections.Count;
				if (length == 0)
				{
					parentCollections = new List<BusinessObjectCollection> { collection };
				}
				else
				{
					parentCollections.Add(collection);
				}
			}
		}

		internal void RemoveParentCollection(BusinessObjectCollection collection)
		{
			if (parentCollections.Count > 0)
			{
				parentCollections.Remove(collection);
				if (parentCollections.Count == 0)
				{
					parentCollections = emptyCollection;
				}
			}
		}
	}
}
