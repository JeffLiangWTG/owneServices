using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Threading;
using CargoWise.Common;
using CargoWise.Common.Testing;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// A wrapper around an existing PropertyDescriptor object. This is convenient when you only
	/// want to override one or 2 properties but don't want to override all of them.
	/// </summary>
	[DebuggerDisplay("Name={Name},ComponentType={ComponentType},PropertyType={PropertyType}")]
	public class KPropertyDescriptor : PropertyDescriptor
	{
		/// <summary>
		/// Wraps an existing PropertyDescriptor.
		/// </summary>
		public KPropertyDescriptor(KPropertyDescriptorCollection collection, PropertyDescriptor inner)
			: base(inner.Name, null) // don't pass inner to the base for performance - it reads all the attributes up front
		{
			Argument.NotNull(inner, nameof(inner)); // Suggested By ReviewBot 
			Collection = collection;
			Inner = inner;
		}

		/// <summary>
		/// Doesn't wrap a PropertyDescriptor but gives you the extra features.
		/// </summary>
		protected KPropertyDescriptor(KPropertyDescriptorCollection collection, string name, Attribute[] attributes)
			: base(name, attributes)
		{
			Collection = collection;
			Inner = null;
		}

		/// <summary>
		/// Get the collection this property is a part of.
		/// </summary>
		public KPropertyDescriptorCollection Collection { get; private set; }

		/// <summary>
		/// The wrapped PropertyDescriptor.
		/// </summary>
		public PropertyDescriptor Inner { get; private set; }

		/// <summary>
		/// Get a collection of additional properties.
		/// </summary>
		[SuppressWeaklyTypedCollectionMessage]
		public IDictionary UserData
		{
			get
			{
				if (userData == null)
				{
					Interlocked.CompareExchange(ref userData, Hashtable.Synchronized(new Hashtable()), null);
				}
				return userData;
			}
		}
		IDictionary userData;

		/// <summary>
		/// Fire the property's change event. This involves invoking all the delegates that
		/// were added with AddValueChanged (which is what .NET binding uses to subscribe to
		/// an event).
		/// </summary>
		public virtual void FireChangeEvent(object component)
		{
			OnValueChanged(component, EventArgs.Empty);
		}

		#region ToString / GetHashCode / Equals

		public override int GetHashCode()
		{
			if (hash == 0)
			{
				if (ComponentType != null)
				{
					hash = ComponentType.FullName.GetHashCode() ^ Name.GetHashCode() ^ GetType().GetHashCode();
				}
				else
				{
					hash = -1;
				}
			}
			return hash;
		}

		public override bool Equals(object obj)
		{
			var rhs = obj as KPropertyDescriptor;
			return rhs != null && GetType() == rhs.GetType() && ComponentType == rhs.ComponentType && Name == rhs.Name;
		}

		#endregion

		#region GetValue / SetValue

		protected internal virtual object GetValueForMetaData(object component)
		{
			return GetValue(component);
		}

		public sealed override object GetValue(object component)
		{
			try
			{
				return GetValueCore(component);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				HandleException(component, e);
				throw;
			}
		}

		protected virtual object GetValueCore(object component)
		{
			object result = null;
			if (GetMethod != null && !IsDesignTime(component))
			{
				if (component != null)
				{
					if (FastPropertyGetter != null)
					{
						try
						{
							result = FastPropertyGetter(component);
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							if (!InnerOrNullActionProperty.ComponentType.IsInstanceOfType(component))
							{
								throw new TargetException();
							}
							throw;
						}
					}
					else
					{
						result = SafeMethodInvoker.SafeMethodInvoke(GetMethod, component, null);
					}
				}
			}
			else
			{
				result = InnerOrNullActionProperty.GetValue(component);
			}
			return result;
		}

		public sealed override void SetValue(object component, object value)
		{
			try
			{
				SetValueCore(component, value);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				HandleException(component, e);
				throw;
			}
		}

		protected virtual void SetValueCore(object component, object value)
		{
			if (HasSetMethod && !IsDesignTime(component))
			{
				if (component != null && HasSetter())
				{
					try
					{
						SafeMethodInvoker.SafeMethodInvoke(SetMethod, component, new object[] { value });
					}
					catch (TargetInvocationException ex)
					{
						if (ex.InnerException != null)
						{
							throw ex.InnerException;
						}
						throw;
					}
				}
			}
			else
			{
				InnerOrNullActionProperty.SetValue(component, value);
			}
		}

		static bool IsDesignTime(object obj)
		{
			var component = obj as IComponent;
			return component != null && component.Site != null;
		}

		FastPropertyGetterDelegate FastPropertyGetter
		{
			get
			{
				if (!fastPropertyGetterPopulated)
				{
					if (HasGetMethod)
					{
						try
						{
							if (GetMethod.DeclaringType != null)
							{
								fastPropertyGetter = (FastPropertyGetterDelegate)SafeMethodInvoker.CreateLightWeightMethodInvoker(GetMethod.Name, GetMethod, typeof(FastPropertyGetterDelegate), typeof(object), new Type[] { typeof(object) });
							}
						}
						catch (SecurityException)
						{
						}
						finally
						{
							fastPropertyGetterPopulated = fastPropertyGetter != null;
						}
					}
				}
				return fastPropertyGetter;
			}
		}
		bool fastPropertyGetterPopulated;
		FastPropertyGetterDelegate fastPropertyGetter;

		delegate object FastPropertyGetterDelegate(object component);

		#endregion

		#region AddValueChanged / RemoveValueChanged and related

		protected virtual bool ShouldHookInnerValueChanged
		{
			get { return true; }
		}

		public override void AddValueChanged(object component, EventHandler handler)
		{
			if (ShouldHookInnerValueChanged)
			{
				InnerOrNullActionProperty.AddValueChanged(component, handler);
			}
			foreach (PropertyDescriptor property in CalculatedFromProperties)
			{
				property.AddValueChanged(component, handler);
			}
		}

		public override void RemoveValueChanged(object component, EventHandler handler)
		{
			if (ShouldHookInnerValueChanged)
			{
				InnerOrNullActionProperty.RemoveValueChanged(component, handler);
			}
			foreach (var property in CalculatedFromProperties)
			{
				property.RemoveValueChanged(component, handler);
			}
		}

		#endregion

		#region HasSetter

		public bool HasSetter()
		{
			return (bool)(hasSetter ?? (hasSetter = HasSetterCore()));
		}
		bool? hasSetter;

		protected virtual bool HasSetterCore()
		{
			return Inner.HasSetter();
		}

		#endregion

		#region GetCalculatedFromProperties

		public PropertyDescriptor[] CalculatedFromProperties
		{
			get
			{
				if (calculatedFromProperties == null)
				{
					calculatedFromProperties = NewCalculatedFromProperties();
				}
				return calculatedFromProperties;
			}
		}
		PropertyDescriptor[] calculatedFromProperties;

		protected virtual PropertyDescriptor[] NewCalculatedFromProperties()
		{
			var result = new List<PropertyDescriptor>();
			var collectionCached = Collection;
			if (collectionCached != null)
			{
				foreach (CalculatedFromAttribute attribute in GetAttributesAllowMultiple(typeof(CalculatedFromAttribute)))
				{
					foreach (var propertyName in attribute.PropertyNames)
					{
						if (propertyName != null)
						{
							var property = collectionCached.AllProperties[propertyName.Replace('.', '+')];
							if (property != null && !result.Contains(property))
							{
								result.Add(property);
							}
						}
					}
				}
			}
			return result.ToArray();
		}

		#endregion

		#region GetAttributesAllowMultiple

		public IEnumerable<Attribute> GetAttributesAllowMultiple(Type attributeType)
		{
			return GetAttributesAllowMultipleCore(attributeType);
		}

		protected virtual IEnumerable<Attribute> GetAttributesAllowMultipleCore(Type attributeType)
		{
			IEnumerable<Attribute> result;
			if (InnerOrNullActionProperty is KPropertyDescriptor)
			{
				result = (InnerOrNullActionProperty as KPropertyDescriptor).GetAttributesAllowMultiple(attributeType);
			}
			else
			{
				result = PropertyDescriptorExtensions.GetAttributesAllowMultiple(InnerOrNullActionProperty, attributeType);
			}
			return result;
		}

		#endregion

		#region PropertyDescriptor Overrides

		public override AttributeCollection Attributes
		{
			get { return InnerOrNullActionProperty.Attributes; }
		}

		public override bool CanResetValue(object component)
		{
			return InnerOrNullActionProperty.CanResetValue(component);
		}

		public override string Category
		{
			get { return InnerOrNullActionProperty.Category; }
		}

		public override Type ComponentType
		{
			get { return InnerOrNullActionProperty.ComponentType; }
		}

		public override TypeConverter Converter
		{
			get { return InnerOrNullActionProperty.Converter; }
		}

		public override string Description
		{
			get { return InnerOrNullActionProperty.Description; }
		}

		public override bool DesignTimeOnly
		{
			get { return InnerOrNullActionProperty.DesignTimeOnly; }
		}

		public override string DisplayName
		{
			get { return InnerOrNullActionProperty.DisplayName; }
		}

		public override PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter)
		{
			if (Collection != null && PropertyType != null)
			{
				var result = Collection.NewEmptyPropertyDescriptorCollection(PropertyType, false);
				var childProperties = InnerOrNullActionProperty.GetChildProperties(instance, filter);
				if (childProperties != null)
				{
					foreach (PropertyDescriptor current in childProperties)
					{
						if (current != null)
						{
							result.Add(result.NewPropertyDescriptor(current));
						}
					}
				}
				return result;
			}
			return null;
		}

		public override object GetEditor(Type editorBaseType)
		{
			return InnerOrNullActionProperty.GetEditor(editorBaseType);
		}

		public override bool IsBrowsable
		{
			get { return isBrowsable ?? (isBrowsable = InnerOrNullActionProperty.IsBrowsable).Value; }
		}
		bool? isBrowsable; // Why cache this flag but not the others in the same class? The other properties aren't on the hot path of binding.

		public override bool IsLocalizable
		{
			get { return InnerOrNullActionProperty.IsLocalizable; }
		}

		public override bool IsReadOnly
		{
			get { return InnerOrNullActionProperty.IsReadOnly; }
		}

		public override Type PropertyType
		{
			get { return InnerOrNullActionProperty.PropertyType; }
		}

		public override void ResetValue(object component)
		{
			InnerOrNullActionProperty.ResetValue(component);
		}

		public override bool ShouldSerializeValue(object component)
		{
			return InnerOrNullActionProperty.ShouldSerializeValue(component);
		}

		public override bool SupportsChangeEvents
		{
			get { return InnerOrNullActionProperty.SupportsChangeEvents; }
		}

		#endregion

		#region NullPropertyDescriptor class

		class NullPropertyDescriptor : PropertyDescriptor
		{
			public NullPropertyDescriptor(string name, Attribute[] attributes)
				: base(name, attributes)
			{
			}

			public override bool CanResetValue(object component)
			{
				return false;
			}

			public override Type ComponentType
			{
				get { return typeof(object); }
			}

			public override object GetValue(object component)
			{
				return null;
			}

			public override bool IsReadOnly
			{
				get { return true; }
			}

			public override Type PropertyType
			{
				get { return typeof(object); }
			}

			public override void ResetValue(object component)
			{
			}

			public override void SetValue(object component, object value)
			{
			}

			public override bool ShouldSerializeValue(object component)
			{
				return false;
			}

			public override void AddValueChanged(object component, EventHandler handler)
			{
			}

			public override void RemoveValueChanged(object component, EventHandler handler)
			{
			}
		}

		#endregion

		#region Implementation

		bool hasGetMethod;
		bool hasSetMethod;
		bool hasGetSetMethodPopulated;
		int hash;

		PropertyDescriptor InnerOrNullActionProperty
		{
			get
			{
				return Inner ?? (Inner = new NullPropertyDescriptor(Name, AttributeArray));
			}
		}

		bool HasGetMethod
		{
			get
			{
				EnsureGetSetMethodPopulated();
				return hasGetMethod;
			}
		}

		bool HasSetMethod
		{
			get
			{
				EnsureGetSetMethodPopulated();
				return hasSetMethod;
			}
		}

		void EnsureGetSetMethodPopulated()
		{
			if (!hasGetSetMethodPopulated)
			{
				hasGetMethod = GetMethod != null;
				hasSetMethod = SetMethod != null;
				hasGetSetMethodPopulated = true;
			}
		}

		MethodInfo GetMethod
		{
			get
			{
				MethodInfo result = null;
				if (this.IsReflectPropertyDescriptor() && Inner != null && Inner.ComponentType != null && PropertyType != null)
				{
					var property = Inner.ComponentType.GetProperty(Inner.Name, PropertyType);
					result = property != null ? property.GetGetMethod(true) : null;
				}
				return result;
			}
		}

		MethodInfo SetMethod
		{
			get
			{
				MethodInfo result = null;
				if (this.IsReflectPropertyDescriptor() && Inner != null && Inner.ComponentType != null && PropertyType != null)
				{
					var property = Inner.ComponentType.GetProperty(Inner.Name, PropertyType);
					result = property != null ? property.GetSetMethod(true) : null;
				}
				return result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error reporting")]
		protected virtual void HandleException(object component, Exception ex)
		{
			var actualException = ex;
			if (ex is TargetInvocationException)
			{
				actualException = ex.InnerException;
			}
			if (actualException is TargetException && component != null)
			{
				var expectedComponent = ComponentType != null ? string.Format("component of type '{0}'", ComponentType.FullName) : "null component";
				throw new TargetException(string.Format("Property '{0}' expected {1} but got '{2}'", Name, expectedComponent, component.GetType().FullName), ex);
			}
		}

		#endregion
	}
}
