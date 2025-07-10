using System;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsArrivalCargoDesc))]
sealed class NctsArrivalCargoDescTest : NctsArrivalCargoDescAbstractTest<NctsHeader>
{
	public void TestValidation()
	{
		AssertType<NctsArrivalCargoDescValidation>(NctsArrivalCargoDesc.Validation);
	}

	public void TestLookups()
	{
		AssertType<NctsArrivalCargoDescLookups>(NctsArrivalCargoDesc.Lookups);
	}

	public void TestUnloadedGoodsItem()
	{
		NctsArrivalCargoDesc.BY_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF;
		AssertType<NctsUnloadedCargoDesc>(NctsArrivalCargoDesc.UnloadedGoodsItem);
	}

	public void TestNctsAdditionalInfoCollection()
	{
		AssertType<NctsAdditionalInfoCollection<NctsAdditionalInfo>>(NctsArrivalCargoDesc.AdditionalInfos);
	}

	public void TestAdditionalInfos_ReadOnly()
	{
		AssertEquals(true, NctsArrivalCargoDesc.AdditionalInfos.ReadOnly);
	}

	public void TestPreviousDocumentsCollection()
	{
		AssertType<NctsPreviousDocumentCollection<NctsPreviousDocument>>(NctsArrivalCargoDesc.PreviousDocuments);
	}

	public void TestSupportingDocumentCollection()
	{
		AssertType<NctsSupportingDocumentCollection<EU.NCTS.Business.NctsSupportingDocument>>(NctsArrivalCargoDesc.SupportingDocuments);
	}

	public void TestGetNctsPackageCollection()
	{
		AssertType<NctsPackageCollection>(NctsArrivalCargoDesc.Packages);
	}

	public void TestSupportingDocuments_ReadOnly()
	{
		AssertEquals(true, NctsArrivalCargoDesc.SupportingDocuments.ReadOnly);
	}

	public new void TestCorrectTypeDecideForLoad()
	{
		Assert("This test is temporarily disabled because NctsArrivalCargoDesc is not connected to NctsArrivalMovementHeader", true);
	}

	public void TestUnloadingRemarkCodeCaption() => AssertEquals("UnloadingRemarkCode", "Unloading Code", NctsArrivalCargoDesc.UnloadingRemarkCodeInfo.Description);

	public void TestUnloadingRemarkTextCaption() => AssertEquals("UnloadingRemarkText", "Unloading Remarks", NctsArrivalCargoDesc.UnloadingRemarkTextInfo.Description);

	public void TestGoodsItemDifferencesDetailsCollection() => CombineAssertions(() =>
	{
		AssertType<GoodsItemDifferencesDetailsCollection>(NctsArrivalCargoDesc.GoodsItemDifferencesDetails);
		AssertType<GoodsItemDifferencesDetails>(NctsArrivalCargoDesc.GoodsItemDifferencesDetails.AddNew());
	});

	public void TestGoodsItemDifferencesDetails() => CombineAssertions(() =>
	{
		AssertEquals("Collection initially empty", 0, NctsArrivalCargoDesc.GoodsItemDifferencesDetails.Count);
		AssertEquals("UnloadingRemarkCode initial state", ZString.Empty, NctsArrivalCargoDesc.UnloadingRemarkCode);
		AssertEquals("UnloadingRemarksText initial state", ZString.Empty, NctsArrivalCargoDesc.UnloadingRemarkText);

		NctsArrivalCargoDesc.UnloadingRemarkCode = UnloadingRemarkCodeList.Codes.NotShipped;
		NctsArrivalCargoDesc.UnloadingRemarkText = "Description 1";

		AssertEquals("Collection value set", 1, NctsArrivalCargoDesc.GoodsItemDifferencesDetails.Count);
		AssertEquals("UnloadingRemarkCode set", UnloadingRemarkCodeList.Codes.NotShipped, NctsArrivalCargoDesc.GoodsItemDifferencesDetail.CY_Code);
		AssertEquals("UnloadingRemarksText set", "Description 1", NctsArrivalCargoDesc.GoodsItemDifferencesDetail.CY_Data);

		NctsArrivalCargoDesc.UnloadingRemarkCode = UnloadingRemarkCodeList.Codes.Stolen;
		NctsArrivalCargoDesc.UnloadingRemarkText = "Description 2";
		AssertEquals("Collection value set", 1, NctsArrivalCargoDesc.GoodsItemDifferencesDetails.Count);

		AssertSame("Cached", NctsArrivalCargoDesc.GoodsItemDifferencesDetails, NctsArrivalCargoDesc.GoodsItemDifferencesDetails);
	});

