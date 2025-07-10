using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	public class UnitMeasurementTextOverrideValidation
	{
		public UnitMeasurementTextOverrideValidation(UnitMeasurementTextOverride parent)
		{
			Parent = parent;
		}

		readonly UnitMeasurementTextOverride Parent;

		public void ValidateUnitMeasurement()
		{
			MandatoryValidation.CheckEntered(Parent.UnitMeasurementInfo);
			ListValidation.ErrorIfInvalidCode(Parent.UnitMeasurementInfo);
			if (!Parent.UnitMeasurementInfo.HasErrors())
			{
				if (Parent.ParentCollections.Count > 0 && !Parent.UnitMeasurement.IsEmpty)
				{
					PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.UnitMeasurementInfo,
						string.Format(Res.GetString("67500D59-FC1F-468D-B004-3C3C933C299C", "The unit measurement override of '{0}' already exists.", Parent.UnitMeasurement)));
				}
			}
		}

		public void ValidateTextOverride()
		{
			MandatoryValidation.CheckEntered(Parent.TextOverrideInfo);
		}
	}
}
