using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(RelatedExportEntryHeaderGenPivot))]
class RelatedExportEntryHeaderGenPivotTest : EnterpriseBusinessObjectTestCase
{
	public void TestDefaultValues() => CombineAssertions(() =>
	{
		AssertEquals("XX_RelationType", GenPivotTypeDecider.Types.NctsRelatedExportGenPivot, RelatedExportEntryHeaderGenPivot.XX_RelationType);
		AssertEquals("XX_Relation1TableCode", CusInBondMoveHeaderSchema.Constants.Prefix, RelatedExportEntryHeaderGenPivot.XX_Relation1TableCode);
		AssertEquals("XX_Relation2TableCode", CusEntryHeaderSchema.Constants.Prefix, RelatedExportEntryHeaderGenPivot.XX_Relation2TableCode);
	});

	public void TestEntryHeader()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		RelatedExportEntryHeaderGenPivot.Relation2Object = entryHeader;

		AssertSame(entryHeader, RelatedExportEntryHeaderGenPivot.EntryHeader);
	}

	public void TestValidation() => AssertType<RelatedExportEntryHeaderGenPivotValidation>(Factory.New<RelatedExportEntryHeaderGenPivot>().Validation);

	public void TestCaptions() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions<RelatedExportEntryHeaderGenPivot>(nameof(RelatedExportEntryHeaderGenPivot.XX_Sequence), caption: "Sequence");
		CaptionTestHelper.AssertCaptions<RelatedExportEntryHeaderGenPivot>(nameof(RelatedExportEntryHeaderGenPivot.ShipmentType), caption: "Shipment Type");
		CaptionTestHelper.AssertCaptions<RelatedExportEntryHeaderGenPivot>(nameof(RelatedExportEntryHeaderGenPivot.EntryNumber), caption: "Entry Number");
		CaptionTestHelper.AssertCaptions<RelatedExportEntryHeaderGenPivot>(nameof(RelatedExportEntryHeaderGenPivot.JobNumber), caption: "Job Number");
		CaptionTestHelper.AssertCaptions<RelatedExportEntryHeaderGenPivot>(nameof(RelatedExportEntryHeaderGenPivot.ReferenceNumber), caption: "Reference Number");
		CaptionTestHelper.AssertCaptions<RelatedExportEntryHeaderGenPivot>(nameof(RelatedExportEntryHeaderGenPivot.EntryStatus), caption: "Entry Status");
		CaptionTestHelper.AssertCaptions<RelatedExportEntryHeaderGenPivot>(nameof(RelatedExportEntryHeaderGenPivot.EntryStatusDescription), caption: "Entry Status Description");
	});

	public void TestXX_Sequence() => CombineAssertions(() =>
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var collection = nctsHeader.MovementHeader.RelatedExportEntryHeaders;

		var relatedExportEntryHeaderGenPivot1 = collection.AddPivotFor(Factory.New<CusEntryHeader>());
		var relatedExportEntryHeaderGenPivot2 = collection.AddPivotFor(Factory.New<CusEntryHeader>());
		var relatedExportEntryHeaderGenPivot3 = collection.AddPivotFor(Factory.New<CusEntryHeader>());
		AssertEquals("Pivot1", 1, relatedExportEntryHeaderGenPivot1.XX_Sequence);
		AssertEquals("Pivot2", 2, relatedExportEntryHeaderGenPivot2.XX_Sequence);
		AssertEquals("Pivot3", 3, relatedExportEntryHeaderGenPivot3.XX_Sequence);

		relatedExportEntryHeaderGenPivot2.Delete();
		AssertEquals("Pivot1 after delete", 1, relatedExportEntryHeaderGenPivot1.XX_Sequence);
		AssertEquals("Pivot3 after delete", 2, relatedExportEntryHeaderGenPivot3.XX_Sequence);
	});

	public void TestShipmentType()
	{
		EntryHeader.Declaration.JE_MessageType = "ABC";
		AssertEquals("ABC", RelatedExportEntryHeaderGenPivot.ShipmentType);
	}

	public void TestEntryNumber()
	{
		EntryHeader.EntryNumber = "00CH123";
		AssertEquals("00CH123", RelatedExportEntryHeaderGenPivot.EntryNumber);
	}

	public void TestJobNumber()
	{
		EntryHeader.Declaration.JE_DeclarationReference = "B123";
		AssertEquals("B123", RelatedExportEntryHeaderGenPivot.JobNumber);
	}

	public void TestReferenceNumber()
	{
		EntryHeader.CH_BGMReference = "BGM123";
		AssertEquals("BGM123", RelatedExportEntryHeaderGenPivot.ReferenceNumber);
	}

	public void TestEntryStatus()
	{
		EntryHeader.CH_EntryStatus = "S00";
		AssertEquals("S00", RelatedExportEntryHeaderGenPivot.EntryStatus);
	}

	public void TestEntryStatusDescription()
	{
		RefCusCodeTestHelper.CreateCustomsStatusCodeListAndFrenchLanguage(Factory);

		EntryHeader.CH_EntryStatus = RefCusCodeTestHelper.EntryStatusCode;
		AssertEquals(RefCusCodeTestHelper.EntryStatusDescription, RelatedExportEntryHeaderGenPivot.EntryStatusDescription);
	}

	public void TestHumanReadableName()
	{
		AssertEquals("Export Declaration", RelatedExportEntryHeaderGenPivot.HumanReadableName);
	}

	protected override BusinessObject GetNewBusinessObject() => RelatedExportEntryHeaderGenPivot;

	RelatedExportEntryHeaderGenPivot RelatedExportEntryHeaderGenPivot => relatedExportEntryHeaderGenPivot ?? (relatedExportEntryHeaderGenPivot = Factory.New<RelatedExportEntryHeaderGenPivot>());
	RelatedExportEntryHeaderGenPivot relatedExportEntryHeaderGenPivot;

	CusEntryHeader EntryHeader => entryHeader ?? (entryHeader = CreateEntryHeader());
	CusEntryHeader entryHeader;

	CusEntryHeader CreateEntryHeader()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		RelatedExportEntryHeaderGenPivot.Relation2Object = entryHeader;
		return entryHeader;
	}
}
