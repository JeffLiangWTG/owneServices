using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISPremisesIdAndProcessingTypeLookups : ZLookups
	{
		public AQISPremisesIdAndProcessingTypeLookups(AQISPremisesIdAndProcessingType parent)
			: base(parent)
		{
		}

		public BusinessObjectCollection AQISPremisesIdList
		{
			get
			{
				return Factory.GetCachedValue("AQISPremisesIdAndProcessingTypeLookups|AQISPremisesIdList", () =>
				{
					return CMRReferenceDataHelper.SetupAQISPremisesIdList(Factory);
				});
			}
		}

		public CodeDescriptionPairList AQISProcessingTypeList
		{
			get
			{
				var cargoType = GetDeclarationCargoType();
				return Factory.GetCachedValue("AQISProcessingTypeList_" + cargoType, () =>
				{
					var aqisProcessingTypeList = CMRReferenceDataHelper.SetupAQISProcessingTypeList(Factory, cargoType);
					aqisProcessingTypeList.SortByDescription();
					return aqisProcessingTypeList;
				});
			}
		}

		ZString GetDeclarationCargoType()
		{
			var cargoType = ZString.Empty;

			var declaration = ((AQISPremisesIdAndProcessingType)Parent).Declaration;
			if (declaration != null)
			{
				if (declaration.IsAir)
				{
					cargoType = AqisAepProcessingCategories.AIR;
				}
				else if (declaration.IsSea)
				{
					switch (declaration.JE_ContainerMode)
					{
						case Core.Constants.ContainerModes.BreakBulk:
							cargoType = AqisAepProcessingCategories.BreakBulk;
							break;
						case Core.Constants.ContainerModes.Bulk:
							cargoType = AqisAepProcessingCategories.Bulk;
							break;
						case Core.Constants.ContainerModes.FCL:
						case Core.Constants.ContainerModes.FCLMixedShipper:
							cargoType = AqisAepProcessingCategories.FCL;
							break;
						case Core.Constants.ContainerModes.LCL:
							cargoType = AqisAepProcessingCategories.LCL;
							break;
						case Core.Constants.ContainerModes.Liquid:
							cargoType = AqisAepProcessingCategories.Liquid;
							break;
					}
				}
			}

			return cargoType;
		}

		public static class AqisAepProcessingCategories
		{
			public const string AIR = "AIR";
			public const string BreakBulk = "B/B";
			public const string Bulk = "BLK";
			public const string FCL = "FCL";
			public const string LCL = "LCL";
			public const string Liquid = "LQD";
		}
	}
}
