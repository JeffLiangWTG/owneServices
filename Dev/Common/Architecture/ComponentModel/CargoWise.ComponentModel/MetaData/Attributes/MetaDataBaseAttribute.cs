using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// The base class for meta-data locator attributes.
	/// </summary>
	public abstract class MetaDataBaseAttribute :
		Attribute,
		IFilteredAttributeForWrappingPropertyDescriptor
	{
		/// <summary>
		/// Is the given meta-data type provided with a constant value?
		/// </summary>
		public abstract bool ProvidesMetaDataValue(string metaDataTypeId);

		/// <summary>
		/// Is the given meta-data type provided with a member property?
		/// </summary>
		public abstract bool ProvidesMetaDataMember(string metaDataTypeId);

		public bool ProvidesMetaData(string metaDataTypeId)
		{
			return ProvidesMetaDataValue(metaDataTypeId) || ProvidesMetaDataMember(metaDataTypeId);
		}

		/// <summary>
		/// Get the value of the given meta-data ID.
		/// </summary>
		public abstract object GetMetaDataValue(string metaDataTypeId);

		/// <summary>
		/// Get the member property that provides the value of the given meta-data ID.
		/// </summary>
		public abstract string GetMetaDataMember(string metaDataTypeId);

		/// <summary>
		/// A dictionary of string->Type (member name, expected value type) of any members that are referenced
		/// by this attribute.
		/// </summary>
		public virtual IDictionary<string, string> ReferencedMembers
		{
			get { return new Dictionary<string, string>(); }
		}

		#region IFilteredAttributeForWrappingPropertyDescriptor

		Attribute IFilteredAttributeForWrappingPropertyDescriptor.GetAttributeOnOuterProperty(WrappingPropertyDescriptor wrappingProperty)
		{
			var result = GetAttributeOnOuterProperty(wrappingProperty) ??
				throw new InvalidOperationException(
					"You must override and return an attribute of the same type as this from " +
					"method GetAttributeOnOuterProperty.");

			return result;
		}

		protected virtual Attribute GetAttributeOnOuterProperty(WrappingPropertyDescriptor wrappingProperty)
		{
			Argument.NotNull(wrappingProperty, nameof(wrappingProperty));
			return this;
		}

		#endregion
	}
}
