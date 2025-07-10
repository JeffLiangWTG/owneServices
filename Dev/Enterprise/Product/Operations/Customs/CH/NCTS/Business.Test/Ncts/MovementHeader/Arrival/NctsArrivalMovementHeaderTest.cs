using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsArrivalMovementHeader))]
sealed class NctsArrivalMovementHeaderTest : NctsArrivalMovementHeaderAbstractTest
{
	[TestDate(2025, 5, 26, 16, 00, 00)]
	public void TestDefaultValues() => CombineAssertions(() =>
	{
		AssertEquals("Default MultipleMRNIndicator:", true, ArrivalMovementHeader.MultipleMRNIndicator);
		AssertEquals("Default BM_ArrivalDate now", ZDateTime.Now, ArrivalMovementHeader.BM_ArrivalDate);
	});

	public void TestGetNewLookups() => AssertType<NctsArrivalMovementHeaderLookups>(ArrivalMovementHeader.Lookups);

	public void TestGetNewValidation() => CombineAssertions(() =>
	{
		AssertType<NctsArrivalMovementHeaderValidation>(ArrivalMovementHeader.Validation);
		AssertType<CusGoodsLocation>(ArrivalMovementHeader.GoodsLocation);
	});

	public void TestCaptions() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(ArrivalMovementHeader.BM_TransportAtArrivalTypeInfo, caption: "Type of ID", fullDescription: "Identification Type of the Transport Means");
		CaptionTestHelper.AssertCaptions(ArrivalMovementHeader.BM_TransportAtArrivalIDInfo, caption: "Transport ID", fullDescription: "Identification of the Transport Means");
		CaptionTestHelper.AssertCaptions(ArrivalMovementHeader.BM_RN_NKTransportAtArrivalIDNationalityInfo, caption: "Nationality", fullDescription: "Nationality of the Transport Means");
	});

	public void TestTotalUnloadedNumberOfPackages()
	{
		var bulkType = Factory.SetupBulkCusCode();
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		var bill1 = header.Bills.AddNew();

		var goodsItem1 = bill1.ArrivalGoodsItems.AddNew();
		var package1 = goodsItem1.Packages.AddNew();
		package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
		package1.B5_UnitCount = 10;
		var package2 = goodsItem1.Packages.AddNew();
		package2.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
		package2.B5_UnitType = bulkType;

		var goodsItem2 = bill1.ArrivalGoodsItems.AddNew();
		goodsItem2.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		var package3 = goodsItem2.Packages.AddNew();
		package3.B5_UnitCount = 20;

		var goodsItem3 = bill1.ArrivalGoodsItems.AddNew();
		var package4 = goodsItem3.Packages.AddNew();
		package4.B5_UnitCount = 30;

		var bill2 = header.Bills.AddNew();

		var goodsItem4 = bill2.ArrivalGoodsItems.AddNew();
		goodsItem4.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		var package5 = goodsItem4.Packages.AddNew();
		package5.B5_UnitCount = 60;

		var goodsItem5 = bill2.ArrivalGoodsItems.AddNew();
		goodsItem5.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		var package6 = goodsItem5.Packages.AddNew();
		package6.B5_UnitCount = 10;

		AssertEquals(111, header.ArrivalMovementHeader.TotalUnloadedNumberOfPackages);
	}

	public void TestSetReadOnlyForUnloadingDifferencesData_WhenBM_NoChangesToReport_False() => CombineAssertions(() =>
	{
		var bill = ArrivalMovementHeader.Header.Bills.AddNew();
		bill.ArrivalGoodsItems.AddNew();
		ArrivalMovementHeader.BM_NoChangesToReport = false;
		Factory.Save();

		var loadedHeader = Factory.Load<NctsHeader>(ArrivalMovementHeader.Header.PK);
		var loadedArrivalMovementHeader = loadedHeader.ArrivalMovementHeader;
		var loadedBill = loadedHeader.Bills.FirstOrDefault();
		var goodsItem = loadedBill.ArrivalGoodsItems.FirstOrDefault();

		AssertEquals("Bill not readonly", false, loadedBill.ReadOnly);
		AssertEquals("Bill.ArrivalHeaderContainers readonly", false, loadedHeader.ArrivalHeaderContainers.ReadOnly);
		AssertEquals("Bill.SupportingDocuments readonly", true, loadedBill.SupportingDocuments.ReadOnly);
		AssertEquals("Bill.AdditionalDocuments readonly", true, loadedBill.AdditionalDocuments.ReadOnly);
		AssertEquals("Bill.PreviousDocuments readonly", true, loadedBill.PreviousDocuments.ReadOnly);
		AssertEquals("Bill.ArrivalTransportInfos readonly", true, loadedBill.ArrivalTransportInfos.ReadOnly);
		AssertEquals("Bill.ArrivalGoodsItems (as one example of Bill children) not readonly", false, loadedBill.ArrivalGoodsItems.ReadOnly);

		AssertEquals("Bill.ArrivalGoodsItems.SupportingDocuments (as one example of Bill children) not readonly", true, goodsItem.SupportingDocuments.ReadOnly);
		AssertEquals("Bill.ArrivalGoodsItems.PreviousDocuments (as one example of Bill children) not readonly", true, goodsItem.PreviousDocuments.ReadOnly);
		AssertEquals("Bill.ArrivalGoodsItems.AdditionalInfos (as one example of Bill children) not readonly", true, goodsItem.AdditionalInfos.ReadOnly);
	});

	public void TestSetReadOnlyForUnloadingDifferencesData_WhenBM_NoChangesToReport_True() => CombineAssertions(() =>
	{
		var bill = ArrivalMovementHeader.Header.Bills.AddNew();
		bill.ArrivalGoodsItems.AddNew();
		ArrivalMovementHeader.BM_NoChangesToReport = false;
		Factory.Save();

		var loadedHeader = Factory.Load<NctsHeader>(ArrivalMovementHeader.Header.PK);
		var loadedArrivalMovementHeader = loadedHeader.ArrivalMovementHeader;
		var loadedBill = loadedHeader.Bills.FirstOrDefault();
		AssertEquals("Bill not readonly", false, loadedBill.ReadOnly);
		AssertEquals("ArrivalTransportInfos readonly", true, ArrivalMovementHeader.ArrivalTransportInfos.ReadOnly);
		AssertEquals("Bill.ArrivalGoodsItems (as one example of Bill children) not readonly", false, loadedBill.ArrivalGoodsItems.ReadOnly);
	});

	public void TestSupernumeraryGoods() => AssertType<SupernumeraryGoods>(ArrivalMovementHeader.SupernumeraryGoods.AddNew());

	public void TestAdditionalTransitOperations() => AssertType<AdditionalTransitOperation>(ArrivalMovementHeader.AdditionalTransitOperations.AddNew());

	public void TestRelatedArrivalMovements()
	{
		AssertType<RelatedArrivalMovementGenPivot>(ArrivalMovementHeader.RelatedArrivalMovements.AddNew());
	}

	public void TestMasterArrivalMovementHeader() => CombineAssertions(() =>
	{
		AssertNull("No master", ArrivalMovementHeader.MasterArrivalMovementHeader);

		var childMovement = CreateNctsArrivalMovementHeader();
		childMovement.MultipleMRNIndicator = false;
		ArrivalMovementHeader.RelatedArrivalMovements.AddPivotFor(childMovement);

		AssertEquals("Master", ArrivalMovementHeader.PK, childMovement.MasterArrivalMovementHeader.PK);
	});

	[TestDate(2023, 5, 21, 8, 12, 33)]
	public void TestValuationDate() => CombineAssertions(() =>
	{
		ArrivalMovementHeader.BM_ValuationDate = ZDateTime.Empty;
		AssertEquals($"BM_ValuationDate={ArrivalMovementHeader.BM_ValuationDate}", new ZDateTime(2023, 5, 21, 0, 0, 0), ArrivalMovementHeader.ValuationDate);
		ArrivalMovementHeader.BM_ValuationDate = new ZDateTime(2024, 6, 22, 9, 13, 44);
		AssertEquals($"BM_ValuationDate={ArrivalMovementHeader.BM_ValuationDate}", new ZDateTime(2024, 6, 22, 9, 13, 44), ArrivalMovementHeader.ValuationDate);
	});

	public void TestSealsStateValid_Caption() => EU.NCTS.Business.Testing.NCTSTestHelper.AssertCaptions(ArrivalMovementHeader.BM_StateOfSealsInfo, "Seals State Valid", string.Empty, string.Empty);

	public void TestAdditionalText_Caption() => EU.NCTS.Business.Testing.NCTSTestHelper.AssertCaptions(ArrivalMovementHeader.BM_AdditionalTextInfo, "Additional Text", string.Empty, string.Empty);

	public void TestMultipleMRNIndicator() => CombineAssertions(() =>
	{
		AssertEquals(true, ArrivalMovementHeader.MultipleMRNIndicator);
		ArrivalMovementHeader.MultipleMRNIndicator = false;
		AssertEquals(false, ArrivalMovementHeader.MultipleMRNIndicator);
	});

	protected override BusinessObject GetNewBusinessObject() => ArrivalMovementHeader;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => ArrivalMovementHeader;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => ArrivalMovementHeader;

	protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest) => new LightValidationTesterExcludingJobDocAddress(bizObjToTest);

	public void TestArrivalCustomerReferenceNumberFountain()
	{
		AssertEquals("Arrival Number Fountain", "CHLocalReferenceNumber", (ArrivalMovementHeader as ILRNGenerator).LrnNumberFountain.Name);
	}

	public void TestBM_PaperlessInbondNum_NotAutomaticallyGeneratedWhenManualArrivalCustomerReferenceIsEnabled()
	{
		var destinationTraderOrg = Factory.New<OrgHeader>();
		destinationTraderOrg.OH_Code = "1234";
		destinationTraderOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SwissCodeTypes.BID, "2233445566", "CH");
		NctsHeader.DestinationTrader.OrganisationPK = destinationTraderOrg.PK;

		using (EU.Registry.EUCustomsDataRegistry.Instance.NctsIsManualArrivalCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			Factory.Save();

			AssertEquals("LNR Nbr not generated", ZString.Empty, nctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum);
		}
	}

	[TestDate(2023, 12, 22)]
	public void TestBM_PaperlessInbondNum_AutomaticallyGeneratedWhenEmpty()
	{
		var destinationTraderOrg = Factory.New<OrgHeader>();
		destinationTraderOrg.OH_Code = "1234";
		destinationTraderOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SwissCodeTypes.BID, "2233445566", "CH");
		NctsHeader.DestinationTrader.OrganisationPK = destinationTraderOrg.PK;

		var referenceFormat = new ArrivalCustomerReferenceFormat();
		referenceFormat.UseSystemDefinedFormat = true;
		using (CHCustomsDataRegistry.Instance.ArrivalCustomerReferenceFormat.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, referenceFormat))
		using (EU.Registry.EUCustomsDataRegistry.Instance.NctsIsManualArrivalCustomerReferenceEnabled.SetTemporaryValue(destinationTraderOrg.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			Factory.Save();
			AssertEquals("Autogenerated LNR Nbr", "2322334455660000000001", NctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum);
		}
	}

	public void TestBM_PaperlessInbondNum_NotAutomaticallyGeneratedWhenCustomerReferenceIsNotEmpty()
	{
		var destinationTraderOrg = Factory.New<OrgHeader>();
		destinationTraderOrg.OH_Code = "1234";
		destinationTraderOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SwissCodeTypes.BID, "2233445566", "CH");
		NctsHeader.DestinationTrader.OrganisationPK = destinationTraderOrg.PK;

		var referenceFormat = new ArrivalCustomerReferenceFormat();
		referenceFormat.UseSystemDefinedFormat = true;
		using (CHCustomsDataRegistry.Instance.ArrivalCustomerReferenceFormat.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, referenceFormat))
		using (EU.Registry.EUCustomsDataRegistry.Instance.NctsIsManualArrivalCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			NctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum = "XYZ";
			Factory.Save();

			AssertEquals("Autogenerated LNR Nbr", "XYZ", nctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum);
		}
	}

	public void TestBM_PaperlessInbondNum_AutomaticallyGeneratedBasedOffAuthorisationNumber()
	{
		var currentYear = ZDateTime.Today.Year.ToString();
		SetUpNctsHeaderForCustomFormatAuthorizationNumber();

		using (EU.Registry.EUCustomsDataRegistry.Instance.NctsIsManualArrivalCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		using (CHCustomsDataRegistry.Instance.ArrivalCustomerReferenceFormat.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, referenceFormat))
		{
			Factory.Save();
			AssertEquals("use arrival reference number formatter when UseSystemFormatting is set to false", "AB" + currentYear + "1CD", NctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum);
		}
	}

	public void TestBM_PaperlessInbondNum_AutomaticallyGeneratedBasedOffAuthorisationNumberNotChanged()
	{
		var customerReference = "AB" + ZDateTime.Today.Year.ToString() + "1CD";
		SetUpNctsHeaderForCustomFormatAuthorizationNumber();

		using (EU.Registry.EUCustomsDataRegistry.Instance.NctsIsManualArrivalCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		using (CHCustomsDataRegistry.Instance.ArrivalCustomerReferenceFormat.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, referenceFormat))
		{
			Factory.Save();
			AssertEquals("The Arrival Reference Number is used to calculate the Cumsomer Reference", customerReference, NctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum);
			ArrivalMovementHeader.GoodsLocation.Address.AuthorisationNumber = "001";

			Factory.Save();
			AssertEquals("The Customer Refernce Number stays the same after a new Saving Procedure", customerReference, NctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum);
		}
	}

	public void TestBM_PaperlessInbondNum_AutomaticallyGeneratedBasedOffAuthorisationNumberWithChangedValue()
	{
		var customerReference1 = "AB" + ZDateTime.Today.Year.ToString() + "1CD";
		var customerReference2 = "EF" + ZDateTime.Today.Year.ToString() + "1GH";
		SetUpNctsHeaderForCustomFormatAuthorizationNumber();

		using (EU.Registry.EUCustomsDataRegistry.Instance.NctsIsManualArrivalCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		using (CHCustomsDataRegistry.Instance.ArrivalCustomerReferenceFormat.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, referenceFormat))
		{
			Factory.Save();
			AssertEquals("The Arrival Reference Number 001 is used to calculate the Cumsomer Reference", customerReference1, NctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum);
			ArrivalMovementHeader.GoodsLocation.Address.AuthorisationNumber = "003";

			Factory.Save();
			AssertEquals("The Arrival Reference Number 003 is used to calculate a new Cumsomer Reference", customerReference2, NctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum);
		}
	}

	public void TestBM_PaperlessInbondNum_AutomaticallyGeneratedBasedOffAuthorisationNumberClearedWhenEmpty()
	{
		var customerReference = "AB" + ZDateTime.Today.Year.ToString() + "1CD";
		SetUpNctsHeaderForCustomFormatAuthorizationNumber();

		using (EU.Registry.EUCustomsDataRegistry.Instance.NctsIsManualArrivalCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		using (CHCustomsDataRegistry.Instance.ArrivalCustomerReferenceFormat.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, referenceFormat))
		{
			Factory.Save();
			AssertEquals("The Arrival Reference Number is used to calculate the Cumsomer Reference", customerReference, NctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum);
			ArrivalMovementHeader.GoodsLocation.Address.AuthorisationNumber = ZString.Empty;

			Factory.Save();
			var test = NctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum;
			AssertEquals("Since no Arrival Reference Number is set on the CusGoodsLocation, the Customer Reference field is empty", ZString.Empty, NctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum);
		}
	}

	public void TestBM_PaperlessInbondNum_AutomaticallyGeneratedBasedOffAuthorisationNumberClearedNoCustomFormat()
	{
		var customerReference = "AB" + ZDateTime.Today.Year.ToString() + "1CD";
		SetUpNctsHeaderForCustomFormatAuthorizationNumber();

		using (EU.Registry.EUCustomsDataRegistry.Instance.NctsIsManualArrivalCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		using (CHCustomsDataRegistry.Instance.ArrivalCustomerReferenceFormat.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, referenceFormat))
		{
			Factory.Save();
			AssertEquals("The Arrival Reference Number is used to calculate the Cumsomer Reference", customerReference, NctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum);
			ArrivalMovementHeader.GoodsLocation.Address.AuthorisationNumber = "002";

			Factory.Save();
			var test = NctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum;
			AssertEquals("Since no AuthorisationNumber can be found on the registy format, the Customer Reference field will be empty", ZString.Empty, NctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum);
		}
	}

	public void TestDestinationCustomsOfficeCodeForArrival()
	{
		AssertEquals("ReadOnly", true, ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrivalInfo.ReadOnly);
	}

	void SetUpNctsHeaderForCustomFormatAuthorizationNumber()
	{
		var destinationTraderOrg = Factory.New<OrgHeader>();
		destinationTraderOrg.OH_Code = "1234";
		destinationTraderOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SwissCodeTypes.BID, "2233445566", "CH");
		GlbCompany.GetCurrentCompany(Factory).GC_OH_OrgProxy = destinationTraderOrg.PK;
		NctsHeader.DestinationTrader.OrganisationPK = destinationTraderOrg.PK;

		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CH.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar, destinationTraderOrg.PK, "001");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CH.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar, destinationTraderOrg.PK, "002");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CH.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar, destinationTraderOrg.PK, "003");
		ArrivalMovementHeader.GoodsLocation.Address.AuthorisationNumber = "001";

		var fallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany, null, null);
		referenceFormat = new ArrivalCustomerReferenceFormat();

		var customFormat1 = new CustomArrivalCustomerReferenceFormat(fallbackLevel, Factory);
		customFormat1.AuthorizationLocationCode = "001";
		customFormat1.Prefix = "AB";
		customFormat1.Suffix = "CD";
		customFormat1.YearOption = CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Prefix4;
		customFormat1.SequenceNumberLength = 8;
		customFormat1.IsRemoveLeadingZeros = true;
		referenceFormat.CustomFormats.Add(customFormat1);

		var customFormat2 = new CustomArrivalCustomerReferenceFormat(fallbackLevel, Factory);
		customFormat2.AuthorizationLocationCode = "003";
		customFormat2.Prefix = "EF";
		customFormat2.Suffix = "GH";
		customFormat2.YearOption = CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Prefix4;
		customFormat2.SequenceNumberLength = 8;
		customFormat2.IsRemoveLeadingZeros = true;
		referenceFormat.CustomFormats.Add(customFormat2);

		referenceFormat.UseSystemDefinedFormat = false;
	}

	public void TestCarrier()
	{
		var oldCarrier = ArrivalMovementHeader.Carrier;
		oldCarrier.Delete();

		var carrier = ArrivalMovementHeader.Carrier;
		AssertNotEquals("New Carrier created", oldCarrier.PK, carrier.PK);
		AssertSame("Cached", carrier, ArrivalMovementHeader.Carrier);

		carrier.Delete();
		carrier = ArrivalMovementHeader.DocAddresses.CreateWithRequirement(ArrivalMovementHeader.CarrierJobDocAddressRequirement);
		AssertEquals("Carrier from DocAddresses", carrier.PK, ArrivalMovementHeader.Carrier.PK);

		carrier.E2_AddressOverride = true;
		carrier.CompanyName = ZString.Empty;
		carrier.Address1 = ZString.Empty;
		carrier.Address2 = ZString.Empty;
		carrier.City = ZString.Empty;
		carrier.Postcode = ZString.Empty;
		carrier.State = ZString.Empty;
		carrier.E2_RN_NKCountryCode = ZString.Empty;
		carrier.Validation.ValidateAll();

		CombineAssertions(() =>
		{
			AssertNoNotifications(carrier.CompanyNameInfo);
			AssertNoNotifications(carrier.Address1Info);
			AssertNoNotifications(carrier.Address2Info);
			AssertNoNotifications(carrier.CityInfo);
			AssertNoNotifications(carrier.PostcodeInfo);
			AssertNoNotifications(carrier.StateCodeInfo);
			AssertNoNotifications(carrier.E2_RN_NKCountryCodeInfo);
		});
	}

	public void TestCarrierJobDocAddressRequirement()
	{
		AssertNotNull("CarrierJobDocAddressRequirement", ArrivalMovementHeader.CarrierJobDocAddressRequirement);
		var carrierJobDocAddressRequirement = ArrivalMovementHeader.CarrierJobDocAddressRequirement;
		AssertSame("CarrierJobDocAddressRequirement Cached", carrierJobDocAddressRequirement, ArrivalMovementHeader.CarrierJobDocAddressRequirement);
	}

	public void TestIDocAddressesMembers()
	{
		var docAddresses = ArrivalMovementHeader as IDocAddresses;
		AssertNotNull("DepartureMovement as IDocAddresses", docAddresses);
		AssertSequencesEqual(new[] { DocAddressType.Representative, DocAddressType.Carrier }, docAddresses.SupportedAddressTypes);
	}

	protected override void TestBizObjectField(ZPropertyInfo info)
	{
		if (info.Name != "DestinationCustomsOfficeCodeForDeparture"
			&& info.Name != "DestinationCustomsOfficeCodeForArrival")
		{
			base.TestBizObjectField(info);
		}
	}

	public void TestSetReadOnlyForUnloadingDifferencesDataWithSealsStateY() => TestSetReadOnlyForUnloadingDifferencesData("Y");

	public void TestSetReadOnlyForUnloadingDifferencesDataWithSealsStateN() => TestSetReadOnlyForUnloadingDifferencesData("N");

	void TestSetReadOnlyForUnloadingDifferencesData(string stateOfSeals) => CombineAssertions(() =>
	{
		ArrivalMovementHeader.BM_StateOfSeals = stateOfSeals;

		var containers = ArrivalMovementHeader.Header.ArrivalHeaderContainers;
		var container = containers.AddNew();
		var seals = container.Seals;
		var seal = seals.AddNew();
		seal.BK_SealNumber = "1";
		Factory.Save();

		var arrivalMovementHeader = new BusinessObjectFactory().Load<NctsArrivalMovementHeader>(ArrivalMovementHeader.PK);
		containers = ArrivalMovementHeader.Header.ArrivalHeaderContainers;
		container = containers[0];
		seals = container.Seals;
		seal = seals[0];

		ArrivalMovementHeader.BM_NoChangesToReport = ZBool.False;
		AssertReadOnly($"BM_NoChangesToReport={ArrivalMovementHeader.BM_NoChangesToReport}", false);

		ArrivalMovementHeader.BM_NoChangesToReport = ZBool.True;
		AssertReadOnly($"BM_NoChangesToReport={ArrivalMovementHeader.BM_NoChangesToReport}", true);

		void AssertReadOnly(string assertionMessage, bool expectedReadOnly)
		{
			AssertEquals($"{assertionMessage} Container collection", expectedReadOnly, containers.ReadOnly);
			AssertEquals($"{assertionMessage} Container", expectedReadOnly, container.ReadOnly);
			AssertEquals($"{assertionMessage} Seals collection", expectedReadOnly, seals.ReadOnly);
			AssertEquals($"{assertionMessage} Seal", expectedReadOnly, seal.ReadOnly);
		}
	});

	public void TestPropertiesReadOnlyIfLockedARN() => CombineAssertions(() =>
	{
		using (new LockForEditTestHelper(Factory, EUJobMessageTypeList.Codes.NctsArrivalNotification, DeclarationTabPages.Codes.NctsArrivalNotification))
		{
			ArrivalMovementHeader.Header.LockFile("Test lock");
			AssertProperties("Locked", true);
			ArrivalMovementHeader.Header.UnlockFile("Test unlock");
			AssertProperties("Unlocked", false);
		}

		void AssertProperties(string assertionMessage, bool expectedReadOnly)
		{
			AssertEquals($"{assertionMessage} - BM_TransportAtArrivalTypeInfo.ReadOnly", expectedReadOnly, ArrivalMovementHeader.BM_TransportAtArrivalTypeInfo.ReadOnly);
			AssertEquals($"{assertionMessage} - BM_TransportAtArrivalIDInfo.ReadOnly", expectedReadOnly, ArrivalMovementHeader.BM_TransportAtArrivalIDInfo.ReadOnly);
			AssertEquals($"{assertionMessage} - BM_RN_NKTransportAtArrivalIDNationalityInfo.ReadOnly", expectedReadOnly, ArrivalMovementHeader.BM_RN_NKTransportAtArrivalIDNationalityInfo.ReadOnly);
			AssertEquals($"{assertionMessage} - BM_StateOfSealsInfo.ReadOnly", expectedReadOnly, ArrivalMovementHeader.BM_StateOfSealsInfo.ReadOnly);
			AssertEquals($"{assertionMessage} - BM_AdditionalTextInfo.ReadOnly", expectedReadOnly, ArrivalMovementHeader.BM_AdditionalTextInfo.ReadOnly);
		}
	});

	public void TestIsAdditionalGoodsInformationLocked() => CombineAssertions(() =>
	{
		using (new LockForEditTestHelper(Factory, EUJobMessageTypeList.Codes.NctsArrivalNotification, DeclarationTabPages.Codes.NctsArrivalNotification))
		{
			ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
			AssertEquals("Not locked by status SNT when AGO not configured", false, ArrivalMovementHeader.IsAdditionalGoodsInformationLocked);
		}
		using (new LockForEditTestHelper(Factory, EUJobMessageTypeList.Codes.NctsArrivalNotification, DeclarationTabPages.Codes.NctsArrivalAdditionalGoodsInformation))
		{
			AssertEquals("Locked by status SNT", true, ArrivalMovementHeader.IsAdditionalGoodsInformationLocked);

			ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Acknowledged;
			AssertEquals("Locked by status ACK", true, ArrivalMovementHeader.IsAdditionalGoodsInformationLocked);

			ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Invalid;
			AssertEquals("Unlocked", false, ArrivalMovementHeader.IsAdditionalGoodsInformationLocked);

			NctsHeader.LockFile("Test");
			AssertEquals("Locked by event", true, ArrivalMovementHeader.IsAdditionalGoodsInformationLocked);
		}

		AssertEquals("Not locked by event when not configured", false, ArrivalMovementHeader.IsAdditionalGoodsInformationLocked);
	});

	public void TestCanLockUnlockAGO() => CombineAssertions(() =>
	{
		using (new LockForEditTestHelper(Factory, EUJobMessageTypeList.Codes.NctsArrivalNotification, DeclarationTabPages.Codes.NctsArrivalNotification))
		{
			AssertEquals("Not configured (only other tab)", false, ArrivalMovementHeader.CanLockUnlockAGO);
		}

		using (new LockForEditTestHelper(Factory, EUJobMessageTypeList.Codes.NctsArrivalNotification, DeclarationTabPages.Codes.NctsArrivalAdditionalGoodsInformation))
		{
			AssertEquals("AGO tab configured", true, ArrivalMovementHeader.CanLockUnlockAGO);
		}

		using (new LockForEditTestHelper(Factory, EUJobMessageTypeList.Codes.NctsArrivalNotification, DeclarationTabPages.Codes.All))
		{
			AssertEquals("ALL tabs configured", true, ArrivalMovementHeader.CanLockUnlockAGO);
		}
	});

	public void TestIsClosedRelease() => CombineAssertions(() =>
	{
		ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
		AssertEquals("IsClosedRelease returns true when the BM_CustomsStatus is set to CL1", true, ArrivalMovementHeader.IsClosedRelease);

		ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease;
		AssertEquals("IsClosedRelease returns true when the BM_CustomsStatus is set to CL3", true, ArrivalMovementHeader.IsClosedRelease);

		ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.CHClear;
		AssertEquals("IsClosedRelease returns true when the BM_CustomsStatus is set to anything but CL1 or CL3", false, ArrivalMovementHeader.IsClosedRelease);
	});

	public void TestGuaranteesForArrival() => AssertType<NctsGuarantee>(ArrivalMovementHeader.GuaranteesForArrival.AddNew());

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;

	NctsArrivalMovementHeader ArrivalMovementHeader => arrivalMovementHeader ?? (arrivalMovementHeader = NctsHeader.ArrivalMovementHeader);
	NctsArrivalMovementHeader arrivalMovementHeader;

	ArrivalCustomerReferenceFormat referenceFormat;
	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader;
	}

	NctsArrivalMovementHeader CreateNctsArrivalMovementHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.ArrivalMovementHeader;
	}
}
