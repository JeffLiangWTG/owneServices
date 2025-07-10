using System;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;
using Moq;
using MessageBuilder = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class I1ItemWrapperTest : I1ItemWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Expected exception when argument is null", () => new I1ItemWrapper(null, messageSendingWrapperFactory));
		AssertExceptionThrown<ArgumentNullException>("Expected exception when messageSendingWrapperFactory argument is null", () => new I1ItemWrapper(EntryLine, null));
	}

	public override void TestAdditionalCodes()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(II1Item.AdditionalCodes), wrapper.AdditionalCodes);
		AssertEquals(nameof(II1Item.AdditionalCodes), 0, wrapper.AdditionalCodes.Count);

		entryLineWrapperMock.Setup(e => e.AdditionalCodes).Returns(new[] { "AC1", "AC2" });
		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.AdditionalCodes), 2, wrapper.AdditionalCodes.Count);
		AssertArrayEqualsByElements(nameof(II1Item.AdditionalCodes), new[] { "AC1", "AC2" }, wrapper.AdditionalCodes.ToArray());
	}

	public override void TestAdditionalInformation()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(II1Item.AdditionalInformation), wrapper.AdditionalInformation);
		AssertEquals(nameof(II1Item.AdditionalInformation), 0, wrapper.AdditionalInformation.Count);

		var additionalInformation = new Mock<IAdditionalInformation>();
		entryLineWrapperMock.Setup(e => e.AdditionalInformation).Returns(new[] { additionalInformation.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.AdditionalInformation), 1, wrapper.AdditionalInformation.Count);
	}

	public override void TestAdditionalSupplyChainActors()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(II1Item.AdditionalSupplyChainActors), wrapper.AdditionalSupplyChainActors);
		AssertEquals(nameof(II1Item.AdditionalSupplyChainActors), 0, wrapper.AdditionalSupplyChainActors.Count);

		var additionalSupplyChainActor = new Mock<IAdditionalSupplyChainActor>();
		entryLineWrapperMock.Setup(e => e.AdditionalSupplyChainActors).Returns(new[] { additionalSupplyChainActor.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.AdditionalSupplyChainActors), 1, wrapper.AdditionalSupplyChainActors.Count);
	}

	public override void TestConcessionOrder()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(II1Item.ConcessionOrder), wrapper.ConcessionOrder);

		entryLineWrapperMock.Setup(e => e.ConcessionOrder).Returns("CONCESSION_ORDER");

		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.ConcessionOrder), "CONCESSION_ORDER", wrapper.ConcessionOrder);
	}

	public override void TestContainers()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(II1Item.Containers), wrapper.Containers);
		AssertEquals(nameof(II1Item.Containers), 0, wrapper.Containers.Count);

		entryLineWrapperMock.Setup(e => e.Containers).Returns(new[] { "CNT1", "CNT2" });
		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.Containers), 2, wrapper.Containers.Count);
		AssertArrayEqualsByElements(nameof(II1Item.Containers), new[] { "CNT1", "CNT2" }, wrapper.Containers.ToArray());
	}

	public override void TestCusCode()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(II1Item.CusCode), itemWrapper.CusCode);

		entryLineWrapperMock.Setup(e => e.CusCode).Returns("00100016");

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.CusCode), "00100016", itemWrapper.CusCode);
	}

	public override void TestDispatchCountryCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(II1Item.DispatchCountryCode), wrapper.DispatchCountryCode);

		entryLineWrapperMock.Setup(e => e.DispatchCountryCode).Returns("US");
		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.DispatchCountryCode), "US", wrapper.DispatchCountryCode);
	}

	public override void TestExporter()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(II1Item.Exporter), wrapper.Exporter);

		var exporterMock = TraderWrapperTest.SetupTrader("EXPORTER");
		entryLineWrapperMock.Setup(i => i.Exporter).Returns(exporterMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(II1Item.Exporter), wrapper.Exporter);
		AssertEquals($"{nameof(II1Item.Exporter)}->IdentificationNumber", "EXPORTER", wrapper.Exporter.IdentificationNumber);
	}

	public override void TestGoodsDescription()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(II1Item.GoodsDescription), wrapper.GoodsDescription);

		entryLineWrapperMock.Setup(e => e.GoodsDescription).Returns("GOODS_DESCRIPTION");
		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.GoodsDescription), "GOODS_DESCRIPTION", wrapper.GoodsDescription);
	}

	public override void TestGrossMass()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.GrossMass), 0m, wrapper.GrossMass);

		entryLineWrapperMock.Setup(e => e.GrossMass).Returns(78.9821m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.GrossMass), 78.9821m, wrapper.GrossMass);
	}

	public override void TestItemNumber()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.ItemNumber), 0, wrapper.ItemNumber);

		entryLineWrapperMock.Setup(e => e.ItemNumber).Returns(145);
		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.ItemNumber), 145, wrapper.ItemNumber);
	}

	public override void TestItemPrice()
	{
		entryHeaderWrapperMock.Setup(x => x.InvoiceCurrencyCode).Returns("");
		entryLineWrapperMock.Setup(x => x.ItemPrice).Returns(200.14m);
		var itemWrapper = CreateWrapper();
		AssertNull($"When Currency missing, {nameof(II1Item.ItemPrice)}", itemWrapper.ItemPrice);

		entryHeaderWrapperMock.Setup(x => x.InvoiceCurrencyCode).Returns("FTM");
		itemWrapper = CreateWrapper();
		AssertEquals($"When Currency added, {nameof(II1Item.ItemPrice)}", 200.14m, itemWrapper.ItemPrice);
	}

	public override void TestNationalAdditionalCodes()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(II1Item.NationalAdditionalCodes), wrapper.NationalAdditionalCodes);
		AssertEquals(nameof(II1Item.NationalAdditionalCodes), 0, wrapper.NationalAdditionalCodes.Count);

		entryLineWrapperMock.Setup(e => e.NationalAdditionalCodes).Returns(new[] { "NAC1", "NAC2" });
		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.NationalAdditionalCodes), 2, wrapper.NationalAdditionalCodes.Count);
		AssertArrayEqualsByElements(nameof(II1Item.NationalAdditionalCodes), new[] { "NAC1", "NAC2" }, wrapper.NationalAdditionalCodes.ToArray());
	}

	public override void TestNcCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(II1Item.NcCode), wrapper.NcCode);

		entryLineWrapperMock.Setup(e => e.NcCode).Returns("NC_CODE");
		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.NcCode), "NC_CODE", wrapper.NcCode);
	}

	public override void TestNetMass()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.NetMass), 0m, wrapper.NetMass);

		entryLineWrapperMock.Setup(e => e.NetMass).Returns(78.9821m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.NetMass), 78.9821m, wrapper.NetMass);
	}

	public override void TestOriginCountryCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(II1Item.OriginCountryCode), wrapper.OriginCountryCode);

		entryLineWrapperMock.Setup(e => e.OriginCountryCode).Returns("FR");
		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.OriginCountryCode), "FR", wrapper.OriginCountryCode);
	}

	public override void TestPackages()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(II1Item.Packages), wrapper.Packages);
		AssertEquals(nameof(II1Item.Packages), 0, wrapper.Packages.Count);

		var packageMock = new Mock<IPackage>();
		entryLineWrapperMock.Setup(x => x.Packages).Returns(new IPackage[1] { packageMock.Object });
		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.Packages), 1, wrapper.Packages.Count);
	}

	public override void TestPreferences()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(II1Item.Preferences), wrapper.Preferences);

		entryLineWrapperMock.Setup(e => e.Preferences).Returns(99);
		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.Preferences), 99, wrapper.Preferences);
	}

	public override void TestPreferredOriginCountryCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(II1Item.PreferredOriginCountryCode), wrapper.PreferredOriginCountryCode);

		entryLineWrapperMock.Setup(e => e.PreferredOriginCountryCode).Returns("US");
		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.PreferredOriginCountryCode), "US", wrapper.PreferredOriginCountryCode);
	}

	public override void TestPreviousDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(II1Item.PreviousDocuments), wrapper.PreviousDocuments);
		AssertEquals(nameof(II1Item.PreviousDocuments), 0, wrapper.PreviousDocuments.Count);

		var previousDocument = new Mock<IPreviousDocument>();

		entryLineWrapperMock.Setup(e => e.PreviousDocuments).Returns(new[] { previousDocument.Object });
		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.PreviousDocuments), 1, wrapper.PreviousDocuments.Count);
	}

	public override void TestProcedure()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(II1Item.Procedure), wrapper.Procedure);

		var cusProcedureMock = new Mock<ICustomsProcedure>();
		entryLineWrapperMock.Setup(i => i.Procedure).Returns(cusProcedureMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(II1Item.Procedure), wrapper.Procedure);
	}

	public override void TestSupplementaryUnit()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(II1Item.SupplementaryUnit), wrapper.SupplementaryUnit);

		entryLineWrapperMock.Setup(e => e.SupplementaryUnit).Returns(22.99m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.SupplementaryUnit), 22.99m, wrapper.SupplementaryUnit);
	}

	public override void TestSupportingDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(II1Item.SupportingDocuments), wrapper.SupportingDocuments);
		AssertEquals(nameof(II1Item.SupportingDocuments), 0, wrapper.SupportingDocuments.Count);

		var supportingDocument = new Mock<MessageBuilder.ISupportingDocument>();
		entryLineWrapperMock.Setup(e => e.SupportingDocuments).Returns(new[] { supportingDocument.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.SupportingDocuments), 1, wrapper.SupportingDocuments.Count);
	}

	public override void TestTaricCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(II1Item.TaricCode), wrapper.TaricCode);

		entryLineWrapperMock.Setup(e => e.TaricCode).Returns("CODE_TR122");
		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Item.TaricCode), "CODE_TR122", wrapper.TaricCode);
	}

	protected override II1Item CreateWrapper() => new I1ItemWrapper(EntryLine, messageSendingWrapperFactory);

	protected override void SetUp()
	{
		base.SetUp();
		entryHeaderWrapperMock = new Mock<ICusEntryHeaderCustomsMessageWrapper>();
		entryLineWrapperMock = new Mock<ICusEntryLineCustomsMessageWrapper>();

		messageSendingWrapperFactory = new MessageSendingWrapperFactoryMockBuilder()
			.ConfigureGetNewCusEntryHeaderCustomsMessageWrapper(entryHeaderWrapperMock.Object)
			.ConfigureGetNewCusEntryLineCustomsMessageWrapper(entryLineWrapperMock.Object)
			.Build();
	}

	Mock<ICusEntryHeaderCustomsMessageWrapper> entryHeaderWrapperMock;
	Mock<ICusEntryLineCustomsMessageWrapper> entryLineWrapperMock;
	CustomsMessageSending.IT.IMessageSendingWrapperFactory messageSendingWrapperFactory;
}
