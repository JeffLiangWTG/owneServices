using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	class CustomBusinessObjectPropertyInfoHashtable : ZPropertyInfoHashtable
	{
		public CustomBusinessObjectPropertyInfoHashtable(CustomBusinessObject cusObj)
			: base(cusObj)
		{
		}

		protected override IDictionary<string, PropertyDescriptor> NewPropertyNamesToPropertyDescriptorsDictionary()
		{
			return new PropertyDescriptorsDictionary((CustomBusinessObject)businessObject);
		}

#if DEBUG
		internal IDictionary<string, PropertyDescriptor> GetNewPropertyNamesToPropertyDescriptorsDictionaryForTest()
		{
			return NewPropertyNamesToPropertyDescriptorsDictionary();
		}
#endif

		class PropertyDescriptorsDictionary : IDictionary<string, PropertyDescriptor>
		{
			public PropertyDescriptorsDictionary(CustomBusinessObject cusObj)
			{
				customBizo = new WeakReference(cusObj);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "simple string comparison")]
			IEnumerable<string> GetPropertyInfoNames()
			{
				if (CustomBizo == null)
				{
					return Array.Empty<string>();
				}

				var query = from propertyName in ((IDynamicBusinessObject)CustomBizo).PropertyNames
							where (propertyName.EndsWith("Info", StringComparison.Ordinal) && CustomBizo.GetProperties().Find(propertyName, false).PropertyType.IsAssignableFrom(typeof(ZPropertyInfo)))
							select propertyName;

				return query;
			}

			#region IDictionary<string,PropertyDescriptor> Members

			public bool ContainsKey(string key)
			{
				return CustomBizo != null && CustomBizo.GetCustomProperty(key) != null;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "simple string comparison")]
			public bool TryGetValue(string key, out PropertyDescriptor value)
			{
				value = null;
				if (ContainsKey(key))
				{
					value = CustomBizo.GetProperties().Find(key + "Info", false);
				}

				return value != null;
			}

			public PropertyDescriptor this[string key]
			{
				get
				{
					PropertyDescriptor value;
					if (TryGetValue(key, out value))
					{
						return value;
					}
					else
					{
						throw new KeyNotFoundException();
					}
				}
				set
				{
					throw new NotSupportedException();
				}
			}

			public ICollection<string> Keys
			{
				get
				{
					return GetPropertyInfoNames().ToArray();
				}
			}

			public ICollection<PropertyDescriptor> Values
			{
				get
				{
					return CustomBizo == null
						? Array.Empty<PropertyDescriptor>()
						: (from propertyName in GetPropertyInfoNames() select CustomBizo.GetProperties().Find(propertyName, false)).ToArray();
				}
			}

			public void Add(string key, PropertyDescriptor value)
			{
				throw new NotSupportedException();
			}

			public bool Remove(string key)
			{
				throw new NotSupportedException();
			}

			#endregion

			#region ICollection<KeyValuePair<string,PropertyDescriptor>> Members

			public int Count
			{
				get { return GetPropertyInfoNames().Count(); }
			}

			public bool IsReadOnly
			{
				get { return true; }
			}

			public bool Contains(KeyValuePair<string, PropertyDescriptor> item)
			{
				return CustomBizo != null && CustomBizo.GetCustomProperty(item.Key) != null;
			}

			public void CopyTo(KeyValuePair<string, PropertyDescriptor>[] array, int arrayIndex)
			{
				throw new NotImplementedException();
			}

			public void Add(KeyValuePair<string, PropertyDescriptor> item)
			{
				throw new NotSupportedException();
			}

			public bool Remove(KeyValuePair<string, PropertyDescriptor> item)
			{
				throw new NotSupportedException();
			}

			public void Clear()
			{
				throw new NotSupportedException();
			}

			#endregion

			#region IEnumerable<KeyValuePair<string,PropertyDescriptor>> Members

			public IEnumerator<KeyValuePair<string, PropertyDescriptor>> GetEnumerator()
			{
				if (CustomBizo == null)
				{
					return new Dictionary<string, PropertyDescriptor>().GetEnumerator();
				}

				var query = from propertyName in GetPropertyInfoNames() select new KeyValuePair<string, PropertyDescriptor>(propertyName, CustomBizo.GetProperties().Find(propertyName, false));
				return query.GetEnumerator();
			}

			#endregion

			#region IEnumerable Members

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			#endregion

			CustomBusinessObject CustomBizo
			{
				get { return customBizo != null ? customBizo.Target as CustomBusinessObject : null; }
			}
			readonly WeakReference customBizo;
		}
	}
}
