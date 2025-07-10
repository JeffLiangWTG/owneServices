using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class WorkflowCustomPropertyDescriptorForTesting : ZCustomPropertyDescriptor, IZPropertyInfoRetriever
	{
		public WorkflowCustomPropertyDescriptorForTesting(string identifier, Type propertyType, bool initializeCustomBizo = false)
			: base(identifier, propertyType)
		{
			this.initializeCustomBizo = initializeCustomBizo;
		}

		readonly bool initializeCustomBizo;

		protected override ICustomPropertyContainer GetCustomPropertyContainer(object component)
		{
			return (component as BusinessObject)?.GetCustomBusinessObject(initializeCustomBizo) ?? base.GetCustomPropertyContainer(component);
		}

		public ZPropertyInfo GetZPropertyInfo(BusinessObject businessObject)
		{
			var customBusinessObject = businessObject.GetCustomBusinessObject(initializeCustomBizo);
			return customBusinessObject != null ? customBusinessObject.ZPropertyInfoHash.GetPropertySafe(Name) : null;
		}
	}
}
