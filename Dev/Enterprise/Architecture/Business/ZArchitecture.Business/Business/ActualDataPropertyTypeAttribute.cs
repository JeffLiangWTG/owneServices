using System;
using CargoWise.Application;

namespace Enterprise.ZArchitecture.Business
{
	/// <summary>
	/// Specifies the property type for whose can not return the actual data type
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class ActualDataPropertyTypeAttribute : Attribute
	{
		public ActualDataPropertyTypeAttribute(Type baseDataPropertyType)
		{
			if (baseDataPropertyType == null)
			{
				throw new ArgumentNullException(nameof(baseDataPropertyType));
			}

			this.dataPropertyType = ObjectFactory.GetType(baseDataPropertyType);
		}

		public Type DataPropertyType => dataPropertyType;
		readonly Type dataPropertyType;
	}
}
