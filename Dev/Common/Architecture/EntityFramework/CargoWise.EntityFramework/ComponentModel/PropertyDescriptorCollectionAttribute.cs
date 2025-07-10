using System;

namespace CargoWise.EntityFramework
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public sealed class PropertyDescriptorCollectionAttribute : Attribute
	{
		public PropertyDescriptorCollectionAttribute(Type collectionType)
		{
			this.CollectionType = collectionType;
		}

		public readonly Type CollectionType;
	}
}
