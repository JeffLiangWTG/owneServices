using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Registry.Business.Warehouse
{
	public class PutawaySequenceValidator
	{
		internal readonly IPutawaySequenceValidatorConsumer consumer;

		public PutawaySequenceValidator(IPutawaySequenceValidatorConsumer consumer)
		{
			this.consumer = consumer;
		}

		internal ZPropertyInfo[] GetLocationSortOrderPropertyInfos()
		{
			return new ZPropertyInfo[]
			{
				consumer.ColumnInfo,
				consumer.LevelInfo,
				consumer.RowInfo
			};
		}

		internal ZPropertyInfo[] GetPutawayAlgorithmSequencePropertyInfos()
		{
			return new ZPropertyInfo[]
			{
				consumer.ClientAreaInfo,
				consumer.LocationInfo,
				consumer.PickFaceInfo,
				consumer.ProductAreaInfo
			};
		}

		public void ValidateLocationSortOrder(ZPropertyInfo propertyInfo)
		{
			CompareValidation.CheckWithinRange(propertyInfo, 0m, 3m);

			if (!propertyInfo.HasErrors())
			{
				ZByte value = (ZByte)propertyInfo.Value;
				if (value > 0)
				{
					CompareValidation.CheckValueIsNotDuplicated(propertyInfo, GetLocationSortOrderPropertyInfos());
				}
				else
				{
					propertyInfo.AddWarning(Res.GetString("7ccbecd1-cf4a-47c7-830c-f68f906a898b", "This sort order is disabled because it has a value of zero."));
				}
			}
		}

		public void ValidatePutawayAlgorithmSequence(ZPropertyInfo propertyInfo)
		{
			bool isLocationInfo = (propertyInfo == consumer.LocationInfo);
			ZByte minValue = isLocationInfo ? (ZByte)1 : ZByte.Zero;
			CompareValidation.CheckWithinRange(propertyInfo, minValue, 4);

			if (!propertyInfo.HasErrors())
			{
				ZByte value = (ZByte)propertyInfo.Value;
				if (!value.IsEmpty)
				{
					CompareValidation.CheckValueIsNotDuplicated(propertyInfo, GetPutawayAlgorithmSequencePropertyInfos());
				}
				if (!isLocationInfo && !propertyInfo.HasErrors())
				{
					if (value.IsEmpty)
					{
						propertyInfo.AddWarning(Res.GetString("3ffb1f30-4b1f-4e12-b34b-2db29b9cd942", "This algorithm is disabled because it has a value of zero."));
					}
					else if (value > (ZByte)consumer.LocationInfo.Value)
					{
						propertyInfo.AddWarning(
							Res.GetString("927c6c8d-caa1-4d75-92ce-5dc3fa19843c", "This algorithm is disabled because it has a value higher than the {0}.",
							consumer.LocationInfo.HumanReadableName));
					}
				}
			}
		}
	}
}
