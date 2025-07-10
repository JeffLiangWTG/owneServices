using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Moq;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class D1ConsignmentItemWrapperTest : D1ConsignmentItemWrapperBase
{
	public override void TestAdditionalInformation()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1ConsignmentItem.AdditionalInformation), wrapper.AdditionalInformation);
		AssertEquals($"{nameof(ID1ConsignmentItem.AdditionalInformation)}, count", 0, wrapper.AdditionalInformation.Count);

		var additionalInformationMock = new Mock<IAdditionalInformation>();
		consignmentItemCustomsMessageWrapperMock
			.Setup(h => h.AdditionalInformation)
			.Returns(new[] { additionalInformationMock.Object });
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1ConsignmentItem.AdditionalInformation), wrapper.AdditionalInformation);
		AssertEquals($"{nameof(ID1ConsignmentItem.AdditionalInformation)}, count", 1, wrapper.AdditionalInformation.Count);
	}

	public override void TestAdditionalReferences()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1ConsignmentItem.AdditionalReferences), wrapper.AdditionalReferences);
		AssertEquals($"{nameof(ID1ConsignmentItem.AdditionalReferences)}, count", 0, wrapper.AdditionalReferences.Count);

		var additionalReferenceMock = new Mock<IAdditionalReference>();
		consignmentItemCustomsMessageWrapperMock
			.Setup(h => h.AdditionalReferences)
			.Returns(new[] { additionalReferenceMock.Object });
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1ConsignmentItem.AdditionalReferences), wrapper.AdditionalReferences);
		AssertEquals($"{nameof(ID1ConsignmentItem.AdditionalReferences)}, count", 1, wrapper.AdditionalReferences.Count);
	}

	public override void TestAdditionalSupplyChainActors()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1ConsignmentItem.AdditionalSupplyChainActors), wrapper.AdditionalSupplyChainActors);
		AssertEquals($"{nameof(ID1ConsignmentItem.AdditionalSupplyChainActors)}, count", 0, wrapper.AdditionalSupplyChainActors.Count);

		var additionalSupplyChainActorMock = new Mock<IAdditionalSupplyChainActor>();
		consignmentItemCustomsMessageWrapperMock
			.Setup(h => h.AdditionalSupplyChainActors)
			.Returns(new[] { additionalSupplyChainActorMock.Object });
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1ConsignmentItem.AdditionalSupplyChainActors), wrapper.AdditionalSupplyChainActors);
		AssertEquals($"{nameof(ID1ConsignmentItem.AdditionalSupplyChainActors)}, count", 1, wrapper.AdditionalSupplyChainActors.Count);
	}

	public override void TestConsignee()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(ID1ConsignmentItem.Consignee), wrapper.Consignee);

		var consigneeMock = new Mock<ITrader>();
		consignmentItemCustomsMessageWrapperMock.Setup(h => h.Consignee)
			.Returns(consigneeMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1ConsignmentItem.Consignee), wrapper.Consignee);
	}

	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(
			"Exception expected when goodsItem is null",
			() => new D1ConsignmentItemWrapper(goodsItem: null, messageSendingWrapperFactory));

		AssertExceptionThrown<ArgumentNullException>(
			"Exception expected when messageSendingWrapperFactory is null",
			() => new D1ConsignmentItemWrapper(goodsItem, messageSendingWrapperFactory: null));
	}

	public override void TestCountryOfDestination()
	{
		using (SetTransitionPeriod(false))
		{
			var bill = goodsItem.Bill;
			var goodsItem2 = bill.GoodsItems.AddNew();

			var wrapper = CreateWrapper();
			AssertNullOrEmpty("Consignment, HouseConsignment and GoodsItem: all empty", wrapper.CountryOfDestination);

			movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Italy;
			goodsItem2.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Poland;
			wrapper = CreateWrapper();
			AssertEquals("Inherit from Consignment, Consignment: IT, HouseConsignment and GoodsItem: empty, PL", Core.Constants.CountryCodes.Italy, wrapper.CountryOfDestination);

			bill.B0_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Ireland;
			wrapper = CreateWrapper();
			AssertEquals("Inherit from HouseConsignment, Consignment: IT, HouseConsignment: IE, GoodsItem: empty, PL", Core.Constants.CountryCodes.Ireland, wrapper.CountryOfDestination);

			goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Poland;
			wrapper = CreateWrapper();
			AssertNullOrEmpty("Same at GoodsItem level, Consignment: IT, HouseConsignment: IE, GoodsItem: PL, PL", wrapper.CountryOfDestination);

			goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Spain;
			wrapper = CreateWrapper();
			AssertEquals("Different at GoodsItem level, Consignment: IT, HouseConsignment: IE, GoodsItem: ES, PL", Core.Constants.CountryCodes.Spain, wrapper.CountryOfDestination);
		}

		using (SetTransitionPeriod(true))
		{
			goodsItem = header.Bills.AddNew().GoodsItems.AddNew();

			var wrapper = CreateWrapper();
			AssertNullOrEmpty(nameof(ID1ConsignmentItem.CountryOfDestination), wrapper.CountryOfDestination);

			consignmentItemCustomsMessageWrapperMock
				.Setup(h => h.CountryOfDestination)
				.Returns("DE");
			wrapper = CreateWrapper();
			AssertEquals(nameof(ID1ConsignmentItem.CountryOfDestination), "DE", wrapper.CountryOfDestination);
		}
	}

	public override void TestCountryOfDispatch()
	{
		var bill = goodsItem.Bill;
		var goodsItem2 = bill.GoodsItems.AddNew();

		var wrapper = CreateWrapper();
		AssertNullOrEmpty("Consignment, HouseConsignment and GoodsItem: all empty", wrapper.CountryOfDispatch);

		movementHeader.BM_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Italy;
		goodsItem2.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Poland;
		wrapper = CreateWrapper();
		AssertEquals("Inherit from Consignment, Consignment: IT, HouseConsignment and GoodsItem: empty, PL", Core.Constants.CountryCodes.Italy, wrapper.CountryOfDispatch);

		bill.B0_RN_NKCountryOfExport = Core.Constants.CountryCodes.Ireland;
		wrapper = CreateWrapper();
		AssertEquals("Inherit from HouseConsignment, Consignment: IT, HouseConsignment: IE, GoodsItem: empty, PL", Core.Constants.CountryCodes.Ireland, wrapper.CountryOfDispatch);

		goodsItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Poland;
		wrapper = CreateWrapper();
		AssertNullOrEmpty("Same at GoodsItem level, Consignment: IT, HouseConsignment: IE, GoodsItem: PL, PL", wrapper.CountryOfDispatch);

		goodsItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Spain;
		wrapper = CreateWrapper();
		AssertEquals("Different at GoodsItem level, Consignment: IT, HouseConsignment: IE, GoodsItem: ES, PL", Core.Constants.CountryCodes.Spain, wrapper.CountryOfDispatch);
	}

	public override void TestCusCode()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(ID1ConsignmentItem.CusCode), wrapper.CusCode);

		consignmentItemCustomsMessageWrapperMock
			.Setup(h => h.CusCode)
			.Returns("CUSC");
		wrapper = CreateWrapper();
		AssertEquals(nameof(ID1ConsignmentItem.CusCode), "CUSC", wrapper.CusCode);
	}

	public override void TestDangerousGoodsCodes()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1ConsignmentItem.DangerousGoodsCodes), wrapper.DangerousGoodsCodes);
		AssertEquals($"{nameof(ID1ConsignmentItem.DangerousGoodsCodes)}, count", 0, wrapper.DangerousGoodsCodes.Count);

		consignmentItemCustomsMessageWrapperMock
			.Setup(h => h.DangerousGoodsCodes)
			.Returns(new[] { "DG1", "DG2" });
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1ConsignmentItem.DangerousGoodsCodes), wrapper.DangerousGoodsCodes);
		AssertEquals($"{nameof(ID1ConsignmentItem.DangerousGoodsCodes)}, count", 2, wrapper.DangerousGoodsCodes.Count);
	}

	public override void TestDeclarationGoodsItemNumber()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(ID1ConsignmentItem.DeclarationGoodsItemNumber), 0, wrapper.DeclarationGoodsItemNumber);

		consignmentItemCustomsMessageWrapperMock
			.Setup(h => h.DeclarationGoodsItemNumber)
			.Returns(12);
		wrapper = CreateWrapper();
		AssertEquals(nameof(ID1ConsignmentItem.DeclarationGoodsItemNumber), 12, wrapper.DeclarationGoodsItemNumber);
	}

	public override void TestDeclarationType()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(ID1ConsignmentItem.DeclarationType), wrapper.DeclarationType);

		consignmentItemCustomsMessageWrapperMock
			.Setup(h => h.DeclarationType)
			.Returns("T");
		wrapper = CreateWrapper();
		AssertEquals(nameof(ID1ConsignmentItem.DeclarationType), "T", wrapper.DeclarationType);
	}

	public override void TestDescriptionOfGoods()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(ID1ConsignmentItem.DescriptionOfGoods), wrapper.DescriptionOfGoods);

		consignmentItemCustomsMessageWrapperMock
			.Setup(h => h.DescriptionOfGoods)
			.Returns("DESC");
		wrapper = CreateWrapper();
		AssertEquals(nameof(ID1ConsignmentItem.DescriptionOfGoods), "DESC", wrapper.DescriptionOfGoods);
	}

	public override void TestGoodsItemNumber()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(ID1ConsignmentItem.GoodsItemNumber), 0, wrapper.GoodsItemNumber);

		consignmentItemCustomsMessageWrapperMock
			.Setup(h => h.GoodsItemNumber)
			.Returns(99);
		wrapper = CreateWrapper();
		AssertEquals(nameof(ID1ConsignmentItem.GoodsItemNumber), 99, wrapper.GoodsItemNumber);
	}

	public override void TestGrossMass()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(ID1ConsignmentItem.GrossMass), wrapper.GrossMass);

		consignmentItemCustomsMessageWrapperMock
			.Setup(h => h.GrossMass)
			.Returns(18.33m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(ID1ConsignmentItem.GrossMass), 18.33m, wrapper.GrossMass);
	}

	public override void TestHsTariffCode()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(ID1ConsignmentItem.HsTariffCode), wrapper.HsTariffCode);

		consignmentItemCustomsMessageWrapperMock
			.Setup(h => h.HsTariffCode)
			.Returns("HSTF");
		wrapper = CreateWrapper();
		AssertEquals(nameof(ID1ConsignmentItem.HsTariffCode), "HSTF", wrapper.HsTariffCode);
	}

	public override void TestNcTariffCode()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(ID1ConsignmentItem.NcTariffCode), wrapper.NcTariffCode);

		consignmentItemCustomsMessageWrapperMock
			.Setup(h => h.NcTariffCode)
			.Returns("NCTF");
		wrapper = CreateWrapper();
		AssertEquals(nameof(ID1ConsignmentItem.NcTariffCode), "NCTF", wrapper.NcTariffCode);
	}

	public override void TestNetMass()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(ID1ConsignmentItem.NetMass), wrapper.NetMass);

		consignmentItemCustomsMessageWrapperMock
			.Setup(h => h.NetMass)
			.Returns(1.23m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(ID1ConsignmentItem.NetMass), 1.23m, wrapper.NetMass);
	}

	public override void TestPackages()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1ConsignmentItem.Packages), wrapper.Packages);
		AssertEquals($"{nameof(ID1ConsignmentItem.Packages)}, count", 0, wrapper.Packages.Count);

		var packageMock = new Mock<IPackage>();
		consignmentItemCustomsMessageWrapperMock
			.Setup(h => h.Packages)
			.Returns(new[] { packageMock.Object });
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1ConsignmentItem.Packages), wrapper.Packages);
		AssertEquals($"{nameof(ID1ConsignmentItem.Packages)}, count", 1, wrapper.Packages.Count);
	}

	public override void TestPreviousDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1ConsignmentItem.PreviousDocuments), wrapper.PreviousDocuments);
		AssertEquals($"{nameof(ID1ConsignmentItem.PreviousDocuments)}, count", 0, wrapper.PreviousDocuments.Count);

		var previousDocumentMock = new Mock<IConsignmentItemPreviousDocument>();
		consignmentItemCustomsMessageWrapperMock
			.Setup(h => h.PreviousDocuments)
			.Returns(new[] { previousDocumentMock.Object });
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1ConsignmentItem.PreviousDocuments), wrapper.PreviousDocuments);
		AssertEquals($"{nameof(ID1ConsignmentItem.PreviousDocuments)}, count", 1, wrapper.PreviousDocuments.Count);
	}

	public override void TestSupplementaryUnits()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(ID1ConsignmentItem.SupplementaryUnits), wrapper.SupplementaryUnits);

		consignmentItemCustomsMessageWrapperMock
			.Setup(h => h.SupplementaryUnits)
			.Returns(2m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(ID1ConsignmentItem.SupplementaryUnits), 2m, wrapper.SupplementaryUnits);
	}

	public override void TestSupportingDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1ConsignmentItem.SupportingDocuments), wrapper.SupportingDocuments);
		AssertEquals($"{nameof(ID1ConsignmentItem.SupportingDocuments)}, count", 0, wrapper.SupportingDocuments.Count);

		var supportingDocumentMock = new Mock<ISupportingDocument>();
		consignmentItemCustomsMessageWrapperMock
			.Setup(h => h.SupportingDocuments)
			.Returns(new[] { supportingDocumentMock.Object });
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1ConsignmentItem.SupportingDocuments), wrapper.SupportingDocuments);
		AssertEquals($"{nameof(ID1ConsignmentItem.SupportingDocuments)}, count", 1, wrapper.SupportingDocuments.Count);
	}

	public override void TestTransportChargesMethodOfPayment()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(ID1ConsignmentItem.TransportChargesMethodOfPayment), wrapper.TransportChargesMethodOfPayment);

		consignmentItemCustomsMessageWrapperMock
			.Setup(h => h.TransportChargesMethodOfPayment)
			.Returns("TRMOP");
		wrapper = CreateWrapper();
		AssertEquals(nameof(ID1ConsignmentItem.TransportChargesMethodOfPayment), "TRMOP", wrapper.TransportChargesMethodOfPayment);
	}

	public override void TestTransportDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1ConsignmentItem.TransportDocuments), wrapper.TransportDocuments);
		AssertEquals($"{nameof(ID1ConsignmentItem.TransportDocuments)}, count", 0, wrapper.TransportDocuments.Count);

		var transportDocumentMock = new Mock<ITransportDocument>();
		consignmentItemCustomsMessageWrapperMock
			.Setup(h => h.TransportDocuments)
			.Returns(new[] { transportDocumentMock.Object });
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1ConsignmentItem.TransportDocuments), wrapper.TransportDocuments);
		AssertEquals($"{nameof(ID1ConsignmentItem.TransportDocuments)}, count", 1, wrapper.TransportDocuments.Count);
	}

	public override void TestUcr()
	{
		var header = Factory.NewDepartureNctsHeaderPhase5();

		var b1 = header.Bills.AddNew();
		var b1gi1 = b1.GoodsItems.AddNew();
		var b1gi2 = b1.GoodsItems.AddNew();

		var b2 = header.Bills.AddNew();
		var b2gi1 = b2.GoodsItems.AddNew();
		var b2gi2 = b2.GoodsItems.AddNew();

		header.MovementHeader.BM_UniqueConsignmentReference = "FR";

		b1.B0_ReferenceID = "NO";
		b1gi1.BY_CommercialReferenceNumber = "NO";
		b1gi2.BY_CommercialReferenceNumber = string.Empty;

		b2.B0_ReferenceID = string.Empty;
		b2gi1.BY_CommercialReferenceNumber = "NO";
		b2gi2.BY_CommercialReferenceNumber = string.Empty;

		var ucr = (NctsDepartureCargoDesc item) =>
		{
			consignmentItemCustomsMessageWrapperMock
				.Setup(h => h.Ucr)
				.Returns(item.BY_CommercialReferenceNumber);
			var wrapper = CreateWrapper(item);
			return wrapper.Ucr;
		};

		using (SetTransitionPeriod(false))
		{
			CombineAssertions("Transition Period False", () =>
			{
				AssertEquals(string.Empty, ucr(b1gi1));
				AssertEquals(string.Empty, ucr(b1gi2));
				AssertEquals("NO", ucr(b2gi1));
				AssertEquals("FR", ucr(b2gi2));
			});
		}

		using (SetTransitionPeriod(true))
		{
			CombineAssertions("Transition Period True", () =>
			{
				AssertEquals("NO", ucr(b1gi1));
				AssertEquals(string.Empty, ucr(b1gi2));
				AssertEquals("NO", ucr(b2gi1));
				AssertEquals(string.Empty, ucr(b2gi2));
			});
		}
	}

	protected override ID1ConsignmentItem CreateWrapper() => CreateWrapper(goodsItem);

	ID1ConsignmentItem CreateWrapper(NctsDepartureCargoDesc item) => new D1ConsignmentItemWrapper(item, messageSendingWrapperFactory);

	IDisposable SetTransitionPeriod(bool isActive) =>
		ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isActive);

	protected override void SetUp()
	{
		base.SetUp();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		goodsItem = header.Bills.AddNew().GoodsItems.AddNew();

		consignmentItemCustomsMessageWrapperMock = new Mock<IConsignmentItemCustomsMessageWrapper>();
		messageSendingWrapperFactory = new MessageSendingWrapperFactoryMockBuilder()
			.ConfigureGetNewConsignmentItemCustomsMessageWrapper(consignmentItemCustomsMessageWrapperMock.Object)
			.Build();
	}

	NctsDepartureCargoDesc goodsItem;
	Mock<IConsignmentItemCustomsMessageWrapper> consignmentItemCustomsMessageWrapperMock;
	IMessageSendingWrapperFactory messageSendingWrapperFactory;
}
