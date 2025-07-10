using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.Common.Collections;

namespace CargoWise.ComponentModel
{
	public interface IWrappingPropertyDescriptor
	{
		PropertyDescriptor Outer { get; }
	}

	/// <summary>
	/// Wraps a PropertyDescriptor with another PropertyDescriptor.
	/// Usually this is referenced from a PropertyDescriptorCollection with the '+' operator (such as 'Consignee+OH_FullName').
	/// </summary>
	public class WrappingPropertyDescriptor : KPropertyDescriptor, IWrappingPropertyDescriptor
	{
		public WrappingPropertyDescriptor(PropertyDescriptorCollectionWithWrappingProperties collection, PropertyDescriptor outer, PropertyDescriptor inner, string name)
			: base(collection, inner)
		{
			Argument.NotNull(outer, nameof(outer));
			Argument.NotNull(inner, nameof(inner));
			Outer = outer;
			this.name = name;
		}

		public new PropertyDescriptorCollectionWithWrappingProperties Collection
		{
			get { return base.Collection as PropertyDescriptorCollectionWithWrappingProperties; }
		}

		public PropertyDescriptor Outer
		{
			get
			{
				return outer;
			}
			private set
			{
				Argument.NotNull(value, nameof(value));
				outer = value;
			}
		}
		PropertyDescriptor outer;

		public override string Name
		{
			get { return this.name; }
		}
		readonly string name;

		public override Type ComponentType
		{
			get { return Outer.ComponentType; }
		}

		protected override object GetValueCore(object component)
		{
			object outerValue = Outer.GetValue(component);
			return base.GetValueCore(outerValue);
		}

		protected override void SetValueCore(object component, object value)
		{
			var outerValue = Outer.GetValue(component);
			var nullable = outerValue as INullable;
			if (nullable == null || !nullable.IsNull)
			{
				base.SetValueCore(outerValue, value);
			}
		}

		public override void AddValueChanged(object component, EventHandler handler)
		{
			EnsureComponentManager(component).AddValueChanged(handler);
		}

		public override void RemoveValueChanged(object component, EventHandler handler)
		{
			var manager = handlerManagers == null ? null : handlerManagers[component];
			if (manager != null)
			{
				manager.RemoveValueChanged(handler);
				if (manager.IsEmpty)
				{
					handlerManagers.Remove(component);
				}
			}
		}

		public override void FireChangeEvent(object component)
		{
			if (handlerManagers != null)
			{
				var manager = handlerManagers[component];
				if (manager != null)
				{
					manager.FireChangeEvent();
				}
			}
		}

		#region Wrapped Filtered Attributes

		public override AttributeCollection Attributes
		{
			[SuppressMessage("Microsoft.Contracts", "Nonnull-128-0")] //GetEnumerator()
			get
			{
				if (attributes == null)
				{
					var list = new List<Attribute>();
					if (Inner != null && Inner.Attributes != null)
					{
						foreach (Attribute attribute in Inner.Attributes)
						{
							if (!(attribute is BrowsableAttribute))
							{
								var innerAttribute = attribute as IFilteredAttributeForWrappingPropertyDescriptor;
								if (innerAttribute != null)
								{
									Attribute filteredAttribute = innerAttribute.GetAttributeOnOuterProperty(this);
									list.Add(filteredAttribute);
								}
								else
								{
									list.Add(attribute);
								}
							}
						}
					}
					list.Add(BrowsableAttribute.No);
					attributes = new AttributeCollection(list.ToArray());
				}
				return attributes;
			}
		}
		AttributeCollection attributes;

		protected override IEnumerable<Attribute> GetAttributesAllowMultipleCore(Type attributeType)
		{
			foreach (var attribute in base.GetAttributesAllowMultipleCore(attributeType))
			{
				var innerAttribute = attribute as IFilteredAttributeForWrappingPropertyDescriptor;
				if (innerAttribute != null)
				{
					yield return innerAttribute.GetAttributeOnOuterProperty(this);
				}
				else
				{
					yield return attribute;
				}
			}
		}

		#endregion

		#region EventHandlerManager

		EventHandlerManager EnsureComponentManager(object component)
		{
			if (handlerManagers == null)
			{
				handlerManagers = new WeakReferencedKeyDictionary<object, EventHandlerManager>();
			}
			var result = handlerManagers[component];
			if (result == null)
			{
				result = new EventHandlerManager(this, component);
				handlerManagers[component] = result;
			}
			return result;
		}