	public void TestClearUnloadingRemarks() => CombineAssertions(() =>
	{
		NctsArrivalCargoDesc.BY_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF;
		NctsArrivalCargoDesc.UnloadingRemarkCode = UnloadingRemarkCodeList.Codes.NotShipped;
		NctsArrivalCargoDesc.UnloadingRemarkText = "Text";

		NctsArrivalCargoDesc.BY_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;

		AssertEquals("Clear UnloadingRemarkCode", ZString.Empty, NctsArrivalCargoDesc.UnloadingRemarkCode);
		AssertEquals("Clear UnloadingRemarkText", ZString.Empty, NctsArrivalCargoDesc.UnloadingRemarkText);
	});

	public void TestIsUnloadedCommodityCodeRequired() => AssertEquals("IsUnloadedCommodityCodeRequired", false, NctsArrivalCargoDesc.IsUnloadedCommodityCodeRequired);

	public void TestConsigneeDocAddressRequirementType() => AssertType<JobDocAddressRequirement>(NctsArrivalCargoDesc.ConsigneeDocAddressRequirement);

	public void TestConsigneeDocAddressStateNotRequired()
	{
		const string error = "You must enter a state.";
		var address = NctsArrivalCargoDesc.ConsigneeDocAddress;
		address.E2_AddressOverride = true;
		address.Validation.ValidateE2_State();
		AssertEquals("E2_State empty and no error", false, address.E2_StateInfo.Notifications.Any(n => n.Message.Contains(error)));
	}

	public void TestGetNewTariffFormatter()
	{
		AssertType<TariffFormatterCH>(((ITariffFormatProvider)NctsArrivalCargoDesc).TariffFormatter);
	}

	public void TestPropertiesReadOnlyIfLockedULR() => CombineAssertions(() =>
	{
		using (new LockForEditTestHelper(Factory, EUJobMessageTypeList.Codes.NctsArrivalUnloadingRemarks, DeclarationTabPages.Codes.NctsArrivalUnloadingRemarks))
		{
			NctsArrivalCargoDesc.Header.LockFile("Test lock");
			AssertProperties("Locked", true);
			NctsArrivalCargoDesc.Header.UnlockFile("Test unlock");
			AssertProperties("Unlocked", false);
		}

		void AssertProperties(string assertionMessage, bool expectedReadOnly)
		{
			AssertEquals($"{assertionMessage} - UnloadingRemarkCodeInfo.ReadOnly", expectedReadOnly, NctsArrivalCargoDesc.UnloadingRemarkCodeInfo.ReadOnly);
			AssertEquals($"{assertionMessage} - UnloadingRemarkTextInfo.ReadOnly", expectedReadOnly, NctsArrivalCargoDesc.UnloadingRemarkTextInfo.ReadOnly);
		}
	});

	public void TestUnloadingRemarkCodeReadOnly() => AssertReadOnly(NctsArrivalCargoDesc.UnloadingRemarkCodeInfo);

	public void TestUnloadingRemarkTextReadOnly() => AssertReadOnly(NctsArrivalCargoDesc.UnloadingRemarkTextInfo);

