using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class BusinessObjectWithCustomBusinessObject : NonPersistentBusinessObject
	{
		public CustomBusinessObject CustomBusinessObject
		{
			get
			{
				if (customBusinessObject == null)
				{
					Dictionary<string, object> values = new Dictionary<string, object>();
					customBusinessObject = new CustomBusinessObject(this, new CustomPropertyCollectionImpl(
						propertyName =>
						{
							object value;
							return values.TryGetValue(propertyName, out value) ? value : null;
						},
						(propertyName, value) =>
						{
							values[propertyName] = value;
							return true;
						})
					{
						{ typeof(ZString), "ZZZ_String1" },
						{ typeof(ZString), "ZZZ_String2" },
					});
				}

				return customBusinessObject;
			}
		}
		CustomBusinessObject customBusinessObject;
	}
}
