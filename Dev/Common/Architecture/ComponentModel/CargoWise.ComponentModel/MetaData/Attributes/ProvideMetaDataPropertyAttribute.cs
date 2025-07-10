using System;
using System.Reflection;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Apply this attribute to a class that provides generic meta-data property accessors in the
	/// form of methods.
	/// </summary>
	/// <remarks>
	/// For example, to specify methods that calculate the MaxLength meta-data
	/// property, define methods like this:<br/>
	/// <br/>
	/// <c>
	/// [ProvideMetaDataProperty("MaxLength", MetaDataTypes.MaxLength)]
	/// public class SomeClass
	/// {
	///		public int GetMaxLength(object instance, PropertyDescriptor prop)
	///		{ return instance.MaxLengthOfWhatever; }
	///		public int SetMaxLength(object instance, PropertyDescriptor prop, object value)
	///		{ instance.MaxLengthOfWhatever = value; }
	///	}
	///	</c>
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class ProvideMetaDataPropertyAttribute : Attribute
	{
		public ProvideMetaDataPropertyAttribute(string propertyName, string metaDataTypeId)
		{
			PropertyName = propertyName;
			MetaDataTypeId = metaDataTypeId;
		}

		/// <summary>
		/// Find this attribute on a type for a given meta data info identifier. Null is returned
		/// if it could not be found.
		/// </summary>
		public static ProvideMetaDataPropertyAttribute FindAttribute(MemberInfo type, string metaDataTypeId)
		{
			Argument.NotNull(type, nameof(type)); // Suggested By ReviewBot 
			ProvideMetaDataPropertyAttribute result = null;
			foreach (var attribute in type.GetCustomAttributes(typeof(ProvideMetaDataPropertyAttribute), true))
			{
				var metaAttribute = attribute as ProvideMetaDataPropertyAttribute;
				if (metaAttribute != null && metaAttribute.MetaDataTypeId == metaDataTypeId)
				{
					if (result != null)
					{
						throw new ArgumentException(
							"Cannot apply " + nameof(ProvideMetaDataPropertyAttribute) +
							" to a class more than once per meta-data type (" + metaDataTypeId);
					}
					result = metaAttribute;
				}
			}
			return result;
		}

		/// <summary>
		/// Get the name of the property we are describing.
		/// </summary>
		public string PropertyName { get; private set; }

		/// <summary>
		/// Get the unique identifier for the type of meta data that is being accessed.
		/// </summary>
		public string MetaDataTypeId { get; private set; }
	}
}
