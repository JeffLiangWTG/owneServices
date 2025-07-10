using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace CargoWise.EntityFramework
{
	public class ZDynamicPropertyInfoHashtable : ZPropertyInfoHashtable, IEnumerable
	{
		public ZDynamicPropertyInfoHashtable(BusinessObject bizObj)
			: base(bizObj)
		{
		}

		public override ZPropertyInfo this[string propertyName]
		{
			get
			{
				if (!PropertyNamesToPropertyDescriptors.ContainsKey(propertyName))
				{
					return new ZDynamicPropertyInfo(businessObject, propertyName);
				}
				return base[propertyName];
			}
		}

		public override ZPropertyInfo GetPropertySafe(string propertyName)
		{
			return this[propertyName];
		}

		protected override IDictionary<string, PropertyDescriptor> NewPropertyNamesToPropertyDescriptorsDictionary()
		{
			return new Dictionary<string, PropertyDescriptor>();
		}

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return new Enumerator(this, (IDynamicBusinessObject)businessObject);
		}

		class Enumerator : IEnumerator
		{
			public Enumerator(ZDynamicPropertyInfoHashtable hash, IDynamicBusinessObject bizObj)
			{
				this.hash = hash;
				enumerator = bizObj.PropertyNames.GetEnumerator();
			}

			readonly ZDynamicPropertyInfoHashtable hash;
			readonly IEnumerator enumerator;

			#region IEnumerator Members

			public object Current
			{
				get { return hash[enumerator.Current.ToString()]; }
			}

			public bool MoveNext()
			{
				return enumerator.MoveNext();
			}

			public void Reset()
			{
				enumerator.Reset();
			}

			#endregion
		}

		#endregion
	}
}
