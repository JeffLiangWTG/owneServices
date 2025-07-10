using System;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class ConsignmentItemPreviousDocumentTest : ConsignmentItemPreviousDocumentBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When previous document is null", () => new ConsignmentItemPreviousDocument(null));
		AssertNoExceptionThrown("When previous document is not null", () => new ConsignmentItemPreviousDocument(previousDocument));
	}

	public override void TestDocumentType()
	{
		previousDocument.CSI_Code = ZString.Empty;
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IConsignmentItemPreviousDocument.DocumentType), wrapper.DocumentType);

		previousDocument.CSI_Code = "C152";
		wrapper = CreateWrapper();
		AssertEquals(nameof(IConsignmentItemPreviousDocument.DocumentType), "C152", wrapper.DocumentType);
	}

	public override void TestReferenceNumber()
	{
		previousDocument.CSI_ReferenceNumber = ZString.Empty;
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IConsignmentItemPreviousDocument.ReferenceNumber), wrapper.ReferenceNumber);

		previousDocument.CSI_ReferenceNumber = "A123";
		wrapper = CreateWrapper();
		AssertEquals(nameof(IConsignmentItemPreviousDocument.ReferenceNumber), "A123", wrapper.ReferenceNumber);
	}

	public override void TestNumberOfPackages()
	{
		previousDocument.CSI_PackQty = 0;
		previousDocument.CSI_PackType = ZString.Empty;
		var wrapper = CreateWrapper();
		AssertNull($"When PackQty = 0 and PackType empty, {nameof(IConsignmentItemPreviousDocument.NumberOfPackages)}", wrapper.NumberOfPackages);

		previousDocument.CSI_PackType = "CT";
		wrapper = CreateWrapper();
		AssertEquals($"When PackQty = 0 and PackType filled, {nameof(IConsignmentItemPreviousDocument.NumberOfPackages)}", 0, wrapper.NumberOfPackages);

		previousDocument.CSI_PackQty = 5;
		wrapper = CreateWrapper();
		AssertEquals($"When PackQty = 5 and PackType filled, {nameof(IConsignmentItemPreviousDocument.NumberOfPackages)}", 5, wrapper.NumberOfPackages);

		previousDocument.CSI_PackType = ZString.Empty;
		wrapper = CreateWrapper();
		AssertEquals($"When PackQty = 5 and PackType empty, {nameof(IConsignmentItemPreviousDocument.NumberOfPackages)}", 5, wrapper.NumberOfPackages);
	}

	public override void TestPackageType()
	{
		previousDocument.CSI_PackType = ZString.Empty;
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IConsignmentItemPreviousDocument.PackageType), wrapper.PackageType);

		previousDocument.CSI_PackType = "CT";
		wrapper = CreateWrapper();
		AssertEquals(nameof(IConsignmentItemPreviousDocument.PackageType), "CT", wrapper.PackageType);
	}

	public override void TestQuantity()
	{
		previousDocument.CSI_Quantity = 0;
		previousDocument.CSI_UnitOfQuantity = ZString.Empty;
		var wrapper = CreateWrapper();
		AssertNull($"When Quantity = 0 and UnitOfQuantity empty, {nameof(IConsignmentItemPreviousDocument.Quantity)}", wrapper.Quantity);

		previousDocument.CSI_UnitOfQuantity = "CT";
		wrapper = CreateWrapper();
		AssertEquals($"When Quantity = 0 and UnitOfQuantity filled, {nameof(IConsignmentItemPreviousDocument.Quantity)}", 0m, wrapper.Quantity);

		previousDocument.CSI_Quantity = 5;
		wrapper = CreateWrapper();
		AssertEquals($"When Quantity = 5 and UnitOfQuantity filled, {nameof(IConsignmentItemPreviousDocument.Quantity)}", 5m, wrapper.Quantity);

		previousDocument.CSI_UnitOfQuantity = ZString.Empty;
		wrapper = CreateWrapper();
		AssertEquals($"When Quantity = 5 and UnitOfQuantity empty, {nameof(IConsignmentItemPreviousDocument.Quantity)}", 5m, wrapper.Quantity);
	}

	public override void TestUnitOfQuantity()
	{
		previousDocument.CSI_UnitOfQuantity = ZString.Empty;
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IConsignmentItemPreviousDocument.UnitOfQuantity), wrapper.UnitOfQuantity);

		previousDocument.CSI_UnitOfQuantity = "KG";
		wrapper = CreateWrapper();
		AssertEquals(nameof(IConsignmentItemPreviousDocument.UnitOfQuantity), "KG", wrapper.UnitOfQuantity);
	}

	public override void TestGoodsItemIdentifier()
	{
		previousDocument.CSI_ItemNumber = 0;
		var wrapper = CreateWrapper();
		AssertNull(nameof(IConsignmentItemPreviousDocument.GoodsItemIdentifier), wrapper.GoodsItemIdentifier);

		previousDocument.CSI_ItemNumber = 4;
		wrapper = CreateWrapper();
		AssertEquals(nameof(IConsignmentItemPreviousDocument.GoodsItemIdentifier), 4, wrapper.GoodsItemIdentifier);
	}

	public override void TestComplementOfInformation()
	{
		previousDocument.CSI_ReferenceNumber2 = ZString.Empty;
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IConsignmentItemPreviousDocument.ComplementOfInformation), wrapper.ComplementOfInformation);

		previousDocument.CSI_ReferenceNumber2 = "B1121";
		wrapper = CreateWrapper();
		AssertEquals(nameof(IConsignmentItemPreviousDocument.ComplementOfInformation), "B1121", wrapper.ComplementOfInformation);
	}

	protected override IConsignmentItemPreviousDocument CreateWrapper()
	{
		return new ConsignmentItemPreviousDocument(previousDocument);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		previousDocument = header
			.Bills.AddNew()
			.GoodsItems.AddNew()
			.PreviousDocuments.AddNew();
	}

	NctsPreviousDocument previousDocument;
}
