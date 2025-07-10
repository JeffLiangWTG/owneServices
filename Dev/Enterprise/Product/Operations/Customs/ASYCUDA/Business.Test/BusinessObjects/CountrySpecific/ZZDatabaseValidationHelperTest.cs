using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;
using universalAlias = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(ZZDatabaseValidationHelper))]
	sealed class ZZDatabaseValidationHelperTest : TestCaseWithFactory
	{
		[TestDate(2017, 7, 12)]
		public void TestGetNVCApplicationBusinessProviders()
		{
			AssertGetProviders(ZZDatabaseValidationHelper.GetNVCApplicationBusinessProviders,
				new[]
				{
					Core.Constants.CountryCodes.Bangladesh,
					Core.Constants.CountryCodes.Chile,
					Core.Constants.CountryCodes.Ireland,
					Core.Constants.CountryCodes.Singapore,
					Core.Constants.CountryCodes.SouthAfrica,
					Core.Constants.CountryCodes.UnitedKingdom,
					Core.Constants.CountryCodes.UnitedKingdom,
					Core.Constants.CountryCodes.UnitedStates,
					Core.Constants.CountryCodes.Uruguay,
					Core.Constants.CountryCodes.Taiwan,
					Core.Constants.CountryCodes.Turkey,

					Core.Constants.CountryCodes.Austria,
					Core.Constants.CountryCodes.Belgium,
					Core.Constants.CountryCodes.Bulgaria,
					Core.Constants.CountryCodes.Croatia,
					Core.Constants.CountryCodes.Cyprus,
					Core.Constants.CountryCodes.CzechRepublic,
					Core.Constants.CountryCodes.Denmark,
					Core.Constants.CountryCodes.Estonia,
					Core.Constants.CountryCodes.Guadeloupe,
					Core.Constants.CountryCodes.Reunion,
					Core.Constants.CountryCodes.Martinique,
					Core.Constants.CountryCodes.FrenchGuyana,
					Core.Constants.CountryCodes.Mayotte,
					Core.Constants.CountryCodes.SaintMartin,
					Core.Constants.CountryCodes.EuropeanUnion,
					Core.Constants.CountryCodes.Finland,
					Core.Constants.CountryCodes.France,
					Core.Constants.CountryCodes.Germany,
					Core.Constants.CountryCodes.Greece,
					Core.Constants.CountryCodes.Hungary,
					Core.Constants.CountryCodes.Iceland,
					Core.Constants.CountryCodes.Ireland,
					Core.Constants.CountryCodes.Italy,
					Core.Constants.CountryCodes.Latvia,
					Core.Constants.CountryCodes.Lithuania,
					Core.Constants.CountryCodes.Luxembourg,
					Core.Constants.CountryCodes.Malta,
					Core.Constants.CountryCodes.Netherlands,
					Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes,
					Core.Constants.CountryCodes.Norway,
					Core.Constants.CountryCodes.Poland,
					Core.Constants.CountryCodes.Portugal,
					Core.Constants.CountryCodes.Romania,
					Core.Constants.CountryCodes.Slovakia,
					Core.Constants.CountryCodes.Slovenia,
					Core.Constants.CountryCodes.Spain,
					Core.Constants.CountryCodes.Sweden,
					Core.Constants.CountryCodes.Switzerland,
				},
				ApplicationCodeTypeList.Codes.Consolidator);
		}

		[TestDate(2017, 7, 12)]
		public void TestGetVOCApplicationBusinessProviders()
		{
			AssertGetProviders(ZZDatabaseValidationHelper.GetVOCApplicationBusinessProviders,
				new[]
				{
					Core.Constants.CountryCodes.Ireland,
					Core.Constants.CountryCodes.Kazakhstan,
					Core.Constants.CountryCodes.SouthAfrica,
					Core.Constants.CountryCodes.UnitedKingdom,
					Core.Constants.CountryCodes.UnitedKingdom,
					Core.Constants.CountryCodes.Spain,
					Core.Constants.CountryCodes.Turkey,

					Core.Constants.CountryCodes.Austria,
					Core.Constants.CountryCodes.Belgium,
					Core.Constants.CountryCodes.Bulgaria,
					Core.Constants.CountryCodes.Croatia,
					Core.Constants.CountryCodes.Cyprus,
					Core.Constants.CountryCodes.CzechRepublic,
					Core.Constants.CountryCodes.Denmark,
					Core.Constants.CountryCodes.Estonia,
					Core.Constants.CountryCodes.Guadeloupe,
					Core.Constants.CountryCodes.Reunion,
					Core.Constants.CountryCodes.Martinique,
					Core.Constants.CountryCodes.FrenchGuyana,
					Core.Constants.CountryCodes.Mayotte,
					Core.Constants.CountryCodes.SaintMartin,
					Core.Constants.CountryCodes.EuropeanUnion,
					Core.Constants.CountryCodes.Finland,
					Core.Constants.CountryCodes.France,
					Core.Constants.CountryCodes.Germany,
					Core.Constants.CountryCodes.Greece,
					Core.Constants.CountryCodes.Hungary,
					Core.Constants.CountryCodes.Iceland,
					Core.Constants.CountryCodes.Ireland,
					Core.Constants.CountryCodes.Italy,
					Core.Constants.CountryCodes.Latvia,
					Core.Constants.CountryCodes.Lithuania,
					Core.Constants.CountryCodes.Luxembourg,
					Core.Constants.CountryCodes.Malta,
					Core.Constants.CountryCodes.Netherlands,
					Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes,
					Core.Constants.CountryCodes.Norway,
					Core.Constants.CountryCodes.Poland,
					Core.Constants.CountryCodes.Portugal,
					Core.Constants.CountryCodes.Romania,
					Core.Constants.CountryCodes.Slovakia,
					Core.Constants.CountryCodes.Slovenia,
					Core.Constants.CountryCodes.Spain,
					Core.Constants.CountryCodes.Sweden,
					Core.Constants.CountryCodes.Switzerland,
				},
				ApplicationCodeTypeList.Codes.ShippingLine);
		}

		[TestDate(2017, 7, 12)]
		public void TestIsMandatoryForOneCountryWhenTransportModeMatches()
		{
			Assert(!ZZDatabaseValidationHelper.IsMandatoryForOneCountryWhenTransportModeMatches(new BusinessObjectFactory(), Core.Constants.CountryCodes.SouthAfrica, "ZZDNAME", Core.Constants.TransportModes.Road));
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var zaCon = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, RefCusCodeListTypes.Codes.ManifestValidationRule, "ZZDNAME", "ZZDNAME", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(zaCon.PK, Core.Constants.TransportModes.Road);
			Factory.Save();
			Assert("True", ZZDatabaseValidationHelper.IsMandatoryForOneCountryWhenTransportModeMatches(new BusinessObjectFactory(), Core.Constants.CountryCodes.SouthAfrica, "ZZDNAME", Core.Constants.TransportModes.Road));
		}

		void AssertGetProviders(Func<BusinessObjectFactory, IEnumerable<ApplicationBusinessProvider>> getFunc, string[] expectedCountryCodes, string manifestStyle)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			var bdCountry = helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Bangladesh, Core.Constants.CountryCodes.Bangladesh, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(bdCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, ZString.Empty);

			var fjCountry = helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Kazakhstan, Core.Constants.CountryCodes.Kazakhstan, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(fjCountry.PK, RefCusCodeListAttributeTypes.Codes.VOC, ZString.Empty);

			var vuCountry = helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Vanuatu, Core.Constants.CountryCodes.Vanuatu, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var usCountry = helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(usCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, ZString.Empty);
			helper.CreateCusCodeListAttribute(usCountry.PK, RefCusCodeListAttributeTypes.Codes.VOC, ZString.Empty);

			var enableGlobalManifest = (RegistryItemWrapper)ObjectFactory.Get<Integration.Customs.CL.ICLCustomsRegistry>().EnableGlobalManifest;
			enableGlobalManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.Save();

			var providers = getFunc(Factory).ToList();
			AssertContainsExactElementsInAnyOrder(expectedCountryCodes, providers.SelectMany(x => x.ApplicableCountryCodes(MasterFiles.Business.Directions.Unknown, ZString.Empty, manifestStyle)));
		}

		public void TestCheckIsMandatoryFor()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var validationHelper = new ZZDatabaseValidationHelperForTesting(header);
			var propertyInfo = header.AMA_ManifestTypeInfo;

			using (header.SuspendValidationTesting())
			{
				foreach (var ruleCode in GetAllRuleCodes())
				{
					propertyInfo.ClearAllNotifications();
					validationHelper.CheckIsMandatoryFor(propertyInfo, ruleCode);

					if (ExpectedMandatoryFields.ContainsKey(ruleCode))
					{
						AssertHasMessageError(propertyInfo, ExpectedMandatoryFields[ruleCode]);
					}
					else
					{
						AssertNoNotifications(propertyInfo);
					}
				}
			}
		}

		IEnumerable<string> GetAllRuleCodes() => typeof(ManifestValidationRuleCodes).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => (string)f.GetValue(null));

		public void TestIsMandatoryForTransportMode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var validationHelper = new ZZDatabaseValidationHelperForTesting(header);
			var propertyInfo = header.AMA_ManifestTypeInfo;
			var estimatedDepartureTime = ManifestValidationRuleCodes.EstimatedDepartureTime;

			using (header.SuspendValidationTesting())
			{
				propertyInfo.ClearAllNotifications();
				validationHelper.CheckIsMandatoryForOneCountryWhenTransportModeMatches(propertyInfo, estimatedDepartureTime, TransportTypeList.Codes.Air);
				AssertHasMessageError(propertyInfo, ExpectedMandatoryFields[estimatedDepartureTime]);
				propertyInfo.ClearAllNotifications();
				validationHelper.CheckIsMandatoryForOneCountryWhenTransportModeMatches(propertyInfo, estimatedDepartureTime, TransportTypeList.Codes.Sea);
				AssertHasMessageError(propertyInfo, ExpectedMandatoryFields[estimatedDepartureTime]);
				propertyInfo.ClearAllNotifications();
				validationHelper.CheckIsMandatoryForOneCountryWhenTransportModeMatches(propertyInfo, estimatedDepartureTime, TransportTypeList.Codes.Road);
				AssertNoMessageError(propertyInfo, ExpectedMandatoryFields[estimatedDepartureTime]);

				foreach (var ruleCode in GetAllRuleCodes().Except(ManifestValidationRuleCodes.EstimatedDepartureTime))
				{
					propertyInfo.ClearAllNotifications();
					validationHelper.CheckIsMandatoryForOneCountryWhenTransportModeMatches(propertyInfo, ruleCode, TransportTypeList.Codes.Air);

					if (ExpectedMandatoryFields.ContainsKey(ruleCode))
					{
						AssertHasMessageError(propertyInfo, ExpectedMandatoryFields[ruleCode]);
					}
					else
					{
						AssertNoNotifications(propertyInfo);
					}
				}
			}
		}

		Dictionary<string, string> ExpectedMandatoryFields => new()
		{
			{ ManifestValidationRuleCodes.OfficeCode, "A Customs Office Code is required" },
			{ ManifestValidationRuleCodes.EstimatedDepartureTime, "An Estimated Departure Time is required" },
			{ ManifestValidationRuleCodes.ShipmentType, "A Shipment Type is required" },
			{ ManifestValidationRuleCodes.CurrentUserEmailAddress, "An email address is required for the current user" },
			{ ManifestValidationRuleCodes.Nature, "A Nature is required" },
			{ ManifestValidationRuleCodes.FinalDestination, "A Final Destination is required" },
		};
	}

	class ZZDatabaseValidationHelperForTesting : ZZDatabaseValidationHelper
	{
		public ZZDatabaseValidationHelperForTesting(AsycudaManifestHeader header) : base(header)
		{
		}

		protected internal override bool GetMandatoryFieldsInZZ => false;

		protected override Dictionary<string, MandatoryValidationRule> GetMandatoryFieldsCore()
		{
			var mandatoryFields = base.GetMandatoryFieldsCore();
			mandatoryFields[ManifestValidationRuleCodes.Consignee].NeedsToCheck = () => false;
			mandatoryFields[ManifestValidationRuleCodes.EstimatedDepartureTime].TransportModes = new[] { TransportTypeList.Codes.Air, TransportTypeList.Codes.Sea };
			return mandatoryFields;
		}
	}
}
