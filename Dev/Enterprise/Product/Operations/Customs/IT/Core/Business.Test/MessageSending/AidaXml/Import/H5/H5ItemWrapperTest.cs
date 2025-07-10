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

sealed class H5ItemWrapperTest : H5ItemWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Expected exception when argument is null", () => new H5ItemWrapper(entryLine: null, messageSendingWrapperFactory));
		AssertExceptionThrown<ArgumentNullException>("Expected exception when messageSendingWrapperFactory argument is null", () => new H5ItemWrapper(EntryLine, null));
	}

	public override void TestAcceptanceDate()
	{
		var wrapper = CreateWrapper();
		AssertNull($"{nameof(IH5Item.AcceptanceDate)} not managed now", wrapper.AcceptanceDate);
	}

	public override void TestAdditionalCodes()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.AdditionalCodes), 0, wrapper.AdditionalCodes.Count);

		entryLineWrapperMock.Setup(e => e.AdditionalCodes).Returns(new[] { "AC1", "AC2" });
		wrapper = CreateWrapper();
		AssertArrayEqualsByElements(nameof(IH5Item.AdditionalCodes), new[] { "AC1", "AC2" }, wrapper.AdditionalCodes.ToArray());
	}

	public override void TestAdditionalInformation()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.AdditionalInformation), 0, wrapper.AdditionalInformation.Count);

		var additionalInformation = new Mock<IAdditionalInformation>();
		entryLineWrapperMock.Setup(e => e.AdditionalInformation).Returns(new[] { additionalInformation.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.AdditionalInformation), 1, wrapper.AdditionalInformation.Count);
	}

	public override void TestAdditionalSupplyChainActors()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.AdditionalSupplyChainActors), 0, wrapper.AdditionalSupplyChainActors.Count);

		var additionalSupplyChainActor = new Mock<IAdditionalSupplyChainActor>();
		entryLineWrapperMock.Setup(e => e.AdditionalSupplyChainActors).Returns(new[] { additionalSupplyChainActor.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.AdditionalSupplyChainActors), 1, wrapper.AdditionalSupplyChainActors.Count);
	}

	public override void TestAdditionOrDeductions()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.AdditionOrDeductions), 0, wrapper.AdditionOrDeductions.Count);

		var additionOrDeduction = new Mock<IAdditionOrDeduction>();
		entryLineWrapperMock.Setup(e => e.AdditionOrDeductions).Returns(new[] { additionOrDeduction.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.AdditionOrDeductions), 1, wrapper.AdditionOrDeductions.Count);
	}

	public override void TestContainers()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.Containers), 0, wrapper.Containers.Count);

		entryLineWrapperMock.Setup(e => e.Containers).Returns(new[] { "CNT1", "CNT2" });
		wrapper = CreateWrapper();
		AssertArrayEqualsByElements(nameof(IH5Item.Containers), new[] { "CNT1", "CNT2" }, wrapper.Containers.ToArray());
	}

	public override void TestDestinationCountryCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH5Item.DestinationCountryCode), wrapper.DestinationCountryCode);

		entryLineWrapperMock.Setup(e => e.DestinationCountryCode).Returns("IT");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.DestinationCountryCode), "IT", wrapper.DestinationCountryCode);
	}

	public override void TestDestinationStateCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH5Item.DestinationStateCode), wrapper.DestinationStateCode);

		entryLineWrapperMock.Setup(e => e.DestinationStateCode).Returns("BW");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.DestinationStateCode), "BW", wrapper.DestinationStateCode);
	}

	public override void TestDispatchCountryCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH5Item.DispatchCountryCode), wrapper.DispatchCountryCode);

		entryLineWrapperMock.Setup(e => e.DispatchCountryCode).Returns("US");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.DestinationStateCode), "US", wrapper.DispatchCountryCode);
	}

	public override void TestExporter()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH5Item.Exporter), wrapper.Exporter);

		var exporterMock = TraderWrapperTest.SetupTrader("EXPORTER");
		entryLineWrapperMock.Setup(i => i.Exporter).Returns(exporterMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH5Item.Exporter), wrapper.Exporter);
		AssertEquals($"{nameof(IH5Item.Exporter)}->IdentificationNumber", "EXPORTER", wrapper.Exporter.IdentificationNumber);
	}

	public override void TestFees()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.Fees), 0, wrapper.Fees.Count);

		var feeMock = new Mock<MessageBuilder.IFee>();
		entryLineWrapperMock.Setup(i => i.Fees).Returns(new[] { feeMock.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.Fees), 1, wrapper.Fees.Count);
	}

	public override void TestGoodsDescription()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH5Item.GoodsDescription), wrapper.GoodsDescription);

		entryLineWrapperMock.Setup(e => e.GoodsDescription).Returns("GOODS_DESCRIPTION");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.GoodsDescription), "GOODS_DESCRIPTION", wrapper.GoodsDescription);
	}

	public override void TestGrossMass()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.GrossMass), 0m, wrapper.GrossMass);

		entryLineWrapperMock.Setup(e => e.GrossMass).Returns(78.9821m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.GrossMass), 78.9821m, wrapper.GrossMass);
	}

	public override void TestItemNumber()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.ItemNumber), 0, wrapper.ItemNumber);

		entryLineWrapperMock.Setup(e => e.ItemNumber).Returns(145);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.ItemNumber), 145, wrapper.ItemNumber);
	}

	public override void TestItemPrice()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.ItemPrice), 0m, wrapper.ItemPrice);

		entryLineWrapperMock.Setup(e => e.ItemPrice).Returns(9.99m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.ItemPrice), 9.99m, wrapper.ItemPrice);
	}

	public override void TestNationalAdditionalCodes()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.NationalAdditionalCodes), 0, wrapper.NationalAdditionalCodes.Count);

		entryLineWrapperMock.Setup(e => e.NationalAdditionalCodes).Returns(new[] { "NAC1", "NAC2" });
		wrapper = CreateWrapper();
		AssertArrayEqualsByElements(nameof(IH5Item.NationalAdditionalCodes), new[] { "NAC1", "NAC2" }, wrapper.NationalAdditionalCodes.ToArray());
	}

	public override void TestCusCode()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH5Item.CusCode), itemWrapper.CusCode);

		entryLineWrapperMock.Setup(e => e.CusCode).Returns("00100016");

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.CusCode), "00100016", itemWrapper.CusCode);
	}

	public override void TestNcCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH5Item.NcCode), wrapper.NcCode);

		entryLineWrapperMock.Setup(e => e.NcCode).Returns("NC_CODE");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.NcCode), "NC_CODE", wrapper.NcCode);
	}

	public override void TestNetMass()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.NetMass), 0m, wrapper.NetMass);

		entryLineWrapperMock.Setup(e => e.NetMass).Returns(78.9821m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.NetMass), 78.9821m, wrapper.NetMass);
	}

	public override void TestOriginCountryCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH5Item.OriginCountryCode), wrapper.OriginCountryCode);

		entryLineWrapperMock.Setup(e => e.OriginCountryCode).Returns("FR");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.OriginCountryCode), "FR", wrapper.OriginCountryCode);
	}

	public override void TestPackages()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.Packages), 0, wrapper.Packages.Count);

		var packageMock = new Mock<IPackage>();
		entryLineWrapperMock.Setup(x => x.Packages).Returns(new IPackage[1] { packageMock.Object });
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.Packages), 1, wrapper.Packages.Count);
	}

	public override void TestPreferences()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.Preferences), 0, wrapper.Preferences);

		entryLineWrapperMock.Setup(e => e.Preferences).Returns(99);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.Preferences), 99, wrapper.Preferences);
	}

	public override void TestPreferredOriginCountryCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH5Item.PreferredOriginCountryCode), wrapper.PreferredOriginCountryCode);

		entryLineWrapperMock.Setup(e => e.PreferredOriginCountryCode).Returns("US");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.PreferredOriginCountryCode), "US", wrapper.PreferredOriginCountryCode);
	}

	public override void TestPreviousDocuments()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.PreviousDocuments), 0, wrapper.PreviousDocuments.Count);

		var previousDocument = new Mock<IPreviousDocument>();

		entryLineWrapperMock.Setup(e => e.PreviousDocuments).Returns(new[] { previousDocument.Object });
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.PreviousDocuments), 1, wrapper.PreviousDocuments.Count);
	}

	public override void TestProcedure()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH5Item.Procedure), wrapper.Procedure);

		var cusProcedureMock = new Mock<ICustomsProcedure>();
		entryLineWrapperMock.Setup(i => i.Procedure).Returns(cusProcedureMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH5Item.Procedure), wrapper.Procedure);
	}

	public override void TestRelatedIndicator()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH5Item.RelatedIndicator), wrapper.RelatedIndicator);

		entryLineWrapperMock.Setup(e => e.RelatedIndicator).Returns("1111");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.RelatedIndicator), "1111", wrapper.RelatedIndicator);
	}

	public override void TestStatisticalValue()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.StatisticalValue), 0m, wrapper.StatisticalValue);

		entryLineWrapperMock.Setup(e => e.StatisticalValue).Returns(22.99m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.StatisticalValue), 22.99m, wrapper.StatisticalValue);
	}

	public override void TestSupplementaryUnit()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH5Item.SupplementaryUnit), wrapper.SupplementaryUnit);

		entryLineWrapperMock.Setup(e => e.SupplementaryUnit).Returns(22.99m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.SupplementaryUnit), 22.99m, wrapper.SupplementaryUnit);
	}

	public override void TestSupportingDocuments()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.SupportingDocuments), 0, wrapper.SupportingDocuments.Count);

		var supportingDocument = new Mock<MessageBuilder.ISupportingDocument>();
		entryLineWrapperMock.Setup(e => e.SupportingDocuments).Returns(new[] { supportingDocument.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.SupportingDocuments), 1, wrapper.SupportingDocuments.Count);
	}

	public override void TestTaricCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH5Item.TaricCode), wrapper.TaricCode);

		entryLineWrapperMock.Setup(e => e.TaricCode).Returns("CODE_TR122");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.TaricCode), "CODE_TR122", wrapper.TaricCode);
	}

	public override void TestTotalFeeAmount()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.TotalFeeAmount), 0m, wrapper.TotalFeeAmount);

		entryLineWrapperMock.Setup(e => e.TotalFeeAmount).Returns(22.99m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.TotalFeeAmount), 22.99m, wrapper.TotalFeeAmount);
	}

	public override void TestTransactionNature()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.TransactionNature), 0, wrapper.TransactionNature);

		entryLineWrapperMock.Setup(e => e.TransactionNature).Returns(11);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.TransactionNature), 11, wrapper.TransactionNature);
	}

	public override void TestValuationMethod()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.ValuationMethod), 0, wrapper.ValuationMethod);

		entryLineWrapperMock.Setup(e => e.ValuationMethod).Returns(12);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Item.ValuationMethod), 12, wrapper.ValuationMethod);
	}

	protected override IH5Item CreateWrapper() => new H5ItemWrapper(EntryLine, messageSendingWrapperFactory);

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
