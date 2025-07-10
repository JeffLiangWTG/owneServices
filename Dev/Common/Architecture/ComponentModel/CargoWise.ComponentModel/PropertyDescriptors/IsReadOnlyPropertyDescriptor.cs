using System;
using System.Collections;
using System.ComponentModel;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// A PropertyDescriptor that always returns inner.IsReadOnly, where inner is passed into the
	/// constructor.
	/// </summary>
	public class IsReadOnlyPropertyDescriptor : KPropertyDescriptor
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="collection">The collection of the other properties on the component.</param>
		/// <param name="propertyName">The new name to give this PropertyDescriptor.</param>
		/// <param name="propertyToBeReadOnly">The property that is described to be read-only by this property.</param>
		/// <param name="innerMetaDataProp">The property that is being wrapped by this property.</param>
		public IsReadOnlyPropertyDescriptor(KPropertyDescriptorCollection collection, string propertyName, PropertyDescriptor propertyToBeReadOnly, PropertyDescriptor innerMetaDataProperty)
			: base(collection, GetInnerMetaDataPropertyOrConstantProperty(collection, propertyName, propertyToBeReadOnly.ComponentType, innerMetaDataProperty, false))
		{
			Argument.NotNull(collection, nameof(collection));
			Argument.NotNull(propertyName, nameof(propertyName));
			Argument.NotNull(propertyToBeReadOnly, nameof(propertyToBeReadOnly)); // Suggested By ReviewBot 

			name = propertyName;
			PropertyToBeReadOnly = propertyToBeReadOnly;
			InnerMetaDataProperty = innerMetaDataProperty;
		}

		public override string Name
		{
			get
			{
				return name;
			}
		}
		readonly string name;

		protected virtual bool IsPropertyTypeReadOnlyWithoutSetter()
		{
			return typeof(IList).IsAssignableFrom(PropertyToBeReadOnly.PropertyType);
		}

		protected override object GetValueCore(object component)
		{
			return IsConstant ? ConstantValue : Convert(base.GetValueCore(component));
		}

		public override bool CanResetValue(object component)
		{
			return false;
		}

		public override bool IsReadOnly
		{
			get { return true; }
		}

		public override Type PropertyType
		{
			get { return typeof(bool); }
		}

		/// <summary>
		/// The PropertyDescriptor that describes the property that may be read-only.
		/// </summary>
		protected PropertyDescriptor PropertyToBeReadOnly { get; private set; }

		/// <summary>
		/// Get the associated meta-data property.
		/// </summary>
		protected PropertyDescriptor InnerMetaDataProperty { get; private set; }

		#region Implementation

		protected static bool Convert(object readOnly)
		{
			var result = readOnly;
			if (!(result is bool) && result != null)
			{
				var converter = TypeDescriptor.GetConverter(result.GetType());
				result = converter.ConvertTo(result, typeof(bool));
			}
			return result != null && (bool)result;
		}

		bool IsConstant
		{
			get
			{
				CalculateConstantReadOnlyIfRequired();
				return isConstant;
			}
		}
		bool isConstant;

		bool ConstantValue
		{
			get
			{
				CalculateConstantReadOnlyIfRequired();
				return constantValue;
			}
		}
		bool constantValue;

		void CalculateConstantReadOnlyIfRequired()
		{
			if (!isConstantValueCalculated)
			{
				var hasSetter = PropertyToBeReadOnly.HasSetter();
				var readonlyDueToNoSetter = (!hasSetter && !IsPropertyTypeReadOnlyWithoutSetter());

				var constantInnerProperty = InnerMetaDataProperty as ConstantValuePropertyDescriptor;
				if (readonlyDueToNoSetter || constantInnerProperty != null)
				{
					this.isConstant = true;
					this.constantValue = readonlyDueToNoSetter || (bool)constantInnerProperty.ConstantValue;
				}
				isConstantValueCalculated = true;
			}
		}
		bool isConstantValueCalculated;

		protected static PropertyDescriptor GetInnerMetaDataPropertyOrConstantProperty(KPropertyDescriptorCollection collection, string propertyName, Type componentType, PropertyDescriptor innerMetaDataProperty, bool defaultValue)
		{
			Argument.NotNull(collection, nameof(collection));
			var result = innerMetaDataProperty ?? new ConstantValuePropertyDescriptor(collection, propertyName, componentType, defaultValue);
			return result;
		}

		#endregion
	}
}
