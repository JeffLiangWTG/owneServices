using System;
using System.ComponentModel;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// A property descriptor that always returns the given constant value.
	/// </summary>
	public class ConstantValuePropertyDescriptor : KPropertyDescriptor
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="collection">The collection of the other properties on the component.</param>
		/// <param name="propName">The name of this property.</param>
		/// <param name="componentType">The type of the component this property is on.</param>
		/// <param name="constantValue">The value that is always returned from GetValue().</param>
		public ConstantValuePropertyDescriptor(KPropertyDescriptorCollection collection, string propertyName, Type componentType, object constantValue)
			: base(collection, propertyName, new Attribute[] { new BrowsableAttribute(false) })
		{
			Argument.NotNull(constantValue, nameof(constantValue));
			this.componentType = componentType;
			ConstantValue = constantValue;
		}

		public object ConstantValue { get; private set; }

		protected override object GetValueCore(object component)
		{
			return ConstantValue;
		}

		protected override void SetValueCore(object component, object value)
		{
		}

		public override bool CanResetValue(object component)
		{
			return false;
		}

		public override Type ComponentType
		{
			get { return componentType; }
		}
		readonly Type componentType;

		public override bool IsReadOnly
		{
			get { return true; }
		}

		protected override bool HasSetterCore()
		{
			return false;
		}

		public override Type PropertyType
		{
			get { return ConstantValue.GetType(); }
		}

		public override void ResetValue(object component)
		{
			throw new NotSupportedException();
		}

		public override bool ShouldSerializeValue(object component)
		{
			return false;
		}
	}
}
