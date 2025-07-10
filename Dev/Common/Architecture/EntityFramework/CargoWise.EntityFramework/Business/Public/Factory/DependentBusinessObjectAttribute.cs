using System;
using System.ComponentModel;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Apply this attribute to a business object to specify the master business object of it. This should only be applied if there can
	/// only be one master.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class DependentBusinessObjectAttribute : Attribute
	{
		public DependentBusinessObjectAttribute(Type masterType, string detailRelationshipCollectionProperty)
		{
			this.MasterType = masterType;
			this.DetailRelationshipCollectionProperty = detailRelationshipCollectionProperty;
		}

		public readonly Type MasterType;
		public readonly string DetailRelationshipCollectionProperty;

		public IBusinessObjectCollection GetDependentCollection(BusinessObject master)
		{
			PropertyDescriptor property = TypeDescriptor.GetProperties(master)[DetailRelationshipCollectionProperty];
			if (property == null)
			{
				ErrorReporter.ReportOnce("NoDependentCollection" + DetailRelationshipCollectionProperty + master.GetType().FullName,
					"Could not find dependent collection property " + DetailRelationshipCollectionProperty +
					" on master business object " + master.GetType().FullName);
			}

			return property?.GetValue(master) as IBusinessObjectCollection;
		}
	}
}