	void AssertReadOnly(ZPropertyInfo property) => CombineAssertions(() =>
	{
		NctsArrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		AssertEquals($"{property.HasHumanReadableName} BY_UnloadedState={NctsArrivalCargoDesc.BY_UnloadedState}", true, property.ReadOnly);

		NctsArrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		AssertEquals($"{property.HasHumanReadableName} BY_UnloadedState={NctsArrivalCargoDesc.BY_UnloadedState}", false, property.ReadOnly);

		NctsArrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		AssertEquals($"{property.HasHumanReadableName} BY_UnloadedState={NctsArrivalCargoDesc.BY_UnloadedState}", false, property.ReadOnly);

		NctsArrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		AssertEquals($"{property.HasHumanReadableName} BY_UnloadedState={NctsArrivalCargoDesc.BY_UnloadedState}", false, property.ReadOnly);

		using (new LockForEditTestHelper(Factory, EUJobMessageTypeList.Codes.NctsArrivalUnloadingRemarks, DeclarationTabPages.Codes.NctsArrivalUnloadingRemarks))
		{
			NctsArrivalCargoDesc.Header.LockFile("Test lock");
			AssertEquals($"Lock - {property.HasHumanReadableName}", true, property.ReadOnly);
			NctsArrivalCargoDesc.Header.UnlockFile("Test unlock");
			AssertEquals($"UnLock - {property.HasHumanReadableName}", false, property.ReadOnly);
		}
	});

	public void TestIsAllPackageDEC() => AssertIsAllPackage(NctsUnloadedStateList.Codes.DEC, true, () => NctsArrivalCargoDesc.IsAllPackageDEC);

	public void TestIsAllPackageMIS() => AssertIsAllPackage(NctsUnloadedStateList.Codes.MIS, false, () => NctsArrivalCargoDesc.IsAllPackageMIS);

	void AssertIsAllPackage(string packageTypeOfDifferenceToCheck, bool valueWhenNoPackage, Func<bool> isAllPackageGetter)
	{
		var packageTypeOfDifferenceList = new[] { NctsUnloadedStateList.Codes.DAM, NctsUnloadedStateList.Codes.DEC, NctsUnloadedStateList.Codes.DIF , NctsUnloadedStateList.Codes.MIS, NctsUnloadedStateList.Codes.NEW };

		AssertEquals("No packages", valueWhenNoPackage, isAllPackageGetter());

		NctsArrivalCargoDesc.Packages.AddNew().B5_TypeOfDifference = packageTypeOfDifferenceToCheck;
		var additionalPackage = NctsArrivalCargoDesc.Packages.AddNew();

		foreach (var packageTypeOfDifference in packageTypeOfDifferenceList)
		{
			additionalPackage.B5_TypeOfDifference = packageTypeOfDifference;
			AssertEquals($"Additional '{packageTypeOfDifference}' Package", packageTypeOfDifference == packageTypeOfDifferenceToCheck, isAllPackageGetter());
		}
	}

	public void TestIsAnyPackageDIF() => CombineAssertions(() =>
	{
		AssertEquals("No packages", false, NctsArrivalCargoDesc.IsAnyPackageDIF);

		NctsArrivalCargoDesc.Packages.AddNew().B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
		AssertEquals("DEC", false, NctsArrivalCargoDesc.IsAnyPackageDIF);

		NctsArrivalCargoDesc.Packages.AddNew().B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
		AssertEquals("MIS", false, NctsArrivalCargoDesc.IsAnyPackageDIF);

		NctsArrivalCargoDesc.Packages.AddNew().B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
		AssertEquals("NEW", false, NctsArrivalCargoDesc.IsAnyPackageDIF);

		NctsArrivalCargoDesc.Packages.AddNew().B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
		AssertEquals("DIF", true, NctsArrivalCargoDesc.IsAnyPackageDIF);
	});