		class EventHandlerManager
		{
			public EventHandlerManager(WrappingPropertyDescriptor owner, object outerComponent)
			{
				Argument.NotNull(owner, nameof(owner)); // Suggested By ReviewBot 
				this.outerComponentRef.Target = outerComponent;
				this.innerComponentRef.Target = owner.Outer.GetValue(outerComponent);
				this.owner = owner;
			}

			public bool IsEmpty
			{
				get { return changeHandlers.IsEmpty; }
			}

			public void FireChangeEvent()
			{
				var componentTarget = outerComponentRef.Target;
				if (componentTarget != null && changeHandlers.Delegate != null)
				{
					changeHandlers.Delegate(componentTarget, EventArgs.Empty);
				}
			}

			public void AddValueChanged(EventHandler handler)
			{
				Argument.NotNull(handler, nameof(handler));
				if (handler.GetInvocationList() == null)
				{
					throw new ArgumentException("Invalid argument.", nameof(handler));
				}

				Hooked = true;
				changeHandlers.Add(handler);
			}

			public void RemoveValueChanged(EventHandler handler)
			{
				Argument.NotNull(handler, nameof(handler));
				if (handler.GetInvocationList() == null)
				{
					throw new ArgumentException("Invalid argument.", nameof(handler));
				}

				changeHandlers.Remove(handler);
				if (changeHandlers.IsEmpty)
				{
					Hooked = false;
				}
			}

			#region Implementation

			readonly WrappingPropertyDescriptor owner;
			readonly WeakReference outerComponentRef = new WeakReference(null);
			readonly WeakReference innerComponentRef = new WeakReference(null);
			readonly WeakTargetDelegateList<EventHandler> changeHandlers = new WeakTargetDelegateList<EventHandler>();

			bool Hooked
			{
				set
				{
					if (hooked != value)
					{
						var outerComponent = outerComponentRef.Target;
						var innerComponent = innerComponentRef.Target;
						if (value)
						{
							if (outerComponent != null)
							{
								owner.Outer.AddValueChanged(outerComponent, new EventHandler(OnOuterValueChanged));
								if (innerComponent != null && owner.Inner != null)
								{
									owner.Inner.AddValueChanged(innerComponent, new EventHandler(OnInnerValueChanged));
								}
							}
						}
						else
						{
							if (outerComponent != null)
							{
								owner.Outer.RemoveValueChanged(outerComponent, new EventHandler(OnOuterValueChanged));
								if (innerComponent != null && owner.Inner != null)
								{
									owner.Inner.RemoveValueChanged(innerComponent, new EventHandler(OnInnerValueChanged));
								}
							}
						}
						hooked = value;
					}
				}
			}
			bool hooked;

			void OnOuterValueChanged(object sender, EventArgs e)
			{
				var outerComponent = outerComponentRef.Target;
				if (outerComponent != null && owner.Inner != null)
				{
					var innerComponent = this.innerComponentRef.Target;
					if (innerComponent != null)
					{
						owner.Inner.RemoveValueChanged(innerComponent, new EventHandler(OnInnerValueChanged));
					}
					innerComponent = owner.Outer.GetValue(outerComponent);
					innerComponentRef.Target = innerComponent;
					if (innerComponent != null && owner.Inner != null)
					{
						owner.Inner.AddValueChanged(innerComponent, new EventHandler(OnInnerValueChanged));
					}
					OnValueChanged(outerComponent);
				}
			}

			void OnInnerValueChanged(object sender, EventArgs e)
			{
				OnValueChanged(outerComponentRef.Target);
			}

			void OnValueChanged(object component)
			{
				var handler = changeHandlers.Delegate;
				if (handler != null)
				{
					handler(component, EventArgs.Empty);
				}
			}

			#endregion
		}

		#endregion

		#region PropertyDescriptor Overrides

		public override bool CanResetValue(object component)
		{
			var outerValue = Outer.GetValue(component);
			return Inner != null && Inner.CanResetValue(outerValue);
		}

		public override void ResetValue(object component)
		{
			var outerValue = Outer.GetValue(component);
			if (Inner != null)
			{
				Inner.ResetValue(outerValue);
			}
		}

		public override bool ShouldSerializeValue(object component)
		{
			var outerValue = Outer.GetValue(component);
			return Outer.ShouldSerializeValue(outerValue);
		}

		#endregion

		WeakReferencedKeyDictionary<object, EventHandlerManager> handlerManagers;
	}
}
