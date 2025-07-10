using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISProducerCodeLookups : ZLookups
	{
		public AQISProducerCodeLookups(AQISProducerCode parent)
			: base(parent)
		{
		}

		public IBusinessObjectCollection AQISProducerCodeList => CMRReferenceDataHelper.UseReferenceData ? RefDatabaseAQISProducerCodeList : CMRAqisProducerCodeList;

		public static ModuleIdentifier AQISProducerModuleId => CMRReferenceDataHelper.UseReferenceData
						? Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList
						: Enterprise.ZArchitecture.Modules.ModuleIDs.AQISProducerCode;

		public BusinessObjectCollection CMRAqisProducerCodeList
		{
			get { return new CMRAqisProducerCollection(Factory); }
		}

		public BusinessObjectCollection RefDatabaseAQISProducerCodeList
		{
			get
			{
				return Factory.GetCachedValue("AQISProducerCodeLookups.RefDatabaseAQISProducerCodeList", () =>
				{
					var fAQISProducerCodeList = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRPR, ZDateTime.Today);

					fAQISProducerCodeList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.ZZRefCusCodeListFilters.ListType, "Property", (ZString)AUConstants.RefCusCodeTypeCodes.CMRPR, false));
					fAQISProducerCodeList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.ZZRefCusCodeListFilters.AttributeName, "Property", ZString.Empty, true));
					fAQISProducerCodeList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.ZZRefCusCodeListFilters.AttributeValue, "Property", ZString.Empty, true));
					return fAQISProducerCodeList;
				});
			}
		}
	}
}
