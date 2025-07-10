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

sealed class H2ItemWrapperTest : H2ItemWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Expected exception when argument is null", () => new H2ItemWrapper(null, messageSendingWrapperFactory));
		AssertExceptionThrown<ArgumentNullException>("Expected exception when messageSendingWrapperFactory argument is null", () => new H2ItemWrapper(EntryLine, null));
	}

	public override void TestItemNumber()
	{
		var itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.ItemNumber), 0, itemWrapper.ItemNumber);

		entryLineWrapperMock.Setup(e => e.ItemNumber).Returns(9);

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.ItemNumber), 9, itemWrapper.ItemNumber);
	}

	public override void TestProcedure()
	{
		var itemWrapper = CreateWrapper();
		AssertNull(nameof(IH2Item.Procedure), itemWrapper.Procedure);

		var cusProcedureMock = new Mock<ICustomsProcedure>();
		entryLineWrapperMock.Setup(i => i.Procedure).Returns(cusProcedureMock.Object);

		itemWrapper = CreateWrapper();
		var procedure = itemWrapper.Procedure;
		AssertNotNull(nameof(IH2Item.Procedure), procedure);
	}

	public override void TestPreviousDocuments()
	{
		var itemWrapper = CreateWrapper();
		AssertNotNull(nameof(IH2Item.PreviousDocuments), itemWrapper.PreviousDocuments);
		AssertArrayEqualsByElements(nameof(IH2Item.PreviousDocuments), Array.Empty<IPreviousDocument>(), itemWrapper.PreviousDocuments.ToArray());

		var previousDocument = new Mock<IPreviousDocument>();
		entryLineWrapperMock.Setup(e => e.PreviousDocuments).Returns(new[] { previousDocument.Object });

		itemWrapper = CreateWrapper();
		var previousDocuments = itemWrapper.PreviousDocuments;
		AssertEquals($"{nameof(IH2Item.PreviousDocuments)} count", 1, previousDocuments.Count);
	}

	public override void TestAdditionalInformation()
	{
		var itemWrapper = CreateWrapper();
		AssertNotNull(nameof(IH2Item.AdditionalInformation), itemWrapper.AdditionalInformation);

		var additionalInformationMock = new Mock<IAdditionalInformation>();
		entryLineWrapperMock.Setup(e => e.AdditionalInformation).Returns(new[] { additionalInformationMock.Object });

		itemWrapper = CreateWrapper();
		var additionalInformation = itemWrapper.AdditionalInformation;
		AssertEquals($"{nameof(IH2Item.AdditionalInformation)} count", 1, additionalInformation.Count);
	}

	public override void TestSupportingDocuments()
	{
		var itemWrapper = CreateWrapper();
		AssertNotNull(nameof(IH2Item.SupportingDocuments), itemWrapper.SupportingDocuments);
		AssertArrayEqualsByElements(nameof(IH2Item.SupportingDocuments), Array.Empty<ISupportingDocument>(), itemWrapper.SupportingDocuments.ToArray());

		var supportingDocument = new Mock<MessageBuilder.ISupportingDocument>();
		entryLineWrapperMock.Setup(e => e.SupportingDocuments).Returns(new[] { supportingDocument.Object });

		itemWrapper = CreateWrapper();
		var supportingDocuments = itemWrapper.SupportingDocuments;
		AssertEquals($"{nameof(IH2Item.SupportingDocuments)} count", 1, supportingDocuments.Count);
	}

	public override void TestAdditionalSupplyChainActors()
	{
		var itemWrapper = CreateWrapper();
		AssertNotNull(nameof(IH2Item.AdditionalSupplyChainActors), itemWrapper.AdditionalSupplyChainActors);
		AssertArrayEqualsByElements(nameof(IH2Item.AdditionalSupplyChainActors), Array.Empty<IAdditionalSupplyChainActor>(), itemWrapper.AdditionalSupplyChainActors.ToArray());

		var additionalSupplyChainActor = new Mock<IAdditionalSupplyChainActor>();
		entryLineWrapperMock.Setup(e => e.AdditionalSupplyChainActors).Returns(new[] { additionalSupplyChainActor.Object });

		itemWrapper = CreateWrapper();
		var additionalSupplyChainActors = itemWrapper.AdditionalSupplyChainActors;
		AssertEquals($"{nameof(IH2Item.AdditionalSupplyChainActors)} count", 1, additionalSupplyChainActors.Count);
	}

	public override void TestBaseAmounts()
	{
		var itemWrapper = CreateWrapper();
		AssertNotNull(nameof(IH2Item.BaseAmounts), itemWrapper.BaseAmounts);
		AssertArrayEqualsByElements(nameof(IH2Item.BaseAmounts), Array.Empty<IBaseAmount>(), itemWrapper.BaseAmounts.ToArray());

		var baseAmountMock = new Mock<IBaseAmount>();
		entryLineWrapperMock.Setup(e => e.BaseAmounts).Returns(new[] { baseAmountMock.Object });

		itemWrapper = CreateWrapper();
		var baseAmounts = itemWrapper.BaseAmounts;
		AssertEquals($"{nameof(IH2Item.BaseAmounts)} count", 1, baseAmounts.Count);
	}

	public override void TestPreferences()
	{
		var itemWrapper = CreateWrapper();
		AssertNull(nameof(IH2Item.Preferences), itemWrapper.Preferences);

		entryLineWrapperMock.Setup(e => e.Preferences).Returns(99);

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.Preferences), 99, itemWrapper.Preferences);
	}

	public override void TestDestinationCountryCode()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH2Item.DestinationCountryCode), itemWrapper.DestinationCountryCode);

		entryLineWrapperMock.Setup(e => e.DestinationCountryCode).Returns("CN");

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.DestinationCountryCode), "CN", itemWrapper.DestinationCountryCode);
	}

	public override void TestDestinationStateCode()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH2Item.DestinationStateCode), itemWrapper.DestinationStateCode);

		entryLineWrapperMock.Setup(e => e.DestinationStateCode).Returns("SM");

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.DestinationStateCode), "SM", itemWrapper.DestinationStateCode);
	}

	public override void TestDispatchCountryCode()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH2Item.DispatchCountryCode), itemWrapper.DispatchCountryCode);

		entryLineWrapperMock.Setup(e => e.DispatchCountryCode).Returns("DE");

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.DispatchCountryCode), "DE", itemWrapper.DispatchCountryCode);
	}

	public override void TestOriginCountryCode()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH2Item.OriginCountryCode), itemWrapper.OriginCountryCode);

		entryLineWrapperMock.Setup(e => e.OriginCountryCode).Returns("ES");
		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.OriginCountryCode), "ES", itemWrapper.OriginCountryCode);
	}

	public override void TestPreferredOriginCountryCode()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH2Item.PreferredOriginCountryCode), itemWrapper.PreferredOriginCountryCode);

		entryLineWrapperMock.Setup(e => e.PreferredOriginCountryCode).Returns("ES");

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.PreferredOriginCountryCode), "ES", itemWrapper.PreferredOriginCountryCode);
	}

	public override void TestSupplementaryUnit()
	{
		var itemWrapper = CreateWrapper();
		AssertNull(nameof(IH2Item.SupplementaryUnit), itemWrapper.SupplementaryUnit);

		entryLineWrapperMock.Setup(e => e.SupplementaryUnit).Returns(22.60m);

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.SupplementaryUnit), 22.60m, itemWrapper.SupplementaryUnit);
	}

	public override void TestGrossMass()
	{
		var itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.GrossMass), 0m, itemWrapper.GrossMass);

		entryLineWrapperMock.Setup(e => e.GrossMass).Returns(1400.3m);

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.GrossMass), 1400.3m, itemWrapper.GrossMass);
	}

	public override void TestGoodsDescription()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH2Item.GoodsDescription), itemWrapper.GoodsDescription);

		entryLineWrapperMock.Setup(e => e.GoodsDescription).Returns("GOODS DESCRIPTION");

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.GoodsDescription), "GOODS DESCRIPTION", itemWrapper.GoodsDescription);
	}

	public override void TestPackages()
	{
		var itemWrapper = CreateWrapper();
		AssertNotNull(nameof(IH2Item.Packages), itemWrapper.Packages);
		AssertArrayEqualsByElements(nameof(IH2Item.Packages), Array.Empty<IPackage>(), itemWrapper.Packages.ToArray());

		var packageMock = new Mock<IPackage>();
		entryLineWrapperMock.Setup(x => x.Packages).Returns(new IPackage[1] { packageMock.Object });

		itemWrapper = CreateWrapper();
		var packages = itemWrapper.Packages;
		AssertEquals($"{nameof(IH2Item.Packages)} count", 1, packages.Count);
	}

	public override void TestCusCode()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH2Item.CusCode), itemWrapper.CusCode);

		entryLineWrapperMock.Setup(e => e.CusCode).Returns("00100016");

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.CusCode), "00100016", itemWrapper.CusCode);
	}

	public override void TestNcCode()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH2Item.NcCode), itemWrapper.NcCode);

		entryLineWrapperMock.Setup(e => e.NcCode).Returns("01234567");

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.NcCode), "01234567", itemWrapper.NcCode);
	}

	public override void TestTaricCode()
	{
		var itemWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH2Item.TaricCode), itemWrapper.TaricCode);

		entryLineWrapperMock.Setup(e => e.TaricCode).Returns("89");

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.TaricCode), "89", itemWrapper.TaricCode);
	}

	public override void TestAdditionalCodes()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH2Item.AdditionalCodes), wrapper.AdditionalCodes);
		AssertEquals(nameof(IH2Item.AdditionalCodes), 0, wrapper.AdditionalCodes.Count);

		entryLineWrapperMock.Setup(e => e.AdditionalCodes).Returns(new[] { "AC1", "AC2" });
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.AdditionalCodes), 2, wrapper.AdditionalCodes.Count);
		AssertArrayEqualsByElements(nameof(IH2Item.AdditionalCodes), new[] { "AC1", "AC2" }, wrapper.AdditionalCodes.ToArray());
	}

	public override void TestNationalAdditionalCodes()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH2Item.NationalAdditionalCodes), wrapper.NationalAdditionalCodes);
		AssertEquals(nameof(IH2Item.NationalAdditionalCodes), 0, wrapper.NationalAdditionalCodes.Count);

		entryLineWrapperMock.Setup(e => e.NationalAdditionalCodes).Returns(new[] { "NAC1", "NAC2" });
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.NationalAdditionalCodes), 2, wrapper.NationalAdditionalCodes.Count);
		AssertArrayEqualsByElements(nameof(IH2Item.NationalAdditionalCodes), new[] { "NAC1", "NAC2" }, wrapper.NationalAdditionalCodes.ToArray());
	}

	public override void TestContainers()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH2Item.Containers), wrapper.Containers);
		AssertEquals(nameof(IH2Item.Containers), 0, wrapper.Containers.Count);

		entryLineWrapperMock.Setup(e => e.Containers).Returns(new[] { "CNT1", "CNT2" });
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.Containers), 2, wrapper.Containers.Count);
		AssertArrayEqualsByElements(nameof(IH2Item.Containers), new[] { "CNT1", "CNT2" }, wrapper.Containers.ToArray());
	}

	public override void TestTransactionNature()
	{
		var itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.TransactionNature), 0, itemWrapper.TransactionNature);

		entryLineWrapperMock.Setup(e => e.TransactionNature).Returns(11);
		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.TransactionNature), 11, itemWrapper.TransactionNature);

		entryLineWrapperMock.Setup(e => e.TransactionNature).Returns(0);
		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.TransactionNature), 0, itemWrapper.TransactionNature);
	}

	public override void TestStatisticalValue()
	{
		var itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.StatisticalValue), 0m, itemWrapper.StatisticalValue);

		entryLineWrapperMock.Setup(e => e.StatisticalValue).Returns(18.3m);

		itemWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Item.StatisticalValue), 18.3m, itemWrapper.StatisticalValue);
	}

	protected override void SetUp()
	{
		base.SetUp();
		entryLineWrapperMock = new Mock<ICusEntryLineCustomsMessageWrapper>();

		messageSendingWrapperFactory = new MessageSendingWrapperFactoryMockBuilder()
			.ConfigureGetNewCusEntryLineCustomsMessageWrapper(entryLineWrapperMock.Object)
			.Build();
	}

	protected override IH2Item CreateWrapper() => new H2ItemWrapper(EntryLine, messageSendingWrapperFactory);

	Mock<ICusEntryLineCustomsMessageWrapper> entryLineWrapperMock;
	CustomsMessageSending.IT.IMessageSendingWrapperFactory messageSendingWrapperFactory;
}
