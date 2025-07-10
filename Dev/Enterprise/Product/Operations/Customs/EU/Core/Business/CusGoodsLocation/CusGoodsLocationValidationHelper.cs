using CargoWise.Common;

namespace Enterprise.Customs.EU.Business
{
	public static class CusGoodsLocationValidationHelper
	{
		public static string YouHaveNotEnteredMessageWithQualifier(string propertyDescriptor, string qualifier, string ruleNumber, bool includeRuleCode = true)
		{
			if (includeRuleCode)
			{
				return Res.GetString("9B3469A5-3F52-4881-BABE-8F47BCC8107B", "[{0}] {1} required when qualifier is '{2}'.", ruleNumber, propertyDescriptor, qualifier);
			}
			else
			{
				return Res.GetString("4589B875-D36B-4E8A-B13E-8BB77AFAB565", "{0} required when qualifier is '{1}'.", propertyDescriptor, qualifier);
			}
		}

		public static void ValidateInnerGoodsLocation(ICusGoodsLocationProvider locationProvider)
		{
			Argument.NotNull(locationProvider, nameof(locationProvider));

			if (locationProvider.GoodsLocation is CusGoodsLocation location)
			{
				locationProvider.GoodsLocationDescriptionInfo.AddNotificationBasedOnChildValidationStatus(
					Res.GetString("5C8B5D92-2421-492B-B01B-5D9D8F70E1CA", "There are errors within the 'Location of Goods', please click on 'More..' to view the error information."),
					() =>
					{
						location.Validation.ValidateAll();
						location.Address.Validation.ValidateAll();
					},
					location);
			}
		}
	}
}
