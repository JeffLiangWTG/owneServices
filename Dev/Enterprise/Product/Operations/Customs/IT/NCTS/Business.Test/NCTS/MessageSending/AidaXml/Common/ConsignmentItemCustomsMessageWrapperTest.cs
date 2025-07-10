using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class ConsignmentItemCustomsMessageWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(
			"When goodsItem is null",
			() => CreateNewWrapper(goodsItem: null));
	}

	public void TestDeclarationType()
	{
		goodsItem.BY_Type = "T";

		var wrapper = CreateNewWrapper(goodsItem);
		AssertEquals(nameof(IConsignmentItemCustomsMessageWrapper.DeclarationType), "T", wrapper.DeclarationType);
	}

	public void TestGoodsItemNumber()
	{
		goodsItem.BY_LineNo = 1;

		var wrapper = CreateNewWrapper(goodsItem);
		AssertEquals(nameof(IConsignmentItemCustomsMessageWrapper.GoodsItemNumber), 1, wrapper.GoodsItemNumber);
	}

	public void TestDeclarationGoodsItemNumber()
	{
		goodsItem.BY_DeclarationGoodsItemNumber = 9;

		var wrapper = CreateNewWrapper(goodsItem);
		AssertEquals(nameof(IConsignmentItemCustomsMessageWrapper.DeclarationGoodsItemNumber), 9, wrapper.DeclarationGoodsItemNumber);
	}

	public void TestPreviousDocuments()
	{
		var wrapper = CreateNewWrapper(goodsItem);
		AssertNotNull(nameof(IConsignmentItemCustomsMessageWrapper.PreviousDocuments), wrapper.PreviousDocuments);
		AssertEquals($"{nameof(IConsignmentItemCustomsMessageWrapper.PreviousDocuments)}, Count", 0, wrapper.PreviousDocuments.Count);

		goodsItem.PreviousDocuments.AddNew();
		wrapper = CreateNewWrapper(goodsItem);
		var previousDocuments = wrapper.PreviousDocuments;
		AssertNotNull(nameof(IConsignmentItemCustomsMessageWrapper.PreviousDocuments), previousDocuments);
		AssertEquals($"{nameof(IConsignmentItemCustomsMessageWrapper.PreviousDocuments)}, Count", 1, previousDocuments.Count);
		AssertType<ConsignmentItemPreviousDocument>($"{nameof(IConsignmentItemCustomsMessageWrapper.PreviousDocuments)}, Type", previousDocuments.Single());
		AssertSame($"{nameof(IConsignmentItemCustomsMessageWrapper.PreviousDocuments)}, Cached", previousDocuments, wrapper.PreviousDocuments);
	}

	public void TestAdditionalInformation()
	{
		var wrapper = CreateNewWrapper(goodsItem);
		AssertNotNull(nameof(IConsignmentItemCustomsMessageWrapper.AdditionalInformation), wrapper.AdditionalInformation);
		AssertEquals($"{nameof(IConsignmentItemCustomsMessageWrapper.AdditionalInformation)}, Count", 0, wrapper.AdditionalInformation.Count);

		goodsItem.AdditionalInfos.AddNew();
		goodsItem.AdditionalInfos.AddNew().CSI_SubType = "INF";
		wrapper = CreateNewWrapper(goodsItem);
		var additionalInformation = wrapper.AdditionalInformation;
		AssertNotNull(nameof(IConsignmentItemCustomsMessageWrapper.AdditionalInformation), additionalInformation);
		AssertEquals($"{nameof(IConsignmentItemCustomsMessageWrapper.AdditionalInformation)}, Count", 1, additionalInformation.Count);
		AssertType<AdditionalInformationWrapper>($"{nameof(IConsignmentItemCustomsMessageWrapper.AdditionalInformation)}, Type", additionalInformation.Single());
		AssertSame($"{nameof(IConsignmentItemCustomsMessageWrapper.AdditionalInformation)}, Cached", additionalInformation, wrapper.AdditionalInformation);
	}

	public void TestSupportingDocuments()
	{
		var wrapper = CreateNewWrapper(goodsItem);
		AssertNotNull(nameof(IConsignmentItemCustomsMessageWrapper.SupportingDocuments), wrapper.SupportingDocuments);
		AssertEquals($"{nameof(IConsignmentItemCustomsMessageWrapper.SupportingDocuments)}, Count", 0, wrapper.SupportingDocuments.Count);

		goodsItem.SupportingDocuments.AddNew();
		wrapper = CreateNewWrapper(goodsItem);
		var supportingDocuments = wrapper.SupportingDocuments;
		AssertNotNull(nameof(IConsignmentItemCustomsMessageWrapper.SupportingDocuments), supportingDocuments);
		AssertEquals($"{nameof(IConsignmentItemCustomsMessageWrapper.SupportingDocuments)}, Count", 1, supportingDocuments.Count);
		AssertType<SupportingDocumentWrapper>($"{nameof(IConsignmentItemCustomsMessageWrapper.SupportingDocuments)}, Type", supportingDocuments.Single());
		AssertSame($"{nameof(IConsignmentItemCustomsMessageWrapper.SupportingDocuments)}, Cached", supportingDocuments, wrapper.SupportingDocuments);
	}

	public void TestTransportDocumentsAreEmptyByDefault()
	{
		var wrapper = CreateNewWrapper(goodsItem);
		AssertNotNull(nameof(IConsignmentItemCustomsMessageWrapper.TransportDocuments), wrapper.TransportDocuments);
		AssertEquals($"{nameof(IConsignmentItemCustomsMessageWrapper.TransportDocuments)}, Count", 0, wrapper.TransportDocuments.Count);
	}

	public void TestTransportDocumentsAreMappedWhenTransitionPeriodIsOn()
	{
		goodsItem.Bill.AdditionalDocuments.AddNew();
		goodsItem.Bill.AdditionalDocuments.AddNew().CSI_SubType = "TRA";

		using (SetTransitionPeriod(isActive: true))
		{
			var wrapper = CreateNewWrapper(goodsItem);
			var transportDocuments = wrapper.TransportDocuments;
			AssertNotNull(nameof(IConsignmentItemCustomsMessageWrapper.TransportDocuments), transportDocuments);
			AssertEquals($"{nameof(IConsignmentItemCustomsMessageWrapper.TransportDocuments)}, Count", 1, transportDocuments.Count);
			AssertType<TransportDocumentWrapper>($"{nameof(IConsignmentItemCustomsMessageWrapper.TransportDocuments)}, Type", transportDocuments.Single());
			AssertSame($"{nameof(IConsignmentItemCustomsMessageWrapper.TransportDocuments)}, Cached", transportDocuments, wrapper.TransportDocuments);
		}
	}

	public void TestTransportDocumentAreNotMappedsWhenTransitionPeriodIsOff()
	{
		goodsItem.Bill.AdditionalDocuments.AddNew();
		goodsItem.Bill.AdditionalDocuments.AddNew().CSI_SubType = "TRA";

		using (SetTransitionPeriod(isActive: false))
		{
			var wrapper = CreateNewWrapper(goodsItem);
			var transportDocuments = wrapper.TransportDocuments;
			AssertNotNull(nameof(IConsignmentItemCustomsMessageWrapper.TransportDocuments), transportDocuments);
			AssertEquals($"{nameof(IConsignmentItemCustomsMessageWrapper.TransportDocuments)}, Count", 0, transportDocuments.Count);
		}
	}

	public void TestAdditionalReferences()
	{
		var wrapper = CreateNewWrapper(goodsItem);
		AssertNotNull(nameof(IConsignmentItemCustomsMessageWrapper.AdditionalReferences), wrapper.AdditionalReferences);
		AssertEquals($"{nameof(IConsignmentItemCustomsMessageWrapper.AdditionalReferences)}, Count", 0, wrapper.AdditionalReferences.Count);

		goodsItem.AdditionalInfos.AddNew().CSI_SubType = "TRA";
		goodsItem.AdditionalInfos.AddNew().CSI_SubType = "REF";
		wrapper = CreateNewWrapper(goodsItem);
		var additionalReferences = wrapper.AdditionalReferences;
		AssertNotNull(nameof(IConsignmentItemCustomsMessageWrapper.AdditionalReferences), additionalReferences);
		AssertEquals($"{nameof(IConsignmentItemCustomsMessageWrapper.AdditionalReferences)}, Count", 1, additionalReferences.Count);
		AssertType<AdditionalReferenceWrapper>($"{nameof(IConsignmentItemCustomsMessageWrapper.AdditionalReferences)}, Type", additionalReferences.Single());
		AssertSame($"{nameof(IConsignmentItemCustomsMessageWrapper.AdditionalReferences)}, Cached", additionalReferences, wrapper.AdditionalReferences);
	}

	public void TestUcr()
	{
		goodsItem.BY_CommercialReferenceNumber = ZString.Empty;
		var wrapper = CreateNewWrapper(goodsItem);
		AssertNullOrEmpty(nameof(IConsignmentItemCustomsMessageWrapper.Ucr), wrapper.Ucr);

		goodsItem.BY_CommercialReferenceNumber = "ABC";
		wrapper = CreateNewWrapper(goodsItem);
		AssertEquals(nameof(IConsignmentItemCustomsMessageWrapper.Ucr), "ABC", wrapper.Ucr);
	}

	public void TestConsignee()
	{
		var wrapper = CreateNewWrapper(goodsItem);
		AssertNull(nameof(IConsignmentItemCustomsMessageWrapper.Consignee), wrapper.Consignee);

		var consignee = Factory.NewWithValidTestData<OrgHeader>();
		goodsItem.Consignee.OrganisationPK = consignee.PK;
		wrapper = CreateNewWrapper(goodsItem);
		AssertNotNull(nameof(IConsignmentItemCustomsMessageWrapper.Consignee), wrapper.Consignee);
	}

	public void TestAdditionalSupplyChainActors()
	{
		var wrapper = CreateNewWrapper(goodsItem);
		AssertNotNull(nameof(IConsignmentItemCustomsMessageWrapper.AdditionalSupplyChainActors), wrapper.AdditionalSupplyChainActors);
		AssertEquals($"{nameof(IConsignmentItemCustomsMessageWrapper.AdditionalSupplyChainActors)}, Count", 0, wrapper.AdditionalSupplyChainActors.Count);

		goodsItem.CusSupplyChainActorReferences.AddNew();
		wrapper = CreateNewWrapper(goodsItem);
		var additionalSupplyChainActors = wrapper.AdditionalSupplyChainActors;
		AssertNotNull(nameof(IConsignmentItemCustomsMessageWrapper.AdditionalSupplyChainActors), additionalSupplyChainActors);
		AssertEquals($"{nameof(IConsignmentItemCustomsMessageWrapper.AdditionalSupplyChainActors)}, Count", 1, additionalSupplyChainActors.Count);
		AssertType<AdditionalSupplyChainActorWrapper>($"{nameof(IConsignmentItemCustomsMessageWrapper.AdditionalSupplyChainActors)}, Type", additionalSupplyChainActors.Single());
		AssertSame($"{nameof(IConsignmentItemCustomsMessageWrapper.AdditionalSupplyChainActors)}, Cached", additionalSupplyChainActors, wrapper.AdditionalSupplyChainActors);
	}

	public void TestCountryOfDispatch()
	{
		goodsItem.BY_RN_NKCountryOfDispatch = "US";

		var wrapper = CreateNewWrapper(goodsItem);
		AssertEquals(nameof(IConsignmentItemCustomsMessageWrapper.CountryOfDispatch), "US", wrapper.CountryOfDispatch);
	}

	public void TestCountryOfDestination()
	{
		AssertWrapperFieldWithSingleLine("CountryOfDestination"
		, (goodsItem, value) => { goodsItem.BY_RN_NKCountryOfDestination = value; }
		, (movementHeader, value) => { movementHeader.BM_RL_NKDestinationPort = value; }
		, (wrapper) => wrapper.CountryOfDestination);

		AssertWrapperFieldWithMultiLine("CountryOfDestination"
		, (goodsItem, value) => { goodsItem.BY_RN_NKCountryOfDestination = value; }
		, (goodsItem, value) => { goodsItem.BM_RL_NKDestinationPort = value; }
		, (wrapper) => wrapper.CountryOfDestination);
	}

	public void TestTransportChargesMethodOfPaymentWhenTransitionPeriodIsOn()
	{
		using (SetTransitionPeriod(true))
		{
			AssertWrapperFieldWithSingleLine("MethodOfPayment",
			(goodsItem, value) => { goodsItem.BY_TransportChargesMethodOfPayment = value; },
			(movementHeader, value) => { movementHeader.BM_MethodOfPayment = value; },
			(wrapper) => wrapper.TransportChargesMethodOfPayment);

			AssertWrapperFieldWithMultiLine("MethodOfPayment",
			(goodsItem, value) => { goodsItem.BY_TransportChargesMethodOfPayment = value; },
			(goodsItem, value) => { goodsItem.BM_MethodOfPayment = value; },
			(wrapper) => wrapper.TransportChargesMethodOfPayment);
		}
	}

	public void TestTransportChargesMethodOfPaymentWhenTransitionPeriodIsOff()
	{
		using (SetTransitionPeriod(false))
		{
			var movementHeader = nctsHeader.MovementHeader;
			var bill = nctsHeader.Bills[0];
			var goodsItem1 = bill.GoodsItems[0];
			var goodsItem2 = bill.GoodsItems.AddNew();
			var wrapper1 = CreateNewWrapper(goodsItem1);
			var wrapper2 = CreateNewWrapper(goodsItem2);

			movementHeader.BM_MethodOfPayment = "A";
			goodsItem1.BY_TransportChargesMethodOfPayment = "B";
			goodsItem2.BY_TransportChargesMethodOfPayment = "B";

			wrapper1 = CreateNewWrapper(goodsItem1);
			wrapper2 = CreateNewWrapper(goodsItem2);
			CombineAssertions("Outside TP, when header is filled and all lines have same MethodOfPayment", () =>
			{
				AssertEquals("Line 1 value", "B", wrapper1.TransportChargesMethodOfPayment);
				AssertEquals("Line 2 value", "B", wrapper2.TransportChargesMethodOfPayment);
			});

			goodsItem2.BY_TransportChargesMethodOfPayment = ZString.Empty;
			wrapper1 = CreateNewWrapper(goodsItem1);
			wrapper2 = CreateNewWrapper(goodsItem2);
			CombineAssertions("Outside TP, when header is filled and all lines have different MethodOfPayment", () =>
			{
				AssertEquals("Line 1 value", "B", wrapper1.TransportChargesMethodOfPayment);
				AssertEquals("Line 2 value", ZString.Empty, wrapper2.TransportChargesMethodOfPayment);
			});
		}
	}

	public void TestNetMassReturnsNullIfZero()
	{
		goodsItem.BY_NetWeight = 0m;
		goodsItem.BY_NetWeightUnit = "KG";
		var wrapper = CreateNewWrapper(goodsItem);
		AssertNull(nameof(IConsignmentItemCustomsMessageWrapper.NetMass), wrapper.NetMass);
	}

	public void TestNetMassReturnsValueIfBillHasN830PreviousDocument()
	{
		bill.PreviousDocuments.AddNew().CSI_Code = "N830";

		goodsItem.BY_NetWeight = 123m;
		goodsItem.BY_NetWeightUnit = "KG";
		var wrapper = CreateNewWrapper(goodsItem);
		AssertEquals(nameof(IConsignmentItemCustomsMessageWrapper.NetMass), 123m, wrapper.NetMass);
	}

	public void TestNetMassReturnsValueIfNotReducedDatasetIndicator()
	{
		nctsHeader.MovementHeader.BM_ReducedDatasetIndicator = false;

		goodsItem.BY_NetWeight = 123m;
		goodsItem.BY_NetWeightUnit = "KG";
		var wrapper = CreateNewWrapper(goodsItem);
		AssertEquals(nameof(IConsignmentItemCustomsMessageWrapper.NetMass), 123m, wrapper.NetMass);
	}

	public void TestNetMassIsConvertedToKilograms()
	{
		nctsHeader.MovementHeader.BM_ReducedDatasetIndicator = false;

		goodsItem.BY_NetWeight = 123m;
		goodsItem.BY_NetWeightUnit = "G";
		var wrapper = CreateNewWrapper(goodsItem);
		AssertEquals(nameof(IConsignmentItemCustomsMessageWrapper.NetMass), 0.123m, wrapper.NetMass);
	}

	public void TestNetMassReturnsNullIfBillHasNoN830PreviousDocumentAndHasReducedDatasetIndicator()
	{
		bill.PreviousDocuments.RemoveAndDeleteAll();
		nctsHeader.MovementHeader.BM_ReducedDatasetIndicator = true;

		goodsItem.BY_NetWeight = 123m;
		goodsItem.BY_NetWeightUnit = "KG";
		var wrapper = CreateNewWrapper(goodsItem);
		AssertNull(nameof(IConsignmentItemCustomsMessageWrapper.NetMass), wrapper.NetMass);
	}

	public void TestGrossMass()
	{
		goodsItem.BY_GrossWeight = 0m;
		var wrapper = CreateNewWrapper(goodsItem);
		AssertNull(nameof(IConsignmentItemCustomsMessageWrapper.GrossMass), wrapper.GrossMass);

		goodsItem.BY_GrossWeight = 123m;
		goodsItem.BY_GrossWeightUnit = "KG";
		wrapper = CreateNewWrapper(goodsItem);
		AssertEquals(nameof(IConsignmentItemCustomsMessageWrapper.GrossMass), 123m, wrapper.GrossMass);

		goodsItem.BY_GrossWeightUnit = "G";
		wrapper = CreateNewWrapper(goodsItem);
		AssertEquals(nameof(IConsignmentItemCustomsMessageWrapper.GrossMass), 0.123m, wrapper.GrossMass);
	}

	public void TestSupplementaryUnits()
	{
		goodsItem.BY_CustomsSecondQuantity = 0m;
		var wrapper = CreateNewWrapper(goodsItem);
		AssertNull(nameof(IConsignmentItemCustomsMessageWrapper.SupplementaryUnits), wrapper.SupplementaryUnits);

		goodsItem.BY_CustomsSecondQuantity = 1.23m;
		wrapper = CreateNewWrapper(goodsItem);
		AssertEquals(nameof(IConsignmentItemCustomsMessageWrapper.SupplementaryUnits), 1.23m, wrapper.SupplementaryUnits);
	}

	public void TestDescriptionOfGoods()
	{
		goodsItem.BY_Description = "DESC";

		var wrapper = CreateNewWrapper(goodsItem);
		AssertEquals(nameof(IConsignmentItemCustomsMessageWrapper.DescriptionOfGoods), "DESC", wrapper.DescriptionOfGoods);
	}

	public void TestPackages()
	{
		var wrapper = CreateNewWrapper(goodsItem);
		AssertNotNull(nameof(IConsignmentItemCustomsMessageWrapper.Packages), wrapper.Packages);
		AssertEquals($"{nameof(IConsignmentItemCustomsMessageWrapper.Packages)}, Count", 0, wrapper.Packages.Count);

		goodsItem.Packages.AddNew();
		wrapper = CreateNewWrapper(goodsItem);
		var packages = wrapper.Packages;
		AssertNotNull(nameof(IConsignmentItemCustomsMessageWrapper.Packages), packages);
		AssertEquals($"{nameof(IConsignmentItemCustomsMessageWrapper.Packages)}, Count", 1, packages.Count);
		AssertType<PackageWrapper>($"{nameof(IConsignmentItemCustomsMessageWrapper.Packages)}, Type", packages.Single());
		AssertSame($"{nameof(IConsignmentItemCustomsMessageWrapper.Packages)}, Cached", packages, wrapper.Packages);
	}

	public void TestCusCode()
	{
		goodsItem.BY_CusC4Number = "01";

		var wrapper = CreateNewWrapper(goodsItem);
		AssertEquals(nameof(IConsignmentItemCustomsMessageWrapper.CusCode), "01", wrapper.CusCode);
	}

	public void TestHsTariffCode()
	{
		var wrapper = CreateNewWrapper(goodsItem);
		AssertEquals(nameof(IConsignmentItemCustomsMessageWrapper.HsTariffCode), "", wrapper.HsTariffCode);

		goodsItem.BY_HarmonisedTariff = "12345678";
		wrapper = CreateNewWrapper(goodsItem);
		AssertEquals(nameof(IConsignmentItemCustomsMessageWrapper.HsTariffCode), "123456", wrapper.HsTariffCode);
	}

	public void TestNcTariffCode()
	{
		var wrapper = CreateNewWrapper(goodsItem);
		AssertEquals(nameof(IConsignmentItemCustomsMessageWrapper.NcTariffCode), "", wrapper.NcTariffCode);

		goodsItem.BY_HarmonisedTariff = "12345678";
		wrapper = CreateNewWrapper(goodsItem);
		AssertEquals(nameof(IConsignmentItemCustomsMessageWrapper.NcTariffCode), "78", wrapper.NcTariffCode);
	}

	public void TestNcTariffCodeWhenCustomOfficeOfDeparturePresentInCL112Set()
	{
		goodsItem.BY_HarmonisedTariff = "12345678";

		var startDate = ZDateTime.Today.AddDays(-2);
		var endDate = ZDateTime.Today.AddDays(2);

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingCusCodeType(EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "CL112 Desc.");
		helper.CreateCusCodeList(RefDataGroupingCodes.EuropeanUnionEUN, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "GB", "Polland", startDate, endDate);
		helper.CreateCusCodeList(RefDataGroupingCodes.EuropeanUnionEUN, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "UA", "Andorra", startDate, endDate);
		Factory.Save();

		var customsOffice = goodsItem.Bill.Header.CustomsOfficesForDeparture.AddNew();
		customsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
		customsOffice.CY_Data = "GB1234";

		var wrapper = CreateNewWrapper(goodsItem);

		AssertNull(nameof(IConsignmentItemCustomsMessageWrapper.NcTariffCode), wrapper.NcTariffCode);

		wrapper = CreateNewWrapper(goodsItem);

		customsOffice.CY_Data = "PL123";
		AssertEquals(nameof(IConsignmentItemCustomsMessageWrapper.NcTariffCode), "78", wrapper.NcTariffCode);
	}

	public void TestDangerousGoodsCodes()
	{
		var wrapper = CreateNewWrapper(goodsItem);
		AssertEquals($"{nameof(IConsignmentItemCustomsMessageWrapper.DangerousGoodsCodes)} Count", 0, wrapper.DangerousGoodsCodes.Count);

		var substance0010 = UNDGSubstanceLoader.LoadSubstances(Factory, "0010", "", "IMO").First();
		var substance0012a = UNDGSubstanceLoader.LoadSubstances(Factory, "0012", "a", "IMO").First();

		goodsItem.UNDGs.AddNew();
		goodsItem.UNDGs.AddNew().DI_DG = substance0010.PK;
		goodsItem.UNDGs.AddNew().DI_DG = substance0012a.PK;
		goodsItem.UNDGs.AddNew().DI_DG = substance0012a.PK;
		wrapper = CreateNewWrapper(goodsItem);
		AssertContainsExactElementsInAnyOrder($"{nameof(IConsignmentItemCustomsMessageWrapper.DangerousGoodsCodes)} Elements", new[] { "0010", "0012" }, wrapper.DangerousGoodsCodes.ToArray());
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.NewDepartureNctsHeader();
		bill = nctsHeader.Bills.AddNew();
		goodsItem = bill.GoodsItems.AddNew();
	}

	IConsignmentItemCustomsMessageWrapper CreateNewWrapper(NctsDepartureCargoDesc goodsItem)
		=> new ConsignmentItemCustomsMessageWrapper(goodsItem);

	IDisposable SetTransitionPeriod(bool isActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isActive);

	void AssertWrapperFieldWithMultiLine(string fieldName,
		Action<NctsDepartureCargoDesc, string> setLineFieldTo,
		Action<NctsDepartureMovementHeader, string> setHeaderTo,
		Func<IConsignmentItemCustomsMessageWrapper, string> getWrapperFieldValue)
	{
		var movementHeader = nctsHeader.MovementHeader;
		var bill = nctsHeader.Bills[0];
		var goodsItem1 = bill.GoodsItems[0];
		var goodsItem2 = bill.GoodsItems.AddNew();

		CombineAssertions($"{fieldName} only in lines", () =>
		{
			setHeaderTo(movementHeader, string.Empty);
			setLineFieldTo(goodsItem1, "X");
			setLineFieldTo(goodsItem2, string.Empty);

			var wrapper1 = CreateNewWrapper(goodsItem1);
			var wrapper2 = CreateNewWrapper(goodsItem2);
			AssertEquals($"When {fieldName} is only available in one line, in line 1", "X", getWrapperFieldValue(wrapper1));
			AssertEquals($"When {fieldName} is only available in one line, in line 2", string.Empty, getWrapperFieldValue(wrapper2));

			setLineFieldTo(goodsItem1, "Y");
			setLineFieldTo(goodsItem2, "Y");
			wrapper1 = CreateNewWrapper(goodsItem1);
			wrapper2 = CreateNewWrapper(goodsItem2);
			AssertEquals($"When all lines have same {fieldName}, {fieldName}", string.Empty, getWrapperFieldValue(wrapper1));
			AssertEquals($"When all lines have same {fieldName}, {fieldName}", string.Empty, getWrapperFieldValue(wrapper2));

			setLineFieldTo(goodsItem2, "Z");
			wrapper1 = CreateNewWrapper(goodsItem1);
			wrapper2 = CreateNewWrapper(goodsItem2);
			AssertEquals($"When lines have different {fieldName}, {fieldName}", "Y", getWrapperFieldValue(wrapper1));
			AssertEquals($"When lines have different {fieldName}, {fieldName}", "Z", getWrapperFieldValue(wrapper2));

			setLineFieldTo(goodsItem2, string.Empty);
			wrapper1 = CreateNewWrapper(goodsItem1);
			wrapper2 = CreateNewWrapper(goodsItem2);
			AssertEquals($"When lines have different {fieldName} (one does not have value), {fieldName}", "Y", getWrapperFieldValue(wrapper1));
			AssertEquals($"When lines have different {fieldName} (one does not have value), {fieldName}", string.Empty, getWrapperFieldValue(wrapper2));

			setLineFieldTo(goodsItem1, string.Empty);
			wrapper1 = CreateNewWrapper(goodsItem1);
			wrapper2 = CreateNewWrapper(goodsItem2);
			AssertEquals($"When lines have same {fieldName} (all do not have value), {fieldName}", string.Empty, getWrapperFieldValue(wrapper1));
			AssertEquals($"When lines have same {fieldName} (all do not have value), {fieldName}", string.Empty, getWrapperFieldValue(wrapper2));
		});

		CombineAssertions($"{fieldName} both in lines and header", () =>
		{
			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(goodsItem1, "Y");
			setLineFieldTo(goodsItem2, "Y");
			var wrapper1 = CreateNewWrapper(goodsItem1);
			var wrapper2 = CreateNewWrapper(goodsItem2);
			AssertEquals($"When {fieldName} is the same in lines and header, {fieldName}", string.Empty, getWrapperFieldValue(wrapper1));
			AssertEquals($"When {fieldName} is the same in lines and header, {fieldName}", string.Empty, getWrapperFieldValue(wrapper2));

			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(goodsItem1, "X");
			setLineFieldTo(goodsItem2, "Z");
			wrapper1 = CreateNewWrapper(goodsItem1);
			wrapper2 = CreateNewWrapper(goodsItem2);
			AssertEquals($"When {fieldName} is present in lines and header and different in all cases, {fieldName}", "X", getWrapperFieldValue(wrapper1));
			AssertEquals($"When {fieldName} is present in lines and header and different in all cases, {fieldName}", "Z", getWrapperFieldValue(wrapper2));

			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(goodsItem1, "X");
			setLineFieldTo(goodsItem2, string.Empty);
			wrapper1 = CreateNewWrapper(goodsItem1);
			wrapper2 = CreateNewWrapper(goodsItem2);
			AssertEquals($"When {fieldName} is missing only in one or more lines but present everywhere, {fieldName}", "X", getWrapperFieldValue(wrapper1));
			AssertEquals($"When {fieldName} is missing only in one or more lines but present everywhere, {fieldName}", "Y", getWrapperFieldValue(wrapper2));

			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(goodsItem1, string.Empty);
			setLineFieldTo(goodsItem2, string.Empty);
			wrapper1 = CreateNewWrapper(goodsItem1);
			wrapper2 = CreateNewWrapper(goodsItem2);
			AssertEquals($"When {fieldName} is only available at header level, {fieldName}", string.Empty, getWrapperFieldValue(wrapper1));
			AssertEquals($"When {fieldName} is only available at header level, {fieldName}", string.Empty, getWrapperFieldValue(wrapper2));
		});
	}

	void AssertWrapperFieldWithSingleLine(string fieldName,
		Action<NctsDepartureCargoDesc, string> setLineFieldTo,
		Action<NctsDepartureMovementHeader, string> setHeaderTo,
		Func<IConsignmentItemCustomsMessageWrapper, string> getWrapperFieldValue)
	{
		var movementHeader = goodsItem.MoveHeader;

		CombineAssertions(() =>
		{
			setHeaderTo(movementHeader, "X");
			setLineFieldTo(goodsItem, string.Empty);

			var wrapper = CreateNewWrapper(goodsItem);
			AssertEquals($"When {fieldName} is only provided at header level and not at line, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setLineFieldTo(goodsItem, "X");
			wrapper = CreateNewWrapper(goodsItem);
			AssertEquals($"When line and header have same {fieldName}, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setLineFieldTo(goodsItem, "Y");
			wrapper = CreateNewWrapper(goodsItem);
			AssertEquals($"When line has {fieldName} different from header, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setHeaderTo(movementHeader, string.Empty);
			setLineFieldTo(goodsItem, "Y");
			wrapper = CreateNewWrapper(goodsItem);
			AssertEquals($"When line has {fieldName} and header is empty, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setLineFieldTo(goodsItem, string.Empty);
			wrapper = CreateNewWrapper(goodsItem);
			AssertEquals($"When both line and header don't have {fieldName}, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));
		});
	}

	NctsHeader nctsHeader;
	NctsBill bill;
	NctsDepartureCargoDesc goodsItem;
}