	public void TestIsDIFWithDifferences() => CombineAssertions(() =>
	{
		NctsArrivalCargoDesc.BY_HarmonisedTariff = "1000.00";
		NctsArrivalCargoDesc.BY_CusC4Number = "12";
		NctsArrivalCargoDesc.BY_Description = "abc";
		NctsArrivalCargoDesc.BY_GrossWeight = 11;
		NctsArrivalCargoDesc.BY_NetWeight = 10;

		AssertResult(false);
		AssertResult(true, unloadedHarmonisedTariff: "2000.00");
		AssertResult(true, unloadedCusC4Number: "23");
		AssertResult(true, unloadedDescription: "xyz");
		AssertResult(true, unloadedGrossWeight: 21);
		AssertResult(true, unloadedNetWeight: 20);
		AssertResult(false, unloadedHarmonisedTariff: ZString.Empty);
		AssertResult(false, unloadedCusC4Number: ZString.Empty);
		AssertResult(false, unloadedDescription: ZString.Empty);
		AssertResult(false, unloadedGrossWeight: ZDecimal.Zero);
		AssertResult(false, unloadedNetWeight: ZDecimal.Zero);

		void AssertResult(bool expectedResult, string unloadedState = NctsUnloadedStateList.Codes.DIF, ZString? unloadedHarmonisedTariff = null, ZString? unloadedCusC4Number = null, ZString? unloadedDescription = null, ZDecimal? unloadedGrossWeight = null, ZDecimal? unloadedNetWeight = null, [CallerLineNumber] int line = 0)
		{
			NctsArrivalCargoDesc.BY_UnloadedState = unloadedState;
			var unloadedGoodsItem = NctsArrivalCargoDesc.UnloadedGoodsItem;
			if (unloadedGoodsItem != null)
			{
				unloadedGoodsItem.BY_HarmonisedTariff = unloadedHarmonisedTariff ?? NctsArrivalCargoDesc.BY_HarmonisedTariff;
				unloadedGoodsItem.BY_CusC4Number = unloadedCusC4Number ?? NctsArrivalCargoDesc.BY_CusC4Number;
				unloadedGoodsItem.BY_Description = unloadedDescription ?? NctsArrivalCargoDesc.BY_Description;
				unloadedGoodsItem.BY_GrossWeight = unloadedGrossWeight ?? NctsArrivalCargoDesc.BY_GrossWeight;
				unloadedGoodsItem.BY_NetWeight = unloadedNetWeight ?? NctsArrivalCargoDesc.BY_NetWeight;
			}
			var assertionMessage = $"[{line}] {unloadedState}: HarmonizedTariff={NctsArrivalCargoDesc.BY_HarmonisedTariff}/{unloadedGoodsItem?.BY_HarmonisedTariff} CusC4Number={NctsArrivalCargoDesc.BY_CusC4Number}/{unloadedGoodsItem?.BY_CusC4Number} Description={NctsArrivalCargoDesc.BY_Description}/{unloadedGoodsItem?.BY_Description} GrossWeight={NctsArrivalCargoDesc.BY_GrossWeight}/{unloadedGoodsItem?.BY_GrossWeight} NetWeight={NctsArrivalCargoDesc.BY_NetWeight}/{unloadedGoodsItem?.BY_NetWeight}";
			AssertEquals(assertionMessage, expectedResult, NctsArrivalCargoDesc.IsDIFWithDifferences);
		}
	});

