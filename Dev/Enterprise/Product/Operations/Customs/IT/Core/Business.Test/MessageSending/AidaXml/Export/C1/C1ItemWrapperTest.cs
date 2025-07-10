using System;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Moq;
using MessageBuilder = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class C1ItemWrapperTest : C1ItemWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Expected exception when entryLine is null", () => new C1ItemWrapper(null, exportMessageSendingWrapperFactory));
		AssertExceptionThrown<ArgumentNullException>("Expected exception when messageSendingWrapperFactory argument is null", () => new C1ItemWrapper(EntryLine, null));
	}

	public override void TestAdditionalCodes()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.AdditionalCodes), 0, wrapper.AdditionalCodes.Count);

		entryLineWrapperMock.Setup(e => e.AdditionalCodes).Returns(new[] { "AC1", "AC2" });
		wrapper = CreateWrapper();
		AssertArrayEqualsByElements(nameof(IC1Item.AdditionalCodes), new[] { "AC1", "AC2" }, wrapper.AdditionalCodes.ToArray());
	}

	public override void TestAdditionalInformation()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.AdditionalInformation), 0, wrapper.AdditionalInformation.Count);

		var additionalInformation = new Mock<IAdditionalInformation>();
		entryLineWrapperMock.Setup(e => e.ExportAdditionalInformation).Returns(new[] { additionalInformation.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.AdditionalInformation), 1, wrapper.AdditionalInformation.Count);
	}

	public override void TestAdditionalReferences()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.AdditionalReferences), 0, wrapper.AdditionalReferences.Count);

		var additionalInformation = new Mock<IAdditionalReference>();
		entryLineWrapperMock.Setup(e => e.ExportAdditionalReferences).Returns(new[] { additionalInformation.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.AdditionalReferences), 1, wrapper.AdditionalReferences.Count);
	}

	public override void TestAdditionalSupplyChainActors()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.AdditionalSupplyChainActors), 0, wrapper.AdditionalSupplyChainActors.Count);

		var additionalSupplyChainActor = new Mock<IAdditionalSupplyChainActor>();
		entryLineWrapperMock.Setup(e => e.AdditionalSupplyChainActors).Returns(new[] { additionalSupplyChainActor.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.AdditionalSupplyChainActors), 1, wrapper.AdditionalSupplyChainActors.Count);
	}

	public override void TestAuthorizations()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.Authorizations), 0, wrapper.Authorizations.Count);

		var authorization = new Mock<IAuthorization>();
		entryLineWrapperMock.Setup(e => e.ExportAuthorizations).Returns(new[] { authorization.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.Authorizations), 1, wrapper.Authorizations.Count);
	}

	public override void TestConsignee()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IC1Item.Consignee), wrapper.Consignee);

		var traderMock = new Mock<IEoriTrader>();
		entryLineWrapperMock.Setup(i => i.ExportConsignee).Returns(traderMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IC1Item.Consignee), wrapper.Consignee);
	}

	public override void TestConsignor()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IC1Item.Consignor), wrapper.Consignor);

		var traderMock = new Mock<IEoriTrader>();
		entryLineWrapperMock.Setup(i => i.ExportConsignor).Returns(traderMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IC1Item.Consignor), wrapper.Consignor);
	}

	public override void TestCountryOfDestination()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Item.CountryOfDestination), itemWrapper.CountryOfDestination);

		entryLineWrapperMock.Setup(e => e.CountryOfDestination).Returns("AU");

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.CountryOfDestination), "AU", itemWrapper.CountryOfDestination);
	}

	public override void TestCountryOfExport()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Item.CountryOfExport), itemWrapper.CountryOfExport);

		entryLineWrapperMock.Setup(e => e.CountryOfExport).Returns("ES");

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.CountryOfExport), "ES", itemWrapper.CountryOfExport);
	}

	public override void TestCountryOfOrigin()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Item.CountryOfOrigin), itemWrapper.CountryOfOrigin);

		entryLineWrapperMock.Setup(e => e.ExportCountryOfOrigin).Returns("GB");

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.CountryOfOrigin), "GB", itemWrapper.CountryOfOrigin);
	}

	public override void TestCusCode()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Item.CusCode), itemWrapper.CusCode);

		entryLineWrapperMock.Setup(e => e.CusCode).Returns("00100016");

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.CusCode), "00100016", itemWrapper.CusCode);
	}

	public override void TestDangerousGoodsCodes()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.DangerousGoodsCodes), 0, wrapper.DangerousGoodsCodes.Count);

		entryLineWrapperMock.Setup(e => e.DangerousGoodsCodes).Returns(new[] { "C1", "C2" });
		wrapper = CreateWrapper();
		AssertArrayEqualsByElements(nameof(IC1Item.DangerousGoodsCodes), new[] { "C1", "C2" }, wrapper.DangerousGoodsCodes.ToArray());
	}

	public override void TestGoodsDescription()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Item.GoodsDescription), wrapper.GoodsDescription);

		entryLineWrapperMock.Setup(e => e.GoodsDescription).Returns("GOODS_DESCRIPTION");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.GoodsDescription), "GOODS_DESCRIPTION", wrapper.GoodsDescription);
	}

	public override void TestGrossMass()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.GrossMass), 0m, wrapper.GrossMass);

		entryLineWrapperMock.Setup(e => e.GrossMass).Returns(78.9821m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.GrossMass), 78.9821m, wrapper.GrossMass);
	}

	public override void TestHsTariffCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Item.HsTariffCode), wrapper.HsTariffCode);

		entryLineWrapperMock.Setup(e => e.ExportHsTariffCode).Returns("HS");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.HsTariffCode), "HS", wrapper.HsTariffCode);
	}

	public override void TestItemNumber()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.ItemNumber), 0, wrapper.ItemNumber);

		entryLineWrapperMock.Setup(e => e.ItemNumber).Returns(145);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.ItemNumber), 145, wrapper.ItemNumber);
	}

	public override void TestNationalAdditionalCodes()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.NationalAdditionalCodes), 0, wrapper.NationalAdditionalCodes.Count);

		entryLineWrapperMock.Setup(e => e.NationalAdditionalCodes).Returns(new[] { "NAC1", "NAC2" });
		wrapper = CreateWrapper();
		AssertArrayEqualsByElements(nameof(IC1Item.NationalAdditionalCodes), new[] { "NAC1", "NAC2" }, wrapper.NationalAdditionalCodes.ToArray());
	}

	public override void TestNcTariffCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Item.NcTariffCode), wrapper.NcTariffCode);

		entryLineWrapperMock.Setup(e => e.ExportNcTariffCode).Returns("NC");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.NcTariffCode), "NC", wrapper.NcTariffCode);
	}

	public override void TestNetMass()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.NetMass), 0m, wrapper.NetMass);

		entryLineWrapperMock.Setup(e => e.NetMass).Returns(78.9821m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.NetMass), 78.9821m, wrapper.NetMass);
	}

	public override void TestPackages()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.Packages), 0, wrapper.Packages.Count);

		var packageMock = new Mock<IPackage>();
		entryLineWrapperMock.Setup(x => x.Packages).Returns(new IPackage[1] { packageMock.Object });
		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.Packages), 1, wrapper.Packages.Count);
	}

	public override void TestPreviousDocuments()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.PreviousDocuments), 0, wrapper.PreviousDocuments.Count);

		var previousDocument = new Mock<IPreviousDocument>();
		entryLineWrapperMock.Setup(e => e.ExportPreviousDocuments).Returns(new[] { previousDocument.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.PreviousDocuments), 1, wrapper.PreviousDocuments.Count);
	}

	public override void TestProcedure()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IC1Item.Procedure), wrapper.Procedure);

		var cusProcedureMock = new Mock<ICustomsProcedure>();
		entryLineWrapperMock.Setup(i => i.Procedure).Returns(cusProcedureMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IC1Item.Procedure), wrapper.Procedure);
	}

	public override void TestSupportingDocuments()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.SupportingDocuments), 0, wrapper.SupportingDocuments.Count);

		var supportingDocument = new Mock<MessageBuilder.ISupportingDocument>();
		entryLineWrapperMock.Setup(e => e.SupportingDocuments).Returns(new[] { supportingDocument.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.SupportingDocuments), 1, wrapper.SupportingDocuments.Count);
	}

	public override void TestTransportChargesMethodOfPayment()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Item.TransportChargesMethodOfPayment), wrapper.TransportChargesMethodOfPayment);

		entryLineWrapperMock.Setup(e => e.ExportTransportChargesMethodOfPayment).Returns("A");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.TransportChargesMethodOfPayment), "A", wrapper.TransportChargesMethodOfPayment);
	}

	public override void TestTransportDocuments()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.TransportDocuments), 0, wrapper.TransportDocuments.Count);

		var additionalInformation = new Mock<ITransportDocument>();
		entryLineWrapperMock.Setup(e => e.ExportTransportDocuments).Returns(new[] { additionalInformation.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Item.TransportDocuments), 1, wrapper.TransportDocuments.Count);
	}

	protected override IC1Item CreateWrapper() => new C1ItemWrapper(EntryLine, exportMessageSendingWrapperFactory);

	protected override void SetUp()
	{
		base.SetUp();
		entryLineWrapperMock = new Mock<ICusEntryLineCustomsMessageWrapper>();

		exportMessageSendingWrapperFactory = new ExportMessageSendingWrapperFactoryMockBuilder()
			.ConfigureGetNewCusEntryLineCustomsMessageWrapper(entryLineWrapperMock.Object)
			.Build();
	}

	CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory;
	Mock<ICusEntryLineCustomsMessageWrapper> entryLineWrapperMock;
}
