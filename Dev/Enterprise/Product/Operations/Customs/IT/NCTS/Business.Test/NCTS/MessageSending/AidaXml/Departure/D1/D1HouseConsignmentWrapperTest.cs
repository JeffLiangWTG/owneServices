using System;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Moq;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class D1HouseConsignmentWrapperTest : D1HouseConsignmentWrapperBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when bill is null", () => new D1HouseConsignmentWrapper(null, messageSendingWrapperFactory));
		AssertExceptionThrown<ArgumentNullException>("Exception expected when factory is null", () => new D1HouseConsignmentWrapper(bill, null));
		AssertNoExceptionThrown("When arguments are not null", () => new D1HouseConsignmentWrapper(bill, messageSendingWrapperFactory));
	}

	public override void TestSequenceNumber()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(ID1HouseConsignment.SequenceNumber), 0, wrapper.SequenceNumber);

		houseConsignmentCustomsMessageWrapperMock.Setup(h => h.SequenceNumber).Returns(3);
		wrapper = CreateWrapper();
		AssertEquals(nameof(ID1HouseConsignment.SequenceNumber), 3, wrapper.SequenceNumber);
	}

	public override void TestPreviousDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1HouseConsignment.PreviousDocuments), wrapper.PreviousDocuments);
		AssertEquals($"{nameof(ID1HouseConsignment.PreviousDocuments)}, count", 0, wrapper.PreviousDocuments.Count);

		var previousDocumentMock = new Mock<IPreviousDocument>();
		houseConsignmentCustomsMessageWrapperMock.Setup(h => h.PreviousDocuments)
			.Returns(new[] { previousDocumentMock.Object });
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1HouseConsignment.PreviousDocuments), wrapper.PreviousDocuments);
		AssertEquals($"{nameof(ID1HouseConsignment.PreviousDocuments)}, count", 1, wrapper.PreviousDocuments.Count);
	}

	public override void TestSupportingDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1HouseConsignment.SupportingDocuments), wrapper.SupportingDocuments);
		AssertEquals($"{nameof(ID1HouseConsignment.SupportingDocuments)}, count", 0, wrapper.SupportingDocuments.Count);

		var supportingDocumentMock = new Mock<ISupportingDocument>();
		houseConsignmentCustomsMessageWrapperMock.Setup(h => h.SupportingDocuments)
			.Returns(new[] { supportingDocumentMock.Object });
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1HouseConsignment.SupportingDocuments), wrapper.SupportingDocuments);
		AssertEquals($"{nameof(ID1HouseConsignment.SupportingDocuments)}, count", 1, wrapper.SupportingDocuments.Count);
	}

	public override void TestAdditionalReferences()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1HouseConsignment.AdditionalReferences), wrapper.AdditionalReferences);
		AssertEquals($"{nameof(ID1HouseConsignment.AdditionalReferences)}, count", 0, wrapper.AdditionalReferences.Count);

		var additionalReferenceMock = new Mock<IAdditionalReference>();
		houseConsignmentCustomsMessageWrapperMock.Setup(h => h.AdditionalReferences)
			.Returns(new[] { additionalReferenceMock.Object });
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1HouseConsignment.AdditionalReferences), wrapper.AdditionalReferences);
		AssertEquals($"{nameof(ID1HouseConsignment.AdditionalReferences)}, count", 1, wrapper.AdditionalReferences.Count);
	}

	public override void TestTransportDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1HouseConsignment.TransportDocuments), wrapper.TransportDocuments);
		AssertEquals($"{nameof(ID1HouseConsignment.TransportDocuments)}, count", 0, wrapper.TransportDocuments.Count);

		var transportDocumentMock = new Mock<ITransportDocument>();
		houseConsignmentCustomsMessageWrapperMock.Setup(h => h.TransportDocuments)
			.Returns(new[] { transportDocumentMock.Object });
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1HouseConsignment.TransportDocuments), wrapper.TransportDocuments);
		AssertEquals($"{nameof(ID1HouseConsignment.TransportDocuments)}, count", 1, wrapper.TransportDocuments.Count);
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

		header.MovementHeader.BM_UniqueConsignmentReference = "PL";

		b1.B0_ReferenceID = "ES";
		b1gi1.BY_CommercialReferenceNumber = "PL";
		b1gi2.BY_CommercialReferenceNumber = "ES";

		b2.B0_ReferenceID = "PL";
		b2gi1.BY_CommercialReferenceNumber = "IT";
		b2gi2.BY_CommercialReferenceNumber = "IT";

		var ucr = (NctsBill b) =>
		{
			houseConsignmentCustomsMessageWrapperMock
				.Setup(h => h.Ucr)
				.Returns(b.B0_ReferenceID);
			var wrapper = CreateWrapper(b);
			return wrapper.Ucr;
		};

		using (SetTransitionPeriod(false))
		{
			CombineAssertions("Transition Period False", () =>
			{
				AssertEquals(string.Empty, ucr(b1));
				AssertEquals("IT", ucr(b2));
			});
		}

		using (SetTransitionPeriod(true))
		{
			CombineAssertions("Transition Period True", () =>
			{
				AssertEquals("ES", ucr(b1));
				AssertEquals("PL", ucr(b2));
			});
		}
	}

	public override void TestAdditionalInformation()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1HouseConsignment.AdditionalInformation), wrapper.AdditionalInformation);
		AssertEquals($"{nameof(ID1HouseConsignment.AdditionalInformation)}, count", 0, wrapper.AdditionalInformation.Count);

		var additionalInformationMock = new Mock<IAdditionalInformation>();
		houseConsignmentCustomsMessageWrapperMock.Setup(h => h.AdditionalInformation)
			.Returns(new[] { additionalInformationMock.Object });
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1HouseConsignment.AdditionalInformation), wrapper.AdditionalInformation);
		AssertEquals($"{nameof(ID1HouseConsignment.AdditionalInformation)}, count", 1, wrapper.AdditionalInformation.Count);
	}

	public override void TestConsignor()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(ID1HouseConsignment.Consignor), wrapper.Consignor);

		var consignorMock = new Mock<ITrader>();
		houseConsignmentCustomsMessageWrapperMock.Setup(h => h.Consignor)
			.Returns(consignorMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1HouseConsignment.Consignor), wrapper.Consignor);
	}

	public override void TestConsignee()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(ID1HouseConsignment.Consignee), wrapper.Consignee);

		var consigneeMock = new Mock<ITrader>();
		houseConsignmentCustomsMessageWrapperMock.Setup(h => h.Consignee)
			.Returns(consigneeMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1HouseConsignment.Consignee), wrapper.Consignee);
	}

	public override void TestAdditionalSupplyChainActors()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1HouseConsignment.AdditionalSupplyChainActors), wrapper.AdditionalSupplyChainActors);
		AssertEquals($"{nameof(ID1HouseConsignment.AdditionalSupplyChainActors)}, count", 0, wrapper.AdditionalSupplyChainActors.Count);

		var additionalSupplyChainActorMock = new Mock<IAdditionalSupplyChainActor>();
		houseConsignmentCustomsMessageWrapperMock.Setup(h => h.AdditionalSupplyChainActors)
			.Returns(new[] { additionalSupplyChainActorMock.Object });
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1HouseConsignment.AdditionalSupplyChainActors), wrapper.AdditionalSupplyChainActors);
		AssertEquals($"{nameof(ID1HouseConsignment.AdditionalSupplyChainActors)}, count", 1, wrapper.AdditionalSupplyChainActors.Count);
	}

	public override void TestTransportChargesMethodOfPayment()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(ID1HouseConsignment.TransportChargesMethodOfPayment), wrapper.TransportChargesMethodOfPayment);

		houseConsignmentCustomsMessageWrapperMock.Setup(h => h.TransportChargesMethodOfPayment)
			.Returns("TransportChargesMethodOfPayment");
		wrapper = CreateWrapper();
		AssertEquals(nameof(ID1HouseConsignment.TransportChargesMethodOfPayment), "TransportChargesMethodOfPayment", wrapper.TransportChargesMethodOfPayment);
	}

	public override void TestCountryOfDispatch()
	{
		var goodItem1 = bill.GoodsItems.AddNew();
		var goodItem2 = bill.GoodsItems.AddNew();
		var bill2 = header.Bills.AddNew();
		var goodItem3 = bill2.GoodsItems.AddNew();

		var wrapper = CreateWrapper();
		AssertNullOrEmpty("Consignment, HouseConsignment and GoodsItem: all empty", wrapper.CountryOfDispatch);

		movementHeader.BM_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Italy;
		bill2.B0_RN_NKCountryOfExport = Core.Constants.CountryCodes.Ireland;
		wrapper = CreateWrapper();
		AssertEquals("Inherit from Consignment, Consignment: IT, HouseConsignment: empty, IE,GoodsItem: all empty", Core.Constants.CountryCodes.Italy, wrapper.CountryOfDispatch);

		bill.B0_RN_NKCountryOfExport = Core.Constants.CountryCodes.Spain;
		wrapper = CreateWrapper();
		AssertEquals("Different at HouseConsignment level, Consignment: IT, HouseConsignment: ES, IE, GoodsItem: all empty", Core.Constants.CountryCodes.Spain, wrapper.CountryOfDispatch);

		bill2.B0_RN_NKCountryOfExport = Core.Constants.CountryCodes.Spain;
		wrapper = CreateWrapper();
		AssertNullOrEmpty("Same at HouseConsignment level, Consignment: IT, HouseConsignment: ES, ES, GoodsItem: all empty", wrapper.CountryOfDispatch);

		goodItem1.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Spain;
		goodItem2.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Poland;
		goodItem3.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Ireland;
		wrapper = CreateWrapper();
		AssertNullOrEmpty("Different at GoodsItem level, Consignment: IT, HouseConsignment: ES, ES, GoodsItem: ES, PL, IE", wrapper.CountryOfDispatch);

		goodItem1.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Poland;
		wrapper = CreateWrapper();
		AssertEquals("Same at GoodsItem level, Consignment: IT, HouseConsignment: ES, ES, GoodsItem: PL, PL, IE", Core.Constants.CountryCodes.Poland, wrapper.CountryOfDispatch);
	}

	public override void TestCountryOfDestination()
	{
		using (SetTransitionPeriod(false))
		{
			var goodItem1 = bill.GoodsItems.AddNew();
			var goodItem2 = bill.GoodsItems.AddNew();
			var bill2 = header.Bills.AddNew();
			var goodItem3 = bill2.GoodsItems.AddNew();

			var wrapper = CreateWrapper();
			AssertNullOrEmpty("Consignment, HouseConsignment and GoodsItem: all empty", wrapper.CountryOfDestination);

			movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Italy;
			bill2.B0_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Ireland;
			wrapper = CreateWrapper();
			AssertEquals("Inherit from Consignment, Consignment: IT, HouseConsignment: empty, IE,GoodsItem: all empty", Core.Constants.CountryCodes.Italy, wrapper.CountryOfDestination);

			bill.B0_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Spain;
			wrapper = CreateWrapper();
			AssertEquals("Different at HouseConsignment level, Consignment: IT, HouseConsignment: ES, IE, GoodsItem: all empty", Core.Constants.CountryCodes.Spain, wrapper.CountryOfDestination);

			bill2.B0_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Spain;
			wrapper = CreateWrapper();
			AssertNullOrEmpty("Same at HouseConsignment level, Consignment: IT, HouseConsignment: ES, ES, GoodsItem: all empty", wrapper.CountryOfDestination);

			goodItem1.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Spain;
			goodItem2.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Poland;
			goodItem3.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Ireland;
			wrapper = CreateWrapper();
			AssertNullOrEmpty("Different at GoodsItem level, Consignment: IT, HouseConsignment: ES, ES, GoodsItem: ES, PL, IE", wrapper.CountryOfDestination);

			goodItem1.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Poland;
			wrapper = CreateWrapper();
			AssertEquals("Same at GoodsItem level, Consignment: IT, HouseConsignment: ES, ES, GoodsItem: PL, PL, IE", Core.Constants.CountryCodes.Poland, wrapper.CountryOfDestination);
		}
	}

	public override void TestGrossMass()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(ID1HouseConsignment.GrossMass), 0m, wrapper.GrossMass);

		houseConsignmentCustomsMessageWrapperMock.Setup(h => h.GrossMass)
			.Returns(1.23m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(ID1HouseConsignment.GrossMass), 1.23m, wrapper.GrossMass);
	}

	public override void TestDepartureMeansOfTransports()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1HouseConsignment.DepartureMeansOfTransports), wrapper.DepartureMeansOfTransports);
		AssertEquals($"{nameof(ID1HouseConsignment.DepartureMeansOfTransports)}, count", 0, wrapper.DepartureMeansOfTransports.Count);

		var meansOfTransportMock = new Mock<IMeansOfTransport>();
		houseConsignmentCustomsMessageWrapperMock.Setup(h => h.DepartureMeansOfTransports)
			.Returns(new[] { meansOfTransportMock.Object });
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1HouseConsignment.DepartureMeansOfTransports), wrapper.DepartureMeansOfTransports);
		AssertEquals($"{nameof(ID1HouseConsignment.DepartureMeansOfTransports)}, count", 1, wrapper.DepartureMeansOfTransports.Count);
	}

	public override void TestConsignmentItems()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1HouseConsignment.ConsignmentItems), wrapper.ConsignmentItems);
		AssertEquals($"{nameof(ID1HouseConsignment.ConsignmentItems)} Count", 0, wrapper.ConsignmentItems.Count);

		var goodsItem = bill.GoodsItems.AddNew();
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1HouseConsignment.ConsignmentItems), wrapper.ConsignmentItems);
		AssertEquals($"{nameof(ID1HouseConsignment.ConsignmentItems)} Count", 1, wrapper.ConsignmentItems.Count);
		AssertType<D1ConsignmentItemWrapper>($"{nameof(ID1HouseConsignment.ConsignmentItems)} Type", wrapper.ConsignmentItems.Single());

		goodsItem.BY_Status = "DLR";
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1HouseConsignment.ConsignmentItems), wrapper.ConsignmentItems);
		AssertEquals($"{nameof(ID1HouseConsignment.ConsignmentItems)} Count", 0, wrapper.ConsignmentItems.Count);

		goodsItem.BY_Status = "DEL";
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1HouseConsignment.ConsignmentItems), wrapper.ConsignmentItems);
		AssertEquals($"{nameof(ID1HouseConsignment.ConsignmentItems)} Count", 0, wrapper.ConsignmentItems.Count);
	}

	IDisposable SetTransitionPeriod(bool isActive) =>
		ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isActive);

	protected override void SetUp()
	{
		base.SetUp();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		bill = header.Bills.AddNew();

		houseConsignmentCustomsMessageWrapperMock = new Mock<IHouseConsignmentCustomsMessageWrapper>();
		messageSendingWrapperFactory = new MessageSendingWrapperFactoryMockBuilder()
			.ConfigureGetNewHouseConsignmentCustomsMessageWrapper(houseConsignmentCustomsMessageWrapperMock.Object)
			.Build();
	}

	protected override ID1HouseConsignment CreateWrapper() => CreateWrapper(bill);

	ID1HouseConsignment CreateWrapper(NctsBill b) => new D1HouseConsignmentWrapper(b, messageSendingWrapperFactory);

	NctsBill bill;
	Mock<IHouseConsignmentCustomsMessageWrapper> houseConsignmentCustomsMessageWrapperMock;
	IMessageSendingWrapperFactory messageSendingWrapperFactory;
}
