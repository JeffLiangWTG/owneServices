using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Apply this attribute to a property that provides a value for a given meta data item.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "Attribute is inherited")]
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class MetaDataMemberAttribute : SingleMetaDataAttribute
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="metaDataTypeID">The type name of the meta data item.</param>
		/// <param name="member">The property name that provides the value for the meta data.</param>
		public MetaDataMemberAttribute(string metaDataTypeId, string member)
			: base(metaDataTypeId)
		{
			Argument.NotNull(metaDataTypeId, nameof(metaDataTypeId));
			Argument.NotNull(member, nameof(member));
			this.member = member;
		}

		/// <summary>
		/// Get the property name that will return the value of the meta data.
		/// </summary>
		public string Member
		{
			get { return member; }
		}
		readonly string member;

		public override bool ProvidesMetaDataValue(string metaDataTypeId)
		{
			return false;
		}

		public override bool ProvidesMetaDataMember(string metaDataTypeId)
		{
			return metaDataTypeId == MetaDataTypeId;
		}

		public override object GetMetaDataValue(string metaDataTypeId)
		{
			return null;
		}

		public override string GetMetaDataMember(string metaDataTypeId)
		{
			if (metaDataTypeId == MetaDataTypeId)
			{
				return Member;
			}
			return null;
		}

		public override IDictionary<string, string> ReferencedMembers
		{
			get
			{
				var result = new Dictionary<string, string>();
				result[Member] = MetaDataTypeId;
				return result;
			}
		}

		protected override Attribute GetAttributeOnOuterProperty(WrappingPropertyDescriptor wrappingProperty)
		{
			return new MetaDataMemberAttribute(MetaDataTypeId, wrappingProperty.Outer.Name + "+" + Member);
		}
	}
}
