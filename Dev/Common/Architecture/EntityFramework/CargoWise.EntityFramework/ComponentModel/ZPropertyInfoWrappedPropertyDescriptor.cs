using System;
using System.ComponentModel;
using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework
{
	internal class ZPropertyInfoWrappingPropertyDescriptor : WrappingPropertyDescriptor
	{
		public ZPropertyInfoWrappingPropertyDescriptor(BusinessObjectPropertyDescriptorCollection collection, PropertyDescriptor outerPropertyDescriptor, PropertyDescriptor innerPropertyDescriptor, string name)
			: base(collection, outerPropertyDescriptor, innerPropertyDescriptor, name)
		{
			zPropertyName = Name.Substring(0, Name.Length - 4);
		}

		protected override object GetValueCore(object component)
		{
			BusinessObject bizO = component as BusinessObject;
			if ((object)bizO == null)
			{
				throw new ArgumentException("Invalid argument as BusinessObject", nameof(component));
			}

			return bizO.GetWrappedZPropertyInfo(zPropertyName, x => GetInnerPropertyInfo(bizO));
		}

		ZPropertyInfo GetInnerPropertyInfo(BusinessObject bizO)
		{
			return base.GetValueCore(bizO) as ZPropertyInfo;
		}

		readonly string zPropertyName;
	}
}
