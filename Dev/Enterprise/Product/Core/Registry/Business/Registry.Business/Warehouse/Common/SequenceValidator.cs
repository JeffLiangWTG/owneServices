using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Registry.Business.Warehouse
{
	public class SequenceValidator
	{
		public SequenceValidator(ZPropertyInfo[] propertyInfos, bool mustHaveAtLeastOneNonZeroValue = false)
		{
			PropertyInfos = propertyInfos;
			MustHaveAtLeastOneNonZeroValue = mustHaveAtLeastOneNonZeroValue;
		}

		#region Validate

		public void Validate(ZPropertyInfo propertyInfo)
		{
			CompareValidation.CheckWithinRange(propertyInfo, 0m, PropertyInfos.Length);

			if (!propertyInfo.HasErrors())
			{
				var value = (ZByte)propertyInfo.Value;
				if (value > 0)
				{
					CompareValidation.CheckValueIsNotDuplicated(propertyInfo, PropertyInfos);
				}
				else if (MustHaveAtLeastOneNonZeroValue && PropertyInfos.Sum(pi => (ZByte)pi.Value) == 0)
				{
					PropertyInfos.ForEach(pi => pi.AddError(Res.GetString("57701047-3e69-4988-bd01-4eecd04772e8", "There must be at least one non-zero value.")));
				}
				else
				{
					propertyInfo.AddWarning(Res.GetString("11d38d47-9eb7-40e0-9774-194394b9cf16", "This item is disabled because it has a value of zero."));
				}
			}
		}

		#endregion

		#region Implementation

		readonly ZPropertyInfo[] PropertyInfos;

		readonly bool MustHaveAtLeastOneNonZeroValue;

		#endregion
	}
}
