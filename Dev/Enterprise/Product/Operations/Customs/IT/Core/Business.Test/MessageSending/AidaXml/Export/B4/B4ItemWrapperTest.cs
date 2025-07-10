using System;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Moq;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class B4ItemWrapperTest : B4ItemWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Expected exception when entryLine is null", () => new B4ItemWrapper(null, exportMessageSendingWrapperFactory));
		AssertExceptionThrown<ArgumentNullException>("Expected exception when messageSendingWrapperFactory argument is null", () => new B4ItemWrapper(EntryLine, exportMessageSendingWrapperFactory: null));
	}

	public override void TestItemNumber()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.ItemNumber), 0, wrapper.ItemNumber);

		entryLineWrapperMock.Setup(e => e.ItemNumber).Returns(145);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.ItemNumber), 145, wrapper.ItemNumber);
	}

	public override void TestProcedure()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IB4Item.Procedure), wrapper.Procedure);

		var cusProcedureMock = new Mock<ICustomsProcedure>();
		entryLineWrapperMock.Setup(i => i.Procedure).Returns(cusProcedureMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IB4Item.Procedure), wrapper.Procedure);
	}

	public override void TestPreviousDocuments()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.PreviousDocuments), 0, wrapper.PreviousDocuments.Count);

		var previousDocument = new Mock<IPreviousDocument>();
		entryLineWrapperMock.Setup(e => e.ExportPreviousDocuments).Returns(new[] { previousDocument.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.PreviousDocuments), 1, wrapper.PreviousDocuments.Count);
	}

	public override void TestAdditionalInformation()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.AdditionalInformation), 0, wrapper.AdditionalInformation.Count);

		var additionalInformation = new Mock<IAdditionalInformation>();
		entryLineWrapperMock.Setup(e => e.ExportAdditionalInformation).Returns(new[] { additionalInformation.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.AdditionalInformation), 1, wrapper.AdditionalInformation.Count);
	}

	public override void TestSupportingDocuments()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.SupportingDocuments), 0, wrapper.SupportingDocuments.Count);

		var supportingDocument = new Mock<CargoWise.Customs.IT.MessageContracts.Declaration.ISupportingDocument>();
		entryLineWrapperMock.Setup(e => e.SupportingDocuments).Returns(new[] { supportingDocument.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.SupportingDocuments), 1, wrapper.SupportingDocuments.Count);
	}

	public override void TestAuthorizations()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.Authorizations), 0, wrapper.Authorizations.Count);

		var authorization = new Mock<IAuthorization>();
		entryLineWrapperMock.Setup(e => e.ExportAuthorizations).Returns(new[] { authorization.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.Authorizations), 1, wrapper.Authorizations.Count);
	}

	public override void TestTransportDocuments()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.TransportDocuments), 0, wrapper.TransportDocuments.Count);

		var additionalInformation = new Mock<ITransportDocument>();
		entryLineWrapperMock.Setup(e => e.ExportTransportDocuments).Returns(new[] { additionalInformation.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.TransportDocuments), 1, wrapper.TransportDocuments.Count);
	}

	public override void TestConsignee()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IB4Item.Consignee), wrapper.Consignee);

		var traderMock = new Mock<IEoriTrader>();
		entryLineWrapperMock.Setup(i => i.ExportConsignee).Returns(traderMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IB4Item.Consignee), wrapper.Consignee);
	}

	public override void TestAdditionalSupplyChainActors()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.AdditionalSupplyChainActors), 0, wrapper.AdditionalSupplyChainActors.Count);

		var additionalSupplyChainActor = new Mock<IAdditionalSupplyChainActor>();
		entryLineWrapperMock.Setup(e => e.AdditionalSupplyChainActors).Returns(new[] { additionalSupplyChainActor.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.AdditionalSupplyChainActors), 1, wrapper.AdditionalSupplyChainActors.Count);
	}

	public override void TestTransportChargesMethodOfPayment()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Item.TransportChargesMethodOfPayment), wrapper.TransportChargesMethodOfPayment);

		entryLineWrapperMock.Setup(e => e.ExportTransportChargesMethodOfPayment).Returns("A");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.TransportChargesMethodOfPayment), "A", wrapper.TransportChargesMethodOfPayment);
	}

	public override void TestCountryOfDestination()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Item.CountryOfDestination), itemWrapper.CountryOfDestination);

		entryLineWrapperMock.Setup(e => e.CountryOfDestination).Returns("AU");

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.CountryOfDestination), "AU", itemWrapper.CountryOfDestination);
	}

	public override void TestCountryOfExport()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Item.CountryOfExport), itemWrapper.CountryOfExport);

		entryLineWrapperMock.Setup(e => e.CountryOfExport).Returns("ES");

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.CountryOfExport), "ES", itemWrapper.CountryOfExport);
	}

	public override void TestCountryOfOrigin()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Item.CountryOfOrigin), itemWrapper.CountryOfOrigin);

		entryLineWrapperMock.Setup(e => e.ExportCountryOfOrigin).Returns("GB");

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.CountryOfOrigin), "GB", itemWrapper.CountryOfOrigin);
	}

	public override void TestRegionOfDispatch()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Item.RegionOfDispatch), itemWrapper.RegionOfDispatch);

		entryLineWrapperMock.Setup(e => e.ExportRegionOfDispatch).Returns("EU");

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.RegionOfDispatch), "EU", itemWrapper.RegionOfDispatch);
	}

	public override void TestNetMass()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.NetMass), 0m, wrapper.NetMass);

		entryLineWrapperMock.Setup(e => e.NetMass).Returns(78.9821m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.NetMass), 78.9821m, wrapper.NetMass);
	}

	public override void TestSupplementaryUnit()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IB4Item.SupplementaryUnit), wrapper.SupplementaryUnit);

		entryLineWrapperMock.Setup(e => e.SupplementaryUnit).Returns(22.99m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.SupplementaryUnit), 22.99m, wrapper.SupplementaryUnit);
	}

	public override void TestGrossMass()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.GrossMass), 0m, wrapper.GrossMass);

		entryLineWrapperMock.Setup(e => e.GrossMass).Returns(78.9821m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.GrossMass), 78.9821m, wrapper.GrossMass);
	}

	public override void TestGoodsDescription()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Item.GoodsDescription), wrapper.GoodsDescription);

		entryLineWrapperMock.Setup(e => e.GoodsDescription).Returns("B2_GOODS_DESCRIPTION");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.GoodsDescription), "B2_GOODS_DESCRIPTION", wrapper.GoodsDescription);
	}

	public override void TestPackages()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.Packages), 0, wrapper.Packages.Count);

		var packageMock = new Mock<IPackage>();
		entryLineWrapperMock.Setup(x => x.Packages).Returns(new IPackage[1] { packageMock.Object });
		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.Packages), 1, wrapper.Packages.Count);
	}

	public override void TestCusCode()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Item.CusCode), itemWrapper.CusCode);

		entryLineWrapperMock.Setup(e => e.CusCode).Returns("00100016");

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.CusCode), "00100016", itemWrapper.CusCode);
	}

	public override void TestHsTariffCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Item.HsTariffCode), wrapper.HsTariffCode);

		entryLineWrapperMock.Setup(e => e.ExportHsTariffCode).Returns("HS");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.HsTariffCode), "HS", wrapper.HsTariffCode);
	}

	public override void TestNcTariffCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Item.NcTariffCode), wrapper.NcTariffCode);

		entryLineWrapperMock.Setup(e => e.ExportNcTariffCode).Returns("NC");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.NcTariffCode), "NC", wrapper.NcTariffCode);
	}

	public override void TestDangerousGoodsCodes()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.DangerousGoodsCodes), 0, wrapper.DangerousGoodsCodes.Count);

		entryLineWrapperMock.Setup(e => e.DangerousGoodsCodes).Returns(new[] { "C1", "C2" });
		wrapper = CreateWrapper();
		AssertArrayEqualsByElements(nameof(IB4Item.DangerousGoodsCodes), new[] { "C1", "C2" }, wrapper.DangerousGoodsCodes.ToArray());
	}

	public override void TestNatureOfTransaction()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IB4Item.NatureOfTransaction), wrapper.NatureOfTransaction);

		entryLineWrapperMock.Setup(e => e.ExportNatureOfTransaction).Returns(11);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.NatureOfTransaction), 11, wrapper.NatureOfTransaction);
	}

	public override void TestStatisticalValue()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.StatisticalValue), 0m, wrapper.StatisticalValue);

		entryLineWrapperMock.Setup(e => e.StatisticalValue).Returns(22.99m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Item.StatisticalValue), 22.99m, wrapper.StatisticalValue);
	}

	protected override IB4Item CreateWrapper() => new B4ItemWrapper(EntryLine, exportMessageSendingWrapperFactory);

	protected override void SetUp()
	{
		base.SetUp();
		entryLineWrapperMock = new Mock<ICusEntryLineCustomsMessageWrapper>();

		exportMessageSendingWrapperFactory = new ExportMessageSendingWrapperFactoryMockBuilder()
			.ConfigureGetNewCusEntryLineCustomsMessageWrapper(entryLineWrapperMock.Object)
			.Build();
	}

	Mock<ICusEntryLineCustomsMessageWrapper> entryLineWrapperMock;
	CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory;
}
