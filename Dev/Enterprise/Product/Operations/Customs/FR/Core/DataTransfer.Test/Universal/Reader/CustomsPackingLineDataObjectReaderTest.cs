using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.FR.DataTransfer.Universal.Testing;

sealed class CustomsPackingLineDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
{
	#region TestReadIntoBusinessObject_CW_Properties

	public void TestReadIntoBusinessObject_CW_Properties_ByDefault()
	{
		AssertReadIntoBusinessObject_CW_Properties(reader1, 12, "2A");

		AssertReadIntoBusinessObject_CW_Properties(reader2, 20, "2B");
	}

	public void TestReadIntoBusinessObject_CW_Properties_SnapshotRevertingWithStrategyOverride()
	{
		using (jobDeclarationReader.TemporarilyMarkJobDeclarationReaderIsBeingUsedForSnapshotReverting(entryHeaderToRevert, SnapshotRevertingStrategy.Override, new TestErrorLogger()))
		{
			AssertReadIntoBusinessObject_CW_Properties(reader1, 12, "2A");

			AssertReadIntoBusinessObject_CW_Properties(reader2, 20, "2B");
		}
	}

	public void TestReadIntoBusinessObject_CW_Properties_SnapshotRevertingWithStrategySkip()
	{
		using (jobDeclarationReader.TemporarilyMarkJobDeclarationReaderIsBeingUsedForSnapshotReverting(entryHeaderToRevert, SnapshotRevertingStrategy.Skip, new TestErrorLogger()))
		{
			AssertReadIntoBusinessObject_CW_Properties(reader1, shouldBePopulated: false);

			AssertReadIntoBusinessObject_CW_Properties(reader2, 20, "2B");
		}
	}

	void AssertReadIntoBusinessObject_CW_Properties(CustomsPackingLineDataObjectReader_ForTest reader, int expectedQty = 0, string expectedPackType = "", bool shouldBePopulated = true)
	{
		declaration.PrimaryHouseBill.PackingGroups.RemoveAndDeleteAll();
		AssertEquals("Prerequisite: No packing groups.", 0, declaration.PrimaryHouseBill.PackingGroups.Count);
		reader.ReadIntoBusinessObject();
		declaration.PrimaryHouseBill.PackingGroups.Reload(true);
		if (shouldBePopulated)
		{
			declaration.PrimaryHouseBill.PackingGroups[0].Packages.Reload(true);
			CombineAssertions(() =>
			{
				AssertEquals("Packing line populated into a packing group.", 1, declaration.PrimaryHouseBill.PackingGroups.Count);
				var package = declaration.PrimaryHouseBill.PackingGroups[0].Packages.Cast<Package>().Single();
				AssertEquals("Populated package.CW_PackQty:", expectedQty, package.CW_PackQty);
				AssertEquals("Populated package.CW_PackType:", expectedPackType, package.CW_PackType);
			});
		}
		else
		{
			AssertEquals("No packing group populated.", 0, declaration.PrimaryHouseBill.PackingGroups.Count);
		}
	}

	#endregion

	public void TestShouldRevertPackLine()
	{
		AssertEquals("The packing line for invoice lines distributed in different entries should be populated when it's not used for snapshot reverting.", true, reader1.ShouldRevertPackLine_Exposed(packingLine1));

		using (jobDeclarationReader.TemporarilyMarkJobDeclarationReaderIsBeingUsedForSnapshotReverting(entryHeaderToRevert, SnapshotRevertingStrategy.Override, new TestErrorLogger()))
		{
			AssertEquals("The packing line for invoice lines distributed in different entries should be populated when it's used for snapshot reverting and SnapshotRevertingStrategy is Override.", true, reader1.ShouldRevertPackLine_Exposed(packingLine1));
		}

		using (jobDeclarationReader.TemporarilyMarkJobDeclarationReaderIsBeingUsedForSnapshotReverting(entryHeaderToRevert, SnapshotRevertingStrategy.Skip, new TestErrorLogger()))
		{
			AssertEquals("The packing line for invoice lines distributed in different entries should NOT be populated when it's used for snapshot reverting and SnapshotRevertingStrategy is Skip.", false, reader1.ShouldRevertPackLine_Exposed(packingLine1));

			AssertEquals("The packing line for invoice lines distributed in a same entry should be populated when it's used for snapshot reverting and SnapshotRevertingStrategy is Skip.", true, reader1.ShouldRevertPackLine_Exposed(packingLine2));
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = JobDeclarationDataObjectReaderTest.GetJobDeclaration(Factory);
		jobDeclarationReader = new JobDeclarationDataObjectReader_ForTest(JobDeclarationDataObjectReaderTest.CreateShipment(Factory), new TestErrorLogger(), Factory);
		entryHeaderToRevert = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(entry => entry.InvoiceLines.Any(line => line.EntryInstruction.CEI_Style == "A"));
		var helper = new Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.France);

		packingLine1 = GetPackingLine1();
		packingLine2 = GetPackingLine2();
		reader1 = new CustomsPackingLineDataObjectReader_ForTest(packingLine1, new TestErrorLogger(), helper, declaration, declaration.PrimaryHouseBill, true, jobDeclarationReader);
		reader2 = new CustomsPackingLineDataObjectReader_ForTest(packingLine2, new TestErrorLogger(), helper, declaration, declaration.PrimaryHouseBill, true, jobDeclarationReader);
	}
	JobDeclarationDataObjectReader_ForTest jobDeclarationReader;
	CustomsPackingLineDataObjectReader_ForTest reader1, reader2;
	JobDeclaration declaration;
	CusEntryHeader entryHeaderToRevert;
	PackingLine packingLine1, packingLine2;

	internal static PackingLine GetPackingLine1()
	{
		var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackType = new PackageType() { Code = "2A", Description = "2A Desc." }, PackQty = 12 };
		packingLine1.SetPackedItemCollection(() => new List<PackedItem>
		{
			new PackedItem { CommercialInvoiceLineLink = 1, PackedQuantity = 3 },
			new PackedItem { CommercialInvoiceLineLink = 2, PackedQuantity = 9 }
		});
		return packingLine1;
	}

	internal static PackingLine GetPackingLine2()
	{
		var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackType = new PackageType() { Code = "2B", Description = "2B Desc." }, PackQty = 20 };
		packingLine2.SetPackedItemCollection(() => new List<PackedItem>
		{
			new PackedItem { CommercialInvoiceLineLink = 3, PackedQuantity = 13 },
			new PackedItem { CommercialInvoiceLineLink = 4, PackedQuantity = 7 }
		});
		return packingLine2;
	}
}

class CustomsPackingLineDataObjectReader_ForTest : CustomsPackingLineDataObjectReader
{
	public CustomsPackingLineDataObjectReader_ForTest(PackingLine packingLineDataObject, IXmlImportLogger logger, Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper helper, BaseJobDeclaration declaration, IColumnIndexer billRow, bool supportsParentPackage, JobDeclarationDataObjectReader parentDeclarationReader = null) : base(packingLineDataObject, logger, helper, declaration, billRow, supportsParentPackage, parentDeclarationReader)
	{
	}

	public bool ShouldRevertPackLine_Exposed(PackingLine packingLine) => base.ShouldRevertPackLine(packingLine);
}
