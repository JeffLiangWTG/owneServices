using System;
using System.ComponentModel;
using System.Reflection;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// A property descriptor that uses explicit get and set methods on the instance of the object.
	/// <seealso cref="ProvideMetaDataPropertyAttribute"/>
	/// </summary>
	internal class MethodProvidedPropertyDescriptor : KPropertyDescriptor
	{
		readonly PropertyDescriptor property;
		readonly MetaDataType metaDataType;
		readonly MethodInfo getMethod;
		readonly MethodInfo setMethod;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="collection">The collection of the other properties on the component.</param>
		/// <param name="componentType">The type of the component this properties sits on.</param>
		/// <param name="propName">The name to give this property.</param>
		/// <param name="methodPropName">The name to get the methods, for example Get[methodPropName].</param>
		/// <param name="prop">The property that is having it's meta-data retrieved for.</param>
		/// <param name="metaDataType">The meta-data type this property is for.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error reporting")]
		public MethodProvidedPropertyDescriptor(KPropertyDescriptorCollection collection, Type componentType, string propertyName, string methodPropertyName, PropertyDescriptor property, MetaDataType metaDataType)
			: base(collection, propertyName, new Attribute[] { new BrowsableAttribute(false) })
		{
			Argument.NotNull(property, nameof(property));
			Argument.NotNull(collection, nameof(collection));
			Argument.NotNull(componentType, nameof(componentType));
			Argument.NotNull(metaDataType, nameof(metaDataType)); // Suggested By ReviewBot 
			this.property = property;
			this.metaDataType = metaDataType;

			getMethod = GetMethod(componentType, "Get" + methodPropertyName, new Type[] { typeof(PropertyDescriptor) });
			setMethod = GetMethod(componentType, "Set" + methodPropertyName, new Type[] { typeof(PropertyDescriptor), typeof(object) });

			if (getMethod == null)
			{
				var expectedComponentTypeName = ComponentType != null ? ComponentType.Name : "[ComponentType.Name is null]";
				throw new ArgumentException(string.Format("Cannot create a {0} without method Get{1}(PropertyDescriptor). You must implement this method on {2}.", GetType().Name, methodPropertyName, expectedComponentTypeName));
			}
			else if (getMethod.ReturnType != metaDataType.DataType)
			{
				var expectedDataTypeName = metaDataType.DataType != null ? metaDataType.DataType.Name : "[metaDataType.DataType is null]";
				throw new ArgumentException(string.Format("Method {0} must return {1} instead it returned {2}", this.getMethod.Name, expectedDataTypeName, getMethod.ReturnType.Name));
			}
		}

		protected override object GetValueCore(object component)
		{
			if (component is DBNull)
			{
				return metaDataType.DefaultValue;
			}
			else
			{
				return getMethod != null ? getMethod.Invoke(component, new object[] { property }) : null;
			}
		}

		protected override void SetValueCore(object component, object value)
		{
			if (setMethod != null)
			{
				setMethod.Invoke(component, new object[] { property, value });
			}
		}

		protected override bool HasSetterCore()
		{
			return false;
		}

		public override bool CanResetValue(object component)
		{
			return false;
		}

		public override Type ComponentType
		{
			get { return property.ComponentType; }
		}

		public override bool IsReadOnly
		{
			get { return setMethod != null; }
		}

		public override Type PropertyType
		{
			get { return metaDataType.DataType; }
		}

		public override void ResetValue(object component)
		{
			throw new NotSupportedException();
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

		#region Implementation

		// A more sophisticated version of GetMethod that allows arguments to be 'assignable'
		// instead of being exactly the types specified in the type array.
		static MethodInfo GetMethod(Type componentType, string methodName, Type[] argTypes)
		{
			Argument.NotNull(componentType, nameof(componentType)); // Suggested By ReviewBot 
			Argument.NotNull(argTypes, nameof(argTypes));
			MethodInfo result = null;
			foreach (var method in componentType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
			{
				if (methodName == method.Name && argTypes.Length == method.GetParameters().Length)
				{
					var found = true;
					for (var i = 0; i < method.GetParameters().Length; i++)
					{
						var param = method.GetParameters()[i];
						if (param != null && argTypes[i] != null && !argTypes[i].IsAssignableFrom(param.ParameterType))
						{
							found = false;
							break;
						}
					}
					if (found)
					{
						result = method;
						break;
					}
				}
			}
			return result;
		}

		#endregion
	}
}
