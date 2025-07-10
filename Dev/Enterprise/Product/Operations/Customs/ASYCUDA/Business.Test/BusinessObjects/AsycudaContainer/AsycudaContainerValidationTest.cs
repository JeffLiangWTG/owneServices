using System;
using System.Diagnostics;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSealsRelationships()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "XX";
			var container = header.Containers.AddNew();
			container.ACN_Seal1 = "X";
			AssertNoMessageErrorContaining(container.ACN_Seal1Info, "seal");
			AssertNoMessageErrorContaining(container.ACN_Seal2Info, "seal");
			AssertNoMessageErrorContaining(container.ACN_Seal3Info, "seal");
			container.ACN_Seal2 = "Y";
			AssertNoMessageErrorContaining(container.ACN_Seal1Info, "seal");
			AssertNoMessageErrorContaining(container.ACN_Seal2Info, "seal");
			AssertNoMessageErrorContaining(container.ACN_Seal3Info, "seal");
			container.ACN_Seal1 = "";
			AssertHasMessageErrorContaining(container.ACN_Seal1Info, "seal");
			AssertNoMessageErrorContaining(container.ACN_Seal2Info, "seal");
			AssertNoMessageErrorContaining(container.ACN_Seal3Info, "seal");
		}

		public void TestCheckACN_ContainerNumber()
		{
			var container = Factory.New<AsycudaContainer>();
			container.ACN_ContainerNumber = "";
			AssertHasMessageError(container.ACN_ContainerNumberInfo, MandatoryValidation.YouHaveNotEnteredMessage("Container Number"));

			container.ACN_ContainerNumber = "123456123456";
			AssertNoMessageError(container.ACN_ContainerNumberInfo, MandatoryValidation.YouHaveNotEnteredMessage("Container Number"));
			AssertHasWarningContaining(container.ACN_ContainerNumberInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");

			container.ACN_ContainerNumber = "QWER1234561";
			AssertNoWarningContaining(container.ACN_ContainerNumberInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");
			AssertHasWarningContaining(container.ACN_ContainerNumberInfo, "Container number does not have a valid check (last) digit.");

			container.ACN_ContainerNumber = ZString.Empty;
			AssertNoErrors("Container number can be empty, not expecting errors", container.ACN_ContainerNumberInfo);
		}

		public void TestCheckACN_ContainerNumber_NoPackLines()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "ABCD1234560";

			AssertHasWarning(container.ACN_ContainerNumberInfo, "This container does not appear on any pack lines.");

			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.SetContainer(container.ACN_ContainerNumber, header);
			container.Validation.ValidateACN_ContainerNumber();

			AssertNoWarning(container.ACN_ContainerNumberInfo, "This container does not appear on any pack lines.");
		}

		[DeveloperOnlyTest]
		public void TestCheckACN_ContainerNumber_NoPackLines_Performance()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var containers = new AsycudaContainer[100];

			for (var i = 0; i < 100; i++)
			{
				var container = header.Containers.AddNew();
				container.ACN_ContainerNumber = "ABCD1234" + i.ToString("D3");
				containers[i] = container;
			}

			for (var i = 0; i < 100; i++)
			{
				var bill = header.Bills.AddNew();
				for (var j = 0; j < 100; j++)
				{
					var pack = bill.Packs.AddNew();
					pack.SetContainer(containers[i].ACN_ContainerNumber, header);
				}
			}
			var containerToTest = header.Containers.AddNew();
			containerToTest.ACN_ContainerNumber = "HIGK1234560";
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			foreach (var c in containers)
			{
				c.Validation.ValidateACN_ContainerNumber();
			}
			containerToTest.Validation.ValidateACN_ContainerNumber();
			stopwatch.Stop();

			AssertLessThan("validation should cost no more than 3 seconds", stopwatch.ElapsedMilliseconds, 3000);
		}

		public void TestCheckACN_RC_ContainerType()
		{
			var container = Factory.New<AsycudaContainer>();
			container.ACN_RC_ContainerType = ZGuid.NewZGuid();
			AssertHasErrorContaining(container.ACN_RC_ContainerTypeInfo, "valid");

			container.ACN_RC_ContainerType = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			AssertNoMessageErrorContaining(container.ACN_RC_ContainerTypeInfo, "Container Type");

			container.ACN_RC_ContainerType = ZGuid.Empty;
			AssertHasMessageErrorContaining(container.ACN_RC_ContainerTypeInfo, "Container Type");
		}

		public void TestCheckACN_EmptyFullIndicator()
		{
			var container = Factory.New<AsycudaContainer>();

			container.ACN_EmptyFullIndicator = "X";
			AssertHasMessageErrorContaining(container.ACN_EmptyFullIndicatorInfo, "list");

			container.ACN_EmptyFullIndicator = container.Lookups.EmptyFullList[0].Code;
			AssertNoMessageErrorContaining(container.ACN_EmptyFullIndicatorInfo, "list");

			container.ACN_EmptyFullIndicator = "";
			AssertHasMessageErrorContaining(container.ACN_EmptyFullIndicatorInfo, "Empty/Full Indicator");
		}

		public void TestCheckACN_EmptyFullIndicatorEcl()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			CreateRefCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Vanuatu);
			CreateRefCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.CountryCodes.Vanuatu);
			helper.CreateNewOrGetExistingCusCodeListAttribute(
				helper.CreateNewOrGetExistingCusCodeList(
					Core.Constants.CountryCodes.Vanuatu,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestValidationRule,
					"ContainerEmptyFullIndicator",
					"All Containers of an ECL Manifest must be Empty Container",
					ZDateTime.MinSmallDateTimeValue,
					ZDateTime.MaxSmallDateTimeValue).PK,
				Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes.MandatoryForManifestType,
				"ECL");
			Factory.Save();

			var manifestTypes = new[] { "BBB", "COH", "COM", "ECL" };
			var emptyFullIndicators = new[] { "FCL", "CLC", "MT" };

			VoidParameterlessDelegate assertDelegate = () => { };

			foreach (var manifestType in manifestTypes)
			{
				foreach (var emptyFullIndicator in emptyFullIndicators)
				{
					assertDelegate += () =>
					{
						var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Vanuatu, "ASY");
						var container = header.Containers.AddNew();
						header.AMA_ManifestType = manifestType;
						container.ACN_EmptyFullIndicator = emptyFullIndicator;
						if (manifestType == "ECL" && emptyFullIndicator != "MT")
						{
							AssertHasMessageErrorContaining(
								container.ACN_EmptyFullIndicatorInfo,
								"All Containers of an ECL Manifest must be Empty Container"
							);
						}
						else
						{
							AssertNoMessageErrorContaining(
								container.ACN_EmptyFullIndicatorInfo,
								"All Containers of an ECL Manifest must be Empty Container"
							);
						}
					};
				}
			}

			assertDelegate += () =>
			{
				var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedKingdom, "ICS");
				var container = header.Containers.AddNew();
				container.ACN_EmptyFullIndicator = "MT";
				AssertNoMessageErrorContaining(
					container.ACN_EmptyFullIndicatorInfo,
					"All Containers of an ECL Manifest must be Empty Container"
				);
			};

			CombineAssertions(assertDelegate);
		}

		public void TestCheckACN_SealingPartyType()
		{
			CreateCusMap(
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping,
				RefCusMapTypeList.Codes.STYPE,
				Core.Constants.ContainerSealParties.Codes.CarrierShippingLine);
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "SB";
			var container = header.Containers.AddNew();

			container.ACN_SealingPartyType = "X";
			AssertHasMessageError(container.ACN_SealingPartyTypeInfo, ListValidation.InvalidCodeMessageError);

			container.ACN_SealingPartyType = Core.Constants.ContainerSealParties.Codes.CarrierShippingLine;
			AssertNoMessageError(container.ACN_SealingPartyTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckACN_SealingPartyTypeFormValidationSTYPE()
		{
			CreateCusMap(
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping,
				RefCusMapTypeList.Codes.STYPE,
				Core.Constants.ContainerSealParties.Codes.CarrierShippingLine);
			CreateCusMap(Core.Constants.CountryCodes.Vanuatu,
				RefCusMapTypeList.Codes.STYPE,
				Core.Constants.ContainerSealParties.Codes.Customs);
			Factory.Save();

			AssertSealingPartyTypeFormValidationSTYPE(x => x.ACN_SealingPartyTypeInfo);
			AssertSealingPartyTypeFormValidationSTYPE(x => x.ACN_SealingPartyType2Info);
			AssertSealingPartyTypeFormValidationSTYPE(x => x.ACN_SealingPartyType3Info);
		}

		public void TestCheckACN_GoodsWeightUQ()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "SB";
			var container = header.Containers.AddNew();

			container.ACN_GoodsWeightUQ = "20";
			AssertHasMessageErrorContaining(container.ACN_GoodsWeightUQInfo, "list");

			container.ACN_GoodsWeightUQ = Core.Constants.Weight.Kilograms;
			AssertNoMessageErrorContaining(container.ACN_GoodsWeightUQInfo, "list");
		}

		public void TestCheckACN_SealTypeBehavior()
		{
			CreateCusMap(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, RefCusMapTypeList.Codes.MSELT, SealTypeList.Codes.ElectronicSeal, "2");
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			AssertSealTypeBehavior(container.ACN_SealType1Info);
			AssertSealTypeBehavior(container.ACN_SealType2Info);
			AssertSealTypeBehavior(container.ACN_SealType3Info);
		}

		public void TestCheckACN_Seal1IsMandatory()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			CreateRefCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Eritrea);
			CreateRefCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingCusCodeListAttribute(
				helper.CreateNewOrGetExistingCusCodeList(
					Core.Constants.CountryCodes.Eritrea,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestValidationRule,
					"Seal",
					"Seal is required",
					ZDateTime.MinSmallDateTimeValue,
					ZDateTime.MaxSmallDateTimeValue).PK,
				Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes.Mandatory,
				"XXX");
			Factory.Save();

			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Eritrea, "ASY");
			var container = header.Containers.AddNew();
			var containerValidation = container.Validation;

			containerValidation.ValidateACN_Seal1();
			AssertHasMessageError(container.ACN_Seal1Info, "Seal is required for ER.");
			AssertNoMessageErrors(container.ACN_Seal2Info);
			AssertNoMessageErrors(container.ACN_Seal3Info);
		}

		public void TestCheckACN_Seal1()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Vanuatu, "ASY");
			var container = header.Containers.AddNew();
			var containerValidation = container.Validation;

			CombineAssertions(() =>
			{
				var errorMessage = "Please complete seal 1 before seal 2 or 3 are entered";
				containerValidation.ValidateACN_Seal1();
				AssertNoMessageError("All seals empty", container.ACN_Seal1Info, errorMessage);
				container.ACN_Seal2 = "SEAL2";
				containerValidation.ValidateACN_Seal1();
				AssertHasMessageError("Seal 2 entered", container.ACN_Seal1Info, errorMessage);
				container.ACN_Seal2 = ZString.Empty;
				container.ACN_Seal3 = "SEAL3";
				containerValidation.ValidateACN_Seal1();
				AssertHasMessageError("Seal 3 entered", container.ACN_Seal1Info, errorMessage);
				container.ACN_Seal2 = "SEAL2";
				container.ACN_Seal1 = "SEAL1";
				AssertNoMessageError("All seals entered", container.ACN_Seal1Info, errorMessage);
			});
		}

		public void TestCheckACN_Seal2()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Vanuatu, "ASY");
			var container = header.Containers.AddNew();
			var containerValidation = container.Validation;

			CombineAssertions(() =>
			{
				var errorMessageSeal1 = "Please complete seal 1 before seal 2 is entered";
				var errorMessageSeal2 = "Please complete seal 2 before seal 3 is entered";
				containerValidation.ValidateACN_Seal2();
				AssertNoMessageError("All seals empty", container.ACN_Seal2Info, errorMessageSeal1);
				AssertNoMessageError("All seals empty", container.ACN_Seal2Info, errorMessageSeal2);
				container.ACN_Seal3 = "SEAL3";
				containerValidation.ValidateACN_Seal2();
				AssertHasMessageError("Seal 3 entered", container.ACN_Seal2Info, errorMessageSeal2);
				container.ACN_Seal3 = ZString.Empty;
				container.ACN_Seal2 = "SEAL2";
				AssertHasMessageError("Seal 2 entered", container.ACN_Seal2Info, errorMessageSeal1);
				container.ACN_Seal1 = "SEAL1";
				containerValidation.ValidateACN_Seal2();
				AssertNoMessageError("Seal 1 entered", container.ACN_Seal2Info, errorMessageSeal1);
				AssertNoMessageError("Seal 1 entered", container.ACN_Seal2Info, errorMessageSeal2);
			});
		}

		public void TestCheckACN_Seal3()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Vanuatu, "ASY");
			var container = header.Containers.AddNew();
			var containerValidation = container.Validation;

			CombineAssertions(() =>
			{
				var errorMessage = "Please complete seal 1 and 2 before seal 3 is entered";
				containerValidation.ValidateACN_Seal3();
				AssertNoMessageError("All seals empty", container.ACN_Seal3Info, errorMessage);
				container.ACN_Seal3 = "SEAL3";
				AssertHasMessageError("Seal 3 entered", container.ACN_Seal3Info, errorMessage);
				container.ACN_Seal2 = "SEAL2";
				containerValidation.ValidateACN_Seal3();
				AssertHasMessageError("Seal 3 and Seal 2 Entered", container.ACN_Seal3Info, errorMessage);
				container.ACN_Seal1 = "SEAL1";
				containerValidation.ValidateACN_Seal3();
				AssertNoMessageError("All seals entered", container.ACN_Seal3Info, errorMessage);
				container.ACN_Seal3 = ZString.Empty;
				AssertNoMessageError("Seal 1 and 2 entered", container.ACN_Seal3Info, errorMessage);
			});
		}

		public void TestCheckACN_Seal1UnloadingState()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Vanuatu, "ASY");
			var container = header.Containers.AddNew();

			AssertUnloadingStates(code => container.ACN_Seal1UnloadingState = code, container.ACN_Seal1UnloadingStateInfo);
		}

		public void TestCheckACN_Seal2UnloadingState()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Vanuatu, "ASY");
			var container = header.Containers.AddNew();

			AssertUnloadingStates(code => container.ACN_Seal2UnloadingState = code, container.ACN_Seal2UnloadingStateInfo);
		}

		public void TestCheckACN_Seal3UnloadingState()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Vanuatu, "ASY");
			var container = header.Containers.AddNew();

			AssertUnloadingStates(code => container.ACN_Seal3UnloadingState = code, container.ACN_Seal3UnloadingStateInfo);
		}

		void AssertUnloadingStates(Action<string> setCode, ZPropertyInfo propertyInfo)
		{
			AssertCode(() => setCode.Invoke("ZZZ"), propertyInfo, true);

			var unloadingStates = new UnloadingStates();
			foreach (var code in unloadingStates.GetAllCodes())
			{
				AssertCode(() => setCode.Invoke(code), propertyInfo, false);
			}
		}

		void AssertCode(Action action, ZPropertyInfo propertyInfo, bool isErrorExpected)
		{
			action.Invoke();

			if (isErrorExpected)
			{
				AssertHasErrorContaining(propertyInfo, ListValidation.InvalidCodeError);
			}
			else
			{
				AssertNoErrorContaining(propertyInfo, ListValidation.InvalidCodeError);
			}
		}

		void AssertSealTypeBehavior(ZPropertyInfo sealTypeInfo)
		{
			sealTypeInfo.Value = (ZString)SealTypeList.Codes.MechanicalSeal;
			AssertHasErrorContaining(sealTypeInfo, ListValidation.InvalidCodeError);

			sealTypeInfo.Value = (ZString)SealTypeList.Codes.ElectronicSeal;
			AssertNoErrorContaining(sealTypeInfo, ListValidation.InvalidCodeError);
		}

		void CreateCusMap(string countryCode, string typeName, string wtgCode, string customsCode = null)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(countryCode);
			helper.CreateCusMapType(typeName, MapDirectionList.Codes.BTH, typeName, false);
			helper.CreateCusMap(typeName, wtgCode, customsCode ?? wtgCode, new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), countryCode);
		}

		void CreateRefCusCodeType(string typeName, string dataGrouping)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(typeName, typeName, dataGrouping);
		}

		void AssertSealingPartyTypeFormValidationSTYPE(Func<AsycudaContainer, ZPropertyInfo> getSealingPartyTypeInfo)
		{
			#region Assert FJ
			var headerFJ = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Fiji, "ASY");
			var containerFJSealingPartyTypeInfo = getSealingPartyTypeInfo(headerFJ.Containers.AddNew());

			containerFJSealingPartyTypeInfo.Value = (ZString)Core.Constants.ContainerSealParties.Codes.Terminal;
			AssertHasMessageError(containerFJSealingPartyTypeInfo, ListValidation.InvalidCodeMessageError);

			containerFJSealingPartyTypeInfo.Value = (ZString)Core.Constants.ContainerSealParties.Codes.Customs;
			AssertHasMessageError(containerFJSealingPartyTypeInfo, ListValidation.InvalidCodeMessageError);

			containerFJSealingPartyTypeInfo.Value = (ZString)Core.Constants.ContainerSealParties.Codes.CarrierShippingLine;
			AssertNoMessageError(containerFJSealingPartyTypeInfo, ListValidation.InvalidCodeMessageError);
			#endregion

			#region Assert VU
			var headerVU = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Vanuatu, "ASY");
			var containerVUSealingPartyTypeInfo = getSealingPartyTypeInfo(headerVU.Containers.AddNew());

			containerVUSealingPartyTypeInfo.Value = (ZString)Core.Constants.ContainerSealParties.Codes.Terminal;
			AssertHasMessageError(containerVUSealingPartyTypeInfo, ListValidation.InvalidCodeMessageError);

			containerVUSealingPartyTypeInfo.Value = (ZString)Core.Constants.ContainerSealParties.Codes.Customs;
			AssertNoMessageError(containerVUSealingPartyTypeInfo, ListValidation.InvalidCodeMessageError);

			containerVUSealingPartyTypeInfo.Value = (ZString)Core.Constants.ContainerSealParties.Codes.CarrierShippingLine;
			AssertHasMessageError(containerVUSealingPartyTypeInfo, ListValidation.InvalidCodeMessageError);
			#endregion
		}
	}
}
