using System;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class UniversalCopyCollectionEntityAttribute : Attribute
	{
		public UniversalCopyCollectionEntityAttribute(string itemsTableName, string itemPropertyName, string itemParentTablePropertyName = "")
		{
			ItemsTableName = itemsTableName;
			ItemPropertyName = itemPropertyName;
			ItemParentTablePropertyName = itemParentTablePropertyName;
		}

		public string OverrideCollectionName { get; set; }
		public string ItemsTableName { get; private set; }
		public string ItemPropertyName { get; private set; }
		public string ItemParentTablePropertyName { get; private set; }
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public sealed class UniversalCopyExtraCollectionAttribute : Attribute
	{
		public UniversalCopyExtraCollectionAttribute(string collectionName, string elementGlowInterfaceName, string itemsTableName, string itemPropertyName, string itemParentTablePropertyName = "")
		{
			CollectionName = collectionName;
			GlowInterfaceName = elementGlowInterfaceName;
			ItemsTableName = itemsTableName;
			ItemPropertyName = itemPropertyName;
			ItemParentTablePropertyName = itemParentTablePropertyName;
		}

		public string CollectionName { get; private set; }
		public string GlowInterfaceName { get; private set; }
		public string ItemsTableName { get; private set; }
		public string ItemPropertyName { get; private set; }
		public string ItemParentTablePropertyName { get; private set; }
	}
}
