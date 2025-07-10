using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(RelatedExportEntryHeaderGenPivotCollection))]
class RelatedExportEntryHeaderGenPivotCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Departure);
		return new RelatedExportEntryHeaderGenPivotCollection(nctsHeader.MovementHeader);
	}
}
