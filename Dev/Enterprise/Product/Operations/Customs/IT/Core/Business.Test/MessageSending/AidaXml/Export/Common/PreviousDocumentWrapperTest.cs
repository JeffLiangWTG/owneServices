using System;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class PreviousDocumentWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new PreviousDocumentWrapper(null));
	}

	public void TestLineNo()
	{
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertNull(nameof(IPreviousDocument.LineNo), previousDocumentWrapper.LineNo);

		previousDocument.CSI_ItemNumber = 5;
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.LineNo), 5, previousDocumentWrapper.LineNo);
	}

	public void TestNumberOfPackages_WhenPackageTypeIsEmpty()
	{
		previousDocument.CSI_PackType = "";

		previousDocument.CSI_PackQty = 0;
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertNull(nameof(IPreviousDocument.NumberOfPackages), previousDocumentWrapper.NumberOfPackages);

		previousDocument.CSI_PackQty = 10;
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.NumberOfPackages), 10, previousDocumentWrapper.NumberOfPackages);
	}

	public void TestNumberOfPackages_WhenPackageTypeIsWhiteSpace()
	{
		previousDocument.CSI_PackType = " ";

		previousDocument.CSI_PackQty = 0;
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertNull(nameof(IPreviousDocument.NumberOfPackages), previousDocumentWrapper.NumberOfPackages);

		previousDocument.CSI_PackQty = 10;
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.NumberOfPackages), 10, previousDocumentWrapper.NumberOfPackages);
	}

	public void TestNumberOfPackages_WhenPackageTypeIsFilled()
	{
		previousDocument.CSI_PackType = "AA";

		previousDocument.CSI_PackQty = 0;
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.NumberOfPackages), 0, previousDocumentWrapper.NumberOfPackages);

		previousDocument.CSI_PackQty = 10;
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.NumberOfPackages), 10, previousDocumentWrapper.NumberOfPackages);
	}

	public void TestPackageType()
	{
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.PackageType), "", previousDocumentWrapper.PackageType);

		previousDocument.CSI_PackType = "CT";
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.NumberOfPackages), "CT", previousDocumentWrapper.PackageType);
	}

	public void TestQuantity_WhenUnitOfQuantityIsEmpty()
	{
		previousDocument.CSI_UnitOfQuantity = "";

		previousDocument.CSI_Quantity = 0;
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertNull(nameof(IPreviousDocument.Quantity), previousDocumentWrapper.Quantity);

		previousDocument.CSI_Quantity = 99;
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.Quantity), 99m, previousDocumentWrapper.Quantity);
	}

	public void TestQuantity_WhenUnitOfQuantityIsWhiteSpace()
	{
		previousDocument.CSI_UnitOfQuantity = " ";

		previousDocument.CSI_Quantity = 0;
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertNull(nameof(IPreviousDocument.Quantity), previousDocumentWrapper.Quantity);

		previousDocument.CSI_Quantity = 99;
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.Quantity), 99m, previousDocumentWrapper.Quantity);
	}

	public void TestQuantity_WhenUnitOfQuantityIsFilled()
	{
		previousDocument.CSI_UnitOfQuantity = "KG";

		previousDocument.CSI_Quantity = 0;
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.Quantity), 0m, previousDocumentWrapper.Quantity);

		previousDocument.CSI_Quantity = 99;
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.Quantity), 99m, previousDocumentWrapper.Quantity);
	}

	public void TestUnitOfQuantity()
	{
		previousDocument.CSI_UnitOfQuantity = "";
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.UnitOfQuantity), "", previousDocumentWrapper.UnitOfQuantity);

		previousDocument.CSI_UnitOfQuantity = "AA";
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.UnitOfQuantity), "AA", previousDocumentWrapper.UnitOfQuantity);
	}

	public void TestDocumentType()
	{
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.DocumentType), "", previousDocumentWrapper.DocumentType);

		previousDocument.CSI_Code = "T";
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.DocumentType), "T", previousDocumentWrapper.DocumentType);
	}

	public void TestReferenceNumber()
	{
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.ReferenceNumber), "", previousDocumentWrapper.ReferenceNumber);

		previousDocument.CSI_ReferenceNumber = "ABCDE";
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.ReferenceNumber), "ABCDE", previousDocumentWrapper.ReferenceNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();
		previousDocument = Factory.New<PreviousDocument>();
	}

	PreviousDocument previousDocument;

	IPreviousDocument GetNewPreviousDocumentWrapper() => new PreviousDocumentWrapper(previousDocument);
}
