using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public abstract class CustomPropertyCollection : ICustomPropertyCollection
	{
		protected CustomPropertyCollection()
		{
			properties = new SortedSet<ICustomProperty>(new CustomPropertyComparer());
		}

		public ICustomProperty Add(ICustomProperty customProperty)
		{
			return properties.Add(customProperty) ? customProperty : null;
		}

		public ICustomProperty Add(Type type, string identifier, params DynamicMetaData[] metaData)
		{
			var customProperty = this.Create(type, identifier, metaData);
			return properties.Add(customProperty) ? customProperty : null;
		}

		public ICustomProperty Add(Type type, string identifier, bool readOnly, params DynamicMetaData[] metaData)
		{
			var customProperty = this.Create(type, identifier, readOnly, metaData);
			return properties.Add(customProperty) ? customProperty : null;
		}

		public ICustomProperty Add(Type type, string identifier, Action<ZPropertyInfo> validator, params DynamicMetaData[] metaData)
		{
			var customProperty = this.Create(type, identifier, validator, metaData);
			return properties.Add(customProperty) ? customProperty : null;
		}

		public ICustomProperty Add(Type type, string identifier, Action<ZPropertyInfo> validator, Action<BusinessObject, string, object, ZGuid> onSet, params DynamicMetaData[] metaData)
		{
			var customProperty = this.Create(type, identifier, validator, onSet, metaData);
			return properties.Add(customProperty) ? customProperty : null;
		}

		public ICustomProperty Add(Type type, string identifier, Action<ZPropertyInfo> validator, Action<BusinessObject, string, object, ZGuid> onSet, Func<IEnumerable<ICustomProperty>> relatedProperties, params DynamicMetaData[] metaData)
		{
			var customProperty = this.Create(type, identifier, validator, onSet, relatedProperties, metaData);
			return properties.Add(customProperty) ? customProperty : null;
		}

		internal object GetValue(BusinessObject cusObj, string propertyIdentifier)
		{
			return GetValueCore(cusObj, propertyIdentifier);
		}

		internal bool TrySetValue(BusinessObject cusObj, string propertyIdentifier, object value)
		{
			return TrySetValueCore(cusObj, propertyIdentifier, value);
		}

		protected abstract object GetValueCore(BusinessObject cusObj, string propertyIdentifier);
		protected abstract bool TrySetValueCore(BusinessObject cusObj, string propertyIdentifier, object value);

		#region IEnumerable<ICustomProperty> Members

		IEnumerator<ICustomProperty> IEnumerable<ICustomProperty>.GetEnumerator()
		{
			return properties.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return properties.GetEnumerator();
		}

		public ICustomProperty GetCustomProperty(string identifier)
		{
			return this.FirstOrDefault(i => i.Identifier == identifier);
		}

		public string[] GetIdentifiers()
		{
			return this.Select(i => i.Identifier).ToArray();
		}

		public IEnumerable<ICustomProperty> GetProperties()
		{
			return this;
		}

		public void AddCustomProperties(IEnumerable<ICustomProperty> properties)
		{
			foreach (var customProperty in properties)
			{
				if (!this.Contains(customProperty))
				{
					Add(customProperty);
				}
			}
		}

		#endregion

		readonly SortedSet<ICustomProperty> properties;
	}

	public class CustomPropertyCollectionImpl : CustomPropertyCollection
	{
		public CustomPropertyCollectionImpl(Func<string, object> getter, Func<string, object, bool> setter)
		{
			this.getter = (cusObj, propertyIdentifier) => getter(propertyIdentifier);
			this.setter = (cusObj, propertyIdentifier, value) => setter(propertyIdentifier, value);
		}

		public CustomPropertyCollectionImpl(Func<BusinessObject, string, object> getter, Func<BusinessObject, string, object, bool> setter)
		{
			this.getter = getter;
			this.setter = setter;
		}

		protected override object GetValueCore(BusinessObject cusObj, string propertyIdentifier)
		{
			return getter(cusObj, propertyIdentifier);
		}

		protected override bool TrySetValueCore(BusinessObject cusObj, string propertyIdentifier, object value)
		{
			return setter(cusObj, propertyIdentifier, value);
		}

		readonly Func<BusinessObject, string, object> getter;
		readonly Func<BusinessObject, string, object, bool> setter;
	}
}
