using System;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Meta-data info for meta-data that must be defined on the property if the control requests it
	/// (ie no default value is available for it).
	/// </summary>
	public class MetaDataMandatoryType : MetaDataType
	{
		public MetaDataMandatoryType(string id, Type dataType, object defaultValue)
			: base(id, dataType, defaultValue)
		{
		}

		public MetaDataMandatoryType(string id, Type dataType, object defaultValue, string[] otherRequiredMetaDataIds)
			: base(id, dataType, defaultValue, otherRequiredMetaDataIds)
		{
			Argument.NotNull(otherRequiredMetaDataIds, nameof(otherRequiredMetaDataIds));
		}

		public override bool IsMandatory
		{
			get { return true; }
		}
	}
}
