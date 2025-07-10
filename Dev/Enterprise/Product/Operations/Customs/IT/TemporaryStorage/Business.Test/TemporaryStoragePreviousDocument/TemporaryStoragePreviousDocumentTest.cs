using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStoragePreviousDocument))]
sealed class TemporaryStoragePreviousDocumentTest : CusSupportingInfoTest<TemporaryStoragePreviousDocument>
{
	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();
		previousDocument = bill.PreviousDocuments.AddNew();
	}

	TemporaryStoragePreviousDocument previousDocument;

	protected override IEnumerable<TemporaryStoragePreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var storageHeader = factory.NewWithValidTestData<TemporaryStorageHeader>();
		var bill = factory.New<TemporaryStorageBill>();
		bill.ABL_AMA = storageHeader.PK;
		var packedItem = bill.PackedItems.AddNew();
		yield return storageHeader.PreviousDocuments.AddNew();
		yield return bill.PreviousDocuments.AddNew();
		yield return packedItem.PreviousDocuments.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		return storageHeader.PreviousDocuments.AddNew();
	}

	public void TestCSI_ReferenceNumber()
	{
		AssertHasCustomAttribute<MaxLengthAttribute>(typeof(TemporaryStoragePreviousDocument), nameof(TemporaryStoragePreviousDocument.CSI_ReferenceNumber), false, x => x.MaxLength == 35);
	}

	public void TestCSI_PackQty()
	{
		AssertResourceStringData(previousDocument.CSI_PackQtyInfo, "Package Quantity", "Package Qty", "Package");
	}

	public void TestCSI_PackType()
	{
		CombineAssertions("CSI_PackType", () =>
		{
			AssertResourceStringData(previousDocument.CSI_PackTypeInfo, "Type of Packages", "Package Type", "Type");
			AssertHasCustomAttribute<MaxLengthAttribute>(typeof(TemporaryStoragePreviousDocument), nameof(TemporaryStoragePreviousDocument.CSI_PackType), false, x => x.MaxLength == 2);
		});
	}

	public void TestCSI_Quantity()
	{
		CombineAssertions("CSI_Quantity", () =>
		{
			AssertResourceStringData(previousDocument.CSI_QuantityInfo, "Quantity", null, "Qty");
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(TemporaryStoragePreviousDocument), nameof(TemporaryStoragePreviousDocument.CSI_Quantity), false, x => x.DecimalPlaces == 6);
			AssertHasCustomAttribute<DecimalPrecisionAttribute>(typeof(TemporaryStoragePreviousDocument), nameof(TemporaryStoragePreviousDocument.CSI_Quantity), false, x => x.DecimalPrecision == 16);
		});
	}

	public void TestCSI_UnitOfQuantity()
	{
		CombineAssertions("CSI_UnitOfQuantity", () =>
		{
			AssertResourceStringData(previousDocument.CSI_UnitOfQuantityInfo, "Unit of Quantity", "Quantity Unit", "UQ");
			AssertHasCustomAttribute<MaxLengthAttribute>(typeof(TemporaryStoragePreviousDocument), nameof(TemporaryStoragePreviousDocument.CSI_UnitOfQuantity), false, x => x.MaxLength == 4);
		});
	}

	public void TestValidationType()
	{
		AssertType<TemporaryStoragePreviousDocumentValidation>(previousDocument.Validation);
	}

	public void TestLookupsType()
	{
		AssertType<TemporaryStoragePreviousDocumentLookups>(previousDocument.Lookups);
	}

	void AssertResourceStringData(ZPropertyInfo info, string caption, string mediumCaption = null, string shortCaption = null, string fullDescription = null)
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(info);
		AssertEquals("Caption", caption, captionResourceString.Caption);

		if (mediumCaption is not null)
		{
			AssertEquals("MediumCaption", mediumCaption, captionResourceString.MediumCaption);
		}

		if (shortCaption is not null)
		{
			AssertEquals("ShortCaption", shortCaption, captionResourceString.ShortCaption);
		}

		if (fullDescription is not null)
		{
			AssertEquals("FullDescription", fullDescription, captionResourceString.FullDescription);
		}
	}
}
