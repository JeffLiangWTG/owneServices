using System;
using System.ComponentModel;
using System.Globalization;

namespace CargoWise.ComponentModel
{
	internal class DescriptionMetaDataType : MetaDataType
	{
		public DescriptionMetaDataType(string id, object defaultValue)
			: base(id, typeof(IDescription), defaultValue)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error messages")]
		public override string ValidateMetaDataValue(Type entityType, PropertyDescriptor property, object value)
		{
			var result = base.ValidateMetaDataValue(entityType, property, value);
			if (result == null)
			{
				var description = (IDescription)value;
				if (description != null)
				{
					if (description.Count == 0)
					{
						result = "No description strings specified.";
					}
					else
					{
						var currentLength = 0;
						for (var i = 0; i < description.Count; i++)
						{
							var nextLength = description.GetDescription(i, CultureInfo.CurrentCulture).Length;
							if (nextLength <= currentLength)
							{
								result = "Description strings don't have consequtive lengths.";
								break;
							}
							currentLength = nextLength;
						}
					}
				}
			}
			return result;
		}
	}
}