	public void TestIsDIFWithDifferencesIncludingPackages() => CombineAssertions(() =>
	{
		NctsArrivalCargoDesc.BY_HarmonisedTariff = "1000.00";

		AssertResult(false);
		AssertResult(true, unloadedHarmonisedTariff: "2000.00");

		var package1 = AddPackage();
		package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
		var package2 = AddPackage();
		package2.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
		AssertResult(false);

		package2.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
		AssertResult(false);

		package2.B5_UnitCount += 1;
		AssertResult(true);

		AssertResult(false, unloadedHarmonisedTariff: "2000.00", unloadedState: NctsUnloadedStateList.Codes.MIS);
		AssertResult(false, unloadedHarmonisedTariff: "2000.00", unloadedState: NctsUnloadedStateList.Codes.NEW);
		AssertResult(false, unloadedHarmonisedTariff: "2000.00", unloadedState: NctsUnloadedStateList.Codes.DEC);

		void AssertResult(bool expectedResult, string unloadedState = NctsUnloadedStateList.Codes.DIF, ZString? unloadedHarmonisedTariff = null, [CallerLineNumber] int line = 0)
		{
			NctsArrivalCargoDesc.BY_UnloadedState = unloadedState;
			var unloadedGoodsItem = NctsArrivalCargoDesc.UnloadedGoodsItem;
			if (unloadedGoodsItem != null)
			{
				unloadedGoodsItem.BY_HarmonisedTariff = unloadedHarmonisedTariff ?? NctsArrivalCargoDesc.BY_HarmonisedTariff;
			}
			var assertionMessage = $"[{line}] {unloadedState}: HarmonizedTariff={NctsArrivalCargoDesc.BY_HarmonisedTariff}/{unloadedGoodsItem?.BY_HarmonisedTariff}";
			AssertEquals(assertionMessage, expectedResult, NctsArrivalCargoDesc.IsDIFWithDifferencesIncludingPackages);
		}

		NctsPackage AddPackage()
		{
			var package = NctsArrivalCargoDesc.Packages.AddNew();
			package.B5_UnitCount = 1;
			package.B5_UnitType = "CT";
			package.B5_MarksAndNumbers = "M+N";
			return package;
		}
	});

	public void TestIsAnyPackageDIFWithDifferences() => CombineAssertions(() =>
	{
		AssertEquals("No packages", false, NctsArrivalCargoDesc.IsAnyPackageDIFWithDifferences);

		var package1 = NctsArrivalCargoDesc.Packages.AddNew();
		package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
		AssertEquals("DEC", false, NctsArrivalCargoDesc.IsAnyPackageDIFWithDifferences);

		var package2 = NctsArrivalCargoDesc.Packages.AddNew();
		package2.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
		AssertEquals("MIS", false, NctsArrivalCargoDesc.IsAnyPackageDIFWithDifferences);

		var package3 = NctsArrivalCargoDesc.Packages.AddNew();
		package3.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
		AssertEquals("NEW", false, NctsArrivalCargoDesc.IsAnyPackageDIFWithDifferences);

		var package4 = NctsArrivalCargoDesc.Packages.AddNew();
		package4.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
		package4.B5_MarksAndNumbers = "a";
		AssertEquals("DIF without differences", false, NctsArrivalCargoDesc.IsAnyPackageDIFWithDifferences);

		package4.PackDifference.B5_MarksAndNumbers = "b";
		AssertEquals("DIF with differences", true, NctsArrivalCargoDesc.IsAnyPackageDIFWithDifferences);
	});

	public void TestIsUnloadedStateMIS() => CombineAssertions(() =>
	{
		NctsArrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		AssertEquals("UnloadedState = DIF", false, NctsArrivalCargoDesc.IsUnloadedStateMIS);

		NctsArrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		AssertEquals("UnloadedState = NEW", false, NctsArrivalCargoDesc.IsUnloadedStateMIS);

		NctsArrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		AssertEquals("UnloadedState = MIS", true, NctsArrivalCargoDesc.IsUnloadedStateMIS);
	});

	protected override BusinessObject GetNewBusinessObject() => NctsArrivalCargoDesc;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => NctsArrivalCargoDesc;

	NctsArrivalCargoDesc CreateNctsArrivalCargoDesc()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		var bill = header.Bills.AddNew();
		var nctsArrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
		return nctsArrivalCargoDesc;
	}

	NctsArrivalCargoDesc NctsArrivalCargoDesc => nctsArrivalCargoDesc ??= CreateNctsArrivalCargoDesc();
	NctsArrivalCargoDesc nctsArrivalCargoDesc;

	protected override ZString CountryCode => Core.Constants.CountryCodes.Switzerland;
}
