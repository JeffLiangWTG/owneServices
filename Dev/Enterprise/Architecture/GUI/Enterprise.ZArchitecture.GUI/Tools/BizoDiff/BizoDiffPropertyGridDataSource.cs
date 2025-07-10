using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.DevTools
{
	public class BizoDiffPropertyGridDataSource
	{
		public BizoDiffPropertyGridDataSource(BusinessObject sourceBizObj, BusinessObject targetBizObj, BizoDiffColumnProvider columnProvider, bool hideSameValue)
		{
			SourceBizo = sourceBizObj;
			TargetBizo = targetBizObj;
			ColumnProvider = columnProvider;
			HideSameValue = hideSameValue;
		}

		readonly BusinessObject SourceBizo;
		readonly BusinessObject TargetBizo;
		readonly BizoDiffColumnProvider ColumnProvider;
		readonly bool HideSameValue;

		public BizoDiffPropertyCollection SourceProperties { get; set; }
		public BizoDiffPropertyCollection TargetProperties { get; set; }

		public void BuildCollections()
		{
			SourceProperties = BuildCollectionCore(SourceBizo, TargetBizo);
			TargetProperties = BuildCollectionCore(TargetBizo, SourceBizo);
		}

		BizoDiffPropertyCollection BuildCollectionCore(BusinessObject bizo, BusinessObject compareBizo)
		{
			var collection = new BizoDiffPropertyCollection();

			if (bizo == null || bizo is IBusinessObjectCollection)
			{
				return collection;
			}

			var propNames = ColumnProvider.GetPropertyList(bizo);
			var isSameType = compareBizo != null && bizo.GetType() == compareBizo.GetType();

			foreach (var propertyName in propNames)
			{
				var value = ((INeedRow)bizo).Row[propertyName];

				if (isSameType)
				{
					var compareValue = ((INeedRow)compareBizo).Row[propertyName];

					var property = new BizoProperty(propertyName, value, compareValue);
					if (HideSameValue && property.IsSameValue)
					{
						continue;
					}

					collection.Add(property);
				}
				else
				{
					collection.Add(new BizoProperty(propertyName, value));
				}
			}

			return collection;
		}
	}
}
