using System;
using System.ComponentModel;

namespace CargoWise.ComponentModel
{
	internal class PositiveOrNeg1IntegerMetaDataType : MetaDataType
	{
		public PositiveOrNeg1IntegerMetaDataType(string id)
			: this(id, -1)
		{ }

		public PositiveOrNeg1IntegerMetaDataType(string id, int defaultValue)
			: base(id, typeof(int), defaultValue)
		{ }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
		public override string ValidateMetaDataValue(Type entityType, PropertyDescriptor prop, object value)
		{
			string result = null;
			if (value is int && ((int)value) < -1)
			{
				result = "Value must be either -1, zero or a positive number";
			}
			return result;
		}
	}
}
