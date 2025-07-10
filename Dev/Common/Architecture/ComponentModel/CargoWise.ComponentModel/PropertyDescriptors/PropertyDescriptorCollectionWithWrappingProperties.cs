using System;
using System.ComponentModel;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Collection of properties that lazy-creates DWrappingPropertyDescriptors as well as the normal ones.
	/// NOTE: foreaching over a variable of type PropertyDescriptorCollectionWithWrappingProperties excludes wrapped properties,
	///		  where foreaching over a variable of type PropertyDescriptorCollection includes wrapped properties.
	/// </summary>
	public class PropertyDescriptorCollectionWithWrappingProperties : PropertyDescriptorCollectionWithMetaData
	{
		protected PropertyDescriptorCollectionWithWrappingProperties(Type componentType, bool includePrivate)
			: base(componentType, includePrivate)
		{
			Argument.NotNull(componentType, nameof(componentType));
		}

		public new static PropertyDescriptorCollectionWithWrappingProperties FromType(Type componentType)
		{
			Argument.NotNull(componentType, nameof(componentType));
			return FromType(componentType, false);
		}

		public new static PropertyDescriptorCollectionWithWrappingProperties FromType(Type componentType, bool includePrivate)
		{
			Argument.NotNull(componentType, nameof(componentType));
			return factory.FromType(componentType, includePrivate);
		}

		static readonly PropertyDescriptorCollectionFactory<PropertyDescriptorCollectionWithWrappingProperties> factory = new PropertyDescriptorCollectionFactory<PropertyDescriptorCollectionWithWrappingProperties>(delegate(Type componentType)
		{
			return new PropertyDescriptorCollectionWithWrappingProperties(componentType, false);
		});

		protected override PropertyDescriptor FindOrCreatePropertyNotInCollection(string name, bool ignoreCase)
		{
			var result = base.FindOrCreatePropertyNotInCollection(name, ignoreCase);
			if (result == null)
			{
				int separatorIndex = name.IndexOf('+');
				if (separatorIndex != -1)
				{
					string outerName = name.Substring(0, separatorIndex);
					string innerName = name.Substring(separatorIndex + 1);
					var outer = this.Find(outerName, ignoreCase);
					if (outer != null)
					{
						var inner = GetInnerPropertyDescriptorForWrapped(outer, innerName, ignoreCase);
						if (inner != null)
						{
							result = NewWrappingPropertyDescriptor(this, outer, inner, name);
						}
					}
				}
			}
			return result;
		}

		protected virtual PropertyDescriptor GetInnerPropertyDescriptorForWrapped(PropertyDescriptor outer, string innerName, bool ignoreCase)
		{
			Argument.NotNull(outer, nameof(outer)); // Suggested By ReviewBot 
			var children = outer.GetChildProperties();
			if (children != null)
			{
				return children.Find(innerName, ignoreCase);
			}
			return null;
		}

		protected virtual WrappingPropertyDescriptor NewWrappingPropertyDescriptor(PropertyDescriptorCollectionWithWrappingProperties collection, PropertyDescriptor outer, PropertyDescriptor inner, string name)
		{
			Argument.NotNull(inner, nameof(inner));
			Argument.NotNull(outer, nameof(outer));
			return new WrappingPropertyDescriptor(this, outer, inner, name);
		}

		protected override KPropertyDescriptorCollection NewEmptyPropertyDescriptorCollectionCore(Type componentType, bool includePrivate)
		{
			return new PropertyDescriptorCollectionWithWrappingProperties(componentType, includePrivate);
		}

		protected override KPropertyDescriptorCollection GetPropertyDescriptorCollectionForComponentTypeCore(Type componentType, bool includePrivate)
		{
			return FromType(componentType, includePrivate);
		}

		protected override bool ShouldEnumerateOver(PropertyDescriptor property)
		{
			var name = property.Name;
			return base.ShouldEnumerateOver(property) && !name.Contains("+");
		}
	}
}
