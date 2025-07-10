using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.ResourceStrings.Cache;

namespace CargoWise.EntityFramework
{
	public sealed class LinkedTranslatableDataFieldAttribute : SingleMetaDataAttribute
	{
		public LinkedTranslatableDataFieldAttribute(Type type, string property)
			: this(type, property, false)
		{ }

		public LinkedTranslatableDataFieldAttribute(Type type, string property, bool skipValidation)
			: base(TranslatableDataFieldAttribute.CDRSMetaDataTypeId)
		{
			attribute = type.GetProperty(property).GetCustomAttributes(typeof(TranslatableDataFieldAttribute), true).FirstOrDefault() as TranslatableDataFieldAttribute;
			this.skipValidation = skipValidation;
		}

		internal readonly TranslatableDataFieldAttribute attribute;
		internal readonly bool skipValidation;

		public override bool ProvidesMetaDataValue(string metaDataTypeId)
		{
			return metaDataTypeId == MetaDataTypeId;
		}

		public override object GetMetaDataValue(string metaDataTypeId)
		{
			return new CustomizableDataResourceStrings(attribute);
		}

		public override bool ProvidesMetaDataMember(string metaDataTypeId)
		{
			return false;
		}

		public override string GetMetaDataMember(string metaDataTypeId)
		{
			throw new NotImplementedException();
		}
	}
}
