using System;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Describes a meta-data item that can only be expressed by a constant. You cannot calculate this value
	/// from a property or similar.
	/// </summary>
	internal class ConstantOnlyMetaDataType : MetaDataType
	{
		public ConstantOnlyMetaDataType(string id, Type dataType, object defaultValue)
			: base(id, dataType, defaultValue)
		{
		}

		public override bool AllowConstantValueOnly
		{
			get { return true; }
		}
	}
}
