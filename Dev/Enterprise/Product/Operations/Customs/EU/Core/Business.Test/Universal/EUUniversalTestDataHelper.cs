using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Testing
{
	public static class EUUniversalTestDataHelper
	{
		public static void CreateSupportingDocumentCodeLists(this BusinessObjectFactory factory, params TestSupportingDocumentCodeList[] codeLists)
		{
			SetEuropeanUnionAsParentIfRequired(factory);

			var helper = new UniversalReferenceTestDataHelper(factory);
			foreach (var codeList in codeLists)
			{
				var codeType = codeList.IsImport ? Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection : Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
				var attributes = new Dictionary<string, string[]>();
				if (codeList.HasPermitAttribute)
				{
					attributes.Add(RefCusCodeListAttributeTypes.Codes.Permit, new[] { string.Empty });
				}
				helper.CreateCusCodeListsForMultipleTypesWithAttributes(codeList.Country, new[] { codeType }, codeList.Code, codeList.Code, attributes, ZDateTime.Today, ZDateTime.Today.AddDays(1));
			}
		}

		static void SetEuropeanUnionAsParentIfRequired(BusinessObjectFactory factory)
		{
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			if (ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(currentCountryCode))
			{
				var helper = new UniversalReferenceTestDataHelper(factory);
				var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
				helper.CreateNewOrGetExistingDataGrouping(currentCountryCode, currentCountryCode, eun);
			}
		}

		public static void SetUpTestRefData(BusinessObjectFactory factory, RefDataConfig config)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			foreach (var dataGrouping in config.DataGroupings)
			{
				RefDataGrouping parentDataGrouping = null;
				if (!dataGrouping.Parent.IsNullOrEmpty())
				{
					parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(dataGrouping.Parent);
				}
				helper.CreateNewOrGetExistingDataGrouping(dataGrouping.Code, dataGrouping.Description, parentDataGrouping);
			}

			foreach (var cusCodeType in config.CusCodeTypes)
			{
				helper.CreateNewOrGetExistingCusCodeType(cusCodeType.TypeCode, cusCodeType.Description);
				foreach (var attributeName in cusCodeType.AttributeTypes)
				{
					if (!string.IsNullOrEmpty(attributeName.Name))
					{
						helper.CreateNewOrGetExistingRefCusCodeListAttributeName(attributeName.Name, $"{attributeName.Name} Desc.", cusCodeType.TypeCode, attributeName.DataGrouping);
					}
				}
				foreach (var cusCode in cusCodeType.CusCodes)
				{
					var code = helper.CreateCusCodeList(cusCode.DataGrouping, cusCodeType.TypeCode, cusCode.Code, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
					foreach (var attribute in cusCode.Attributes)
					{
						if (!string.IsNullOrEmpty(attribute.Name))
						{
							code.Attributes.AddNew(attribute.Name, attribute.Value);
						}
					}
				}
			}

			foreach (var mapType in config.MapTypes)
			{
				helper.CreateCusMapType(mapType.Type, MapDirectionList.Codes.BTH, $"{mapType.Type} DESC.", false);
			}

			foreach (var map in config.Maps)
			{
				helper.CreateCusMap(map.Type, map.Cw1Value, map.CustomsValue, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, map.DataGrouping);
			}

			factory.Save();
		}
	}
}
