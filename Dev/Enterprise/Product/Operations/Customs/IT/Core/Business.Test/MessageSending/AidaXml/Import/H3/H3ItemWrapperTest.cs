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

sealed class H3ItemWrapperTest : H3ItemWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Expected exception when argument is null", () => new H3ItemWrapper(entryLine: null, messageSendingWrapperFactory));
	}

	public override void TestTransactionNature()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.TransactionNature), 0, wrapper.TransactionNature);

		entryLineWrapperMock.Setup(e => e.TransactionNature).Returns(11);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.TransactionNature), 11, wrapper.TransactionNature);
	}

	public override void TestStatisticalValue()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.StatisticalValue), 0m, wrapper.StatisticalValue);

		entryLineWrapperMock.Setup(e => e.StatisticalValue).Returns(22.99m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.StatisticalValue), 22.99m, wrapper.StatisticalValue);
	}

	public override void TestContainers()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH3Item.Containers), wrapper.Containers);
		AssertEquals(nameof(IH3Item.Containers), 0, wrapper.Containers.Count);

		entryLineWrapperMock.Setup(e => e.Containers).Returns(new[] { "CNT1", "CNT2" });
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.Containers), 2, wrapper.Containers.Count);
		AssertArrayEqualsByElements(nameof(IH3Item.Containers), new[] { "CNT1", "CNT2" }, wrapper.Containers.ToArray());
	}

	public override void TestSupplementaryUnit()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH3Item.SupplementaryUnit), wrapper.SupplementaryUnit);

		entryLineWrapperMock.Setup(e => e.SupplementaryUnit).Returns(22.99m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.SupplementaryUnit), 22.99m, wrapper.SupplementaryUnit);
	}

	public override void TestGrossMass()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.GrossMass), 0m, wrapper.GrossMass);

		entryLineWrapperMock.Setup(e => e.GrossMass).Returns(78.9821m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.GrossMass), 78.9821m, wrapper.GrossMass);
	}

	public override void TestGoodsDescription()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH3Item.GoodsDescription), wrapper.GoodsDescription);

		entryLineWrapperMock.Setup(e => e.GoodsDescription).Returns("GOODS_DESCRIPTION");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.GoodsDescription), "GOODS_DESCRIPTION", wrapper.GoodsDescription);
	}

	public override void TestCusCode()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH3Item.CusCode), itemWrapper.CusCode);

		entryLineWrapperMock.Setup(e => e.CusCode).Returns("00100016");

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.CusCode), "00100016", itemWrapper.CusCode);
	}

	public override void TestNcCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH3Item.NcCode), wrapper.NcCode);

		entryLineWrapperMock.Setup(e => e.NcCode).Returns("NC_CODE");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.NcCode), "NC_CODE", wrapper.NcCode);
	}

	public override void TestTaricCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH3Item.TaricCode), wrapper.TaricCode);

		entryLineWrapperMock.Setup(e => e.TaricCode).Returns("CODE_TR122");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.TaricCode), "CODE_TR122", wrapper.TaricCode);
	}

	public override void TestAdditionalCodes()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH3Item.AdditionalCodes), wrapper.AdditionalCodes);
		AssertEquals(nameof(IH3Item.AdditionalCodes), 0, wrapper.AdditionalCodes.Count);

		entryLineWrapperMock.Setup(e => e.AdditionalCodes).Returns(new[] { "AC1", "AC2" });
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.AdditionalCodes), 2, wrapper.AdditionalCodes.Count);
		AssertArrayEqualsByElements(nameof(IH3Item.AdditionalCodes), new[] { "AC1", "AC2" }, wrapper.AdditionalCodes.ToArray());
	}

	public override void TestNationalAdditionalCodes()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH3Item.NationalAdditionalCodes), wrapper.NationalAdditionalCodes);
		AssertEquals(nameof(IH3Item.NationalAdditionalCodes), 0, wrapper.NationalAdditionalCodes.Count);

		entryLineWrapperMock.Setup(e => e.NationalAdditionalCodes).Returns(new[] { "NAC1", "NAC2" });
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.NationalAdditionalCodes), 2, wrapper.NationalAdditionalCodes.Count);
		AssertArrayEqualsByElements(nameof(IH3Item.NationalAdditionalCodes), new[] { "NAC1", "NAC2" }, wrapper.NationalAdditionalCodes.ToArray());
	}

	public override void TestPackages()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH3Item.Packages), wrapper.Packages);
		AssertEquals(nameof(IH3Item.Packages), 0, wrapper.Packages.Count);

		var packageMock = new Mock<IPackage>();
		entryLineWrapperMock.Setup(x => x.Packages).Returns(new IPackage[1] { packageMock.Object });
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.Packages), 1, wrapper.Packages.Count);
	}

	public override void TestDestinationCountryCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH3Item.DestinationCountryCode), wrapper.DestinationCountryCode);

		entryLineWrapperMock.Setup(e => e.DestinationCountryCode).Returns("IT");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.DestinationCountryCode), "IT", wrapper.DestinationCountryCode);
	}

	public override void TestOriginCountryCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH3Item.OriginCountryCode), wrapper.OriginCountryCode);

		entryLineWrapperMock.Setup(e => e.OriginCountryCode).Returns("FR");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.OriginCountryCode), "FR", wrapper.OriginCountryCode);
	}

	public override void TestDestinationStateCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH3Item.DestinationStateCode), wrapper.DestinationStateCode);

		entryLineWrapperMock.Setup(e => e.DestinationStateCode).Returns("BW");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.DestinationStateCode), "BW", wrapper.DestinationStateCode);
	}

	public override void TestDispatchCountryCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH3Item.DispatchCountryCode), wrapper.DispatchCountryCode);

		entryLineWrapperMock.Setup(e => e.DispatchCountryCode).Returns("US");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.DestinationStateCode), "US", wrapper.DispatchCountryCode);
	}

	public override void TestPreferredOriginCountryCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH3Item.PreferredOriginCountryCode), wrapper.PreferredOriginCountryCode);

		entryLineWrapperMock.Setup(e => e.PreferredOriginCountryCode).Returns("US");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.PreferredOriginCountryCode), "US", wrapper.PreferredOriginCountryCode);
	}

	public override void TestAcceptanceDate()
	{
		var wrapper = CreateWrapper();
		AssertNull($"{nameof(IH3Item.AcceptanceDate)} not managed now", wrapper.AcceptanceDate);
	}

	public override void TestFees()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH3Item.Fees), wrapper.Fees);
		AssertEquals(nameof(IH3Item.Fees), 0, wrapper.Fees.Count);

		var feeMock = new Mock<MessageBuilder.IFee>();
		entryLineWrapperMock.Setup(i => i.Fees).Returns(new[] { feeMock.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.Fees), 1, wrapper.Fees.Count);
	}

	public override void TestTotalFeeAmount()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.TotalFeeAmount), 0m, wrapper.TotalFeeAmount);

		entryLineWrapperMock.Setup(e => e.TotalFeeAmount).Returns(22.99m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.TotalFeeAmount), 22.99m, wrapper.TotalFeeAmount);
	}

	public override void TestItemPrice()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.ItemPrice), 0m, wrapper.ItemPrice);

		entryLineWrapperMock.Setup(e => e.ItemPrice).Returns(9.99m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.ItemPrice), 9.99m, wrapper.ItemPrice);
	}

	public override void TestValuationMethod()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.ValuationMethod), 0, wrapper.ValuationMethod);

		entryLineWrapperMock.Setup(e => e.ValuationMethod).Returns(12);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.ValuationMethod), 12, wrapper.ValuationMethod);
	}

	public override void TestPreferences()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.Preferences), 0, wrapper.Preferences);

		entryLineWrapperMock.Setup(e => e.Preferences).Returns(99);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.Preferences), 99, wrapper.Preferences);
	}

	public override void TestExporter()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH3Item.Exporter), wrapper.Exporter);

		var exporterMock = TraderWrapperTest.SetupTrader("EXPORTER");
		entryLineWrapperMock.Setup(i => i.Exporter).Returns(exporterMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH3Item.Exporter), wrapper.Exporter);
		AssertEquals($"{nameof(IH3Item.Exporter)}->IdentificationNumber", "EXPORTER", wrapper.Exporter.IdentificationNumber);
	}

	public override void TestAdditionalSupplyChainActors()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH3Item.AdditionalSupplyChainActors), wrapper.AdditionalSupplyChainActors);
		AssertEquals(nameof(IH3Item.AdditionalSupplyChainActors), 0, wrapper.AdditionalSupplyChainActors.Count);

		var additionalSupplyChainActor = new Mock<IAdditionalSupplyChainActor>();
		entryLineWrapperMock.Setup(e => e.AdditionalSupplyChainActors).Returns(new[] { additionalSupplyChainActor.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.AdditionalSupplyChainActors), 1, wrapper.AdditionalSupplyChainActors.Count);
	}

	public override void TestPreviousDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH3Item.PreviousDocuments), wrapper.PreviousDocuments);
		AssertEquals(nameof(IH3Item.PreviousDocuments), 0, wrapper.PreviousDocuments.Count);

		var previousDocument = new Mock<IPreviousDocument>();

		entryLineWrapperMock.Setup(e => e.PreviousDocuments).Returns(new[] { previousDocument.Object });
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.PreviousDocuments), 1, wrapper.PreviousDocuments.Count);
	}

	public override void TestAdditionalInformation()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH3Item.AdditionalInformation), wrapper.AdditionalInformation);
		AssertEquals(nameof(IH3Item.AdditionalInformation), 0, wrapper.AdditionalInformation.Count);

		var additionalInformation = new Mock<IAdditionalInformation>();
		entryLineWrapperMock.Setup(e => e.AdditionalInformation).Returns(new[] { additionalInformation.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.AdditionalInformation), 1, wrapper.AdditionalInformation.Count);
	}

	public override void TestSupportingDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH3Item.SupportingDocuments), wrapper.SupportingDocuments);
		AssertEquals(nameof(IH3Item.SupportingDocuments), 0, wrapper.SupportingDocuments.Count);

		var supportingDocument = new Mock<MessageBuilder.ISupportingDocument>();
		entryLineWrapperMock.Setup(e => e.SupportingDocuments).Returns(new[] { supportingDocument.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.SupportingDocuments), 1, wrapper.SupportingDocuments.Count);
	}

	public override void TestItemNumber()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.ItemNumber), 0, wrapper.ItemNumber);

		entryLineWrapperMock.Setup(e => e.ItemNumber).Returns(145);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH3Item.ItemNumber), 145, wrapper.ItemNumber);
	}

	public override void TestProcedure()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH3Item.Procedure), wrapper.Procedure);

		var cusProcedureMock = new Mock<ICustomsProcedure>();
		entryLineWrapperMock.Setup(i => i.Procedure).Returns(cusProcedureMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH3Item.Procedure), wrapper.Procedure);
	}

	protected override IH3Item CreateWrapper() => new H3ItemWrapper(EntryLine, messageSendingWrapperFactory);

	protected override void SetUp()
	{
		base.SetUp();
		entryLineWrapperMock = new Mock<ICusEntryLineCustomsMessageWrapper>();

		messageSendingWrapperFactory = new MessageSendingWrapperFactoryMockBuilder()
			.ConfigureGetNewCusEntryLineCustomsMessageWrapper(entryLineWrapperMock.Object)
			.Build();
	}

	Mock<ICusEntryLineCustomsMessageWrapper> entryLineWrapperMock;
	CustomsMessageSending.IT.IMessageSendingWrapperFactory messageSendingWrapperFactory;
}
