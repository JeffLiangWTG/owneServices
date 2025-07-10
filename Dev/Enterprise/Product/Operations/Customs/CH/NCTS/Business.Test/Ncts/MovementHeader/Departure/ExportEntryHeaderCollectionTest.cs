using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.CH.Business.CusEntryHeader;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(ExportEntryHeaderCollection))]
public class ExportEntryHeaderCollectionTest : ModuleEntryHeaderCollectionTest<ExportEntryHeaderCollection>
{
	protected override ExportEntryHeaderCollection GetCollectionToTest()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Departure);
		return new ExportEntryHeaderCollection(nctsHeader.MovementHeader);
	}

	public void TestFilterBusinessObjectDefaults() => CombineAssertions(() =>
	{
		var filterDefaultsCollection = GetCollectionToTest().FilterBusinessObjectDefaults;
		AssertEquals("Count", 3, filterDefaultsCollection.Count);

		assertFilter(ModuleEntryHeaderCollection.FilterConstants.EntryNumber + ":ComparisonOperator", ModuleTextFilter.ComparisonConstants.IsNotBlank);
		assertFilter("Shipment Type:Property:1", CHJobMessageTypeList.Codes.Export);
		assertFilter("Shipment Type:Property:2", CHJobMessageTypeList.Codes.ExportDeclarationActivation);

		void assertFilter(string filterKey, string expectedValue)
		{
			var filterDefault = filterDefaultsCollection[filterKey];
			AssertEquals($@"""{filterKey}"": value", expectedValue, filterDefault.Value);
			AssertEquals($@"""{filterKey}"": IsRemovable", false, filterDefault.IsRemovable);
		}
	});

	public override void TestEntryHeaderCollection() => CombineAssertions(() =>
	{
		var entryHeader1 = CreateEntryHeader();
		var entryHeader2 = CreateEntryHeader(shipmentType: CHJobMessageTypeList.Codes.ExportDeclarationActivation);
		var entryHeader3 = CreateEntryHeader();
		var entryHeader4 = CreateEntryHeader();

		var otherNctsHeader = Factory.New<NctsHeader>();
		otherNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		otherNctsHeader.MovementHeader.RelatedExportEntryHeaders.AddPivotFor(entryHeader4);

		Factory.Save();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.MovementHeader.RelatedExportEntryHeaders.AddPivotFor(entryHeader3);
		var collection = nctsHeader.MovementHeader.Lookups.ExportEntryHeaderCollection;

		AssertEquals("ShipmentType=EXP", true, collection.Contains(entryHeader1));
		AssertEquals("ShipmentType=EDA", true, collection.Contains(entryHeader2));
		AssertEquals("Already added", false, collection.Contains(entryHeader3));
		AssertEquals("Already attached to other movement", false, collection.Contains(entryHeader4));

		CusEntryHeader CreateEntryHeader(string shipmentType = CHJobMessageTypeList.Codes.Export)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = shipmentType;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter(ZGuid.NewZGuid().ToString());
			return entryHeader;
		}
	});
}
