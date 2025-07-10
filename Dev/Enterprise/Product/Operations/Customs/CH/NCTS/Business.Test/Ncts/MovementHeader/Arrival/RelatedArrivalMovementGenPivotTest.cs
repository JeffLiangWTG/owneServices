using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(RelatedArrivalMovementGenPivot))]
class RelatedArrivalMovementGenPivotTest : EnterpriseBusinessObjectTestCase
{
	public void TestDefaultValues()
	{
		CombineAssertions(() =>
		{
			AssertEquals(GenPivotTypeDecider.Types.NctsRelatedArrivalGenPivot, RelatedArrivalMovementGenPivot.XX_RelationType);
			AssertEquals(CusInBondMoveHeaderSchema.Constants.Prefix, RelatedArrivalMovementGenPivot.XX_Relation1TableCode);
			AssertEquals(CusInBondMoveHeaderSchema.Constants.Prefix, RelatedArrivalMovementGenPivot.XX_Relation2TableCode);
		});
	}

	public void TestChildMovement()
	{
		var childMovement = Factory.New<NctsArrivalMovementHeader>();
		childMovement.MultipleMRNIndicator = false;
		RelatedArrivalMovementGenPivot.Relation2Object = childMovement;

		AssertSame(childMovement, RelatedArrivalMovementGenPivot.ChildMovement);
	}

	public void TestReportErrorOnSettingXX_Relation1ID() => CombineAssertions(() =>
	{
		var arrivalMovementHeaderWithouttMultipleMr = CreateNctsArrivalMovementHeader();
		arrivalMovementHeaderWithouttMultipleMr.MultipleMRNIndicator = false;
		var childMovementWithoutMultipleMrn = CreateNctsArrivalMovementHeader();
		childMovementWithoutMultipleMrn.MultipleMRNIndicator = false;
		arrivalMovementHeaderWithouttMultipleMr.RelatedArrivalMovements.AddPivotFor(childMovementWithoutMultipleMrn);

		AssertEquals("It's not possible to set parent movement with MultipleMRNIndicator=FALSE", "RelatedArrivalMovementGenPivot.XX_Relation1ID.MultipleMRNIndicator", ErrorReporter.LastKeyReported);
		ErrorReporter.Clear();

		var childMovementWithMultipleMrn = CreateNctsArrivalMovementHeader();
		childMovementWithMultipleMrn.MultipleMRNIndicator = true;
		childMovementWithMultipleMrn.RelatedArrivalMovements.AddPivotFor(childMovementWithoutMultipleMrn);
		AssertEquals("It's possible to add set parent movement with MultipleMRNIndicator=TRUE", ZString.Empty, ErrorReporter.LastKeyReported);
		ErrorReporter.Clear();
	});

	public void TestReportErrorOnSettingXX_Relation2ID() => CombineAssertions(() =>
	{
		var arrivalMovementHeader = CreateNctsArrivalMovementHeader();
		var childMovementWithMultipleMrn = CreateNctsArrivalMovementHeader();
		arrivalMovementHeader.RelatedArrivalMovements.AddPivotFor(childMovementWithMultipleMrn);

		AssertEquals("It's not possible to add a child movement with MultipleMRNIndicator=TRUE", "RelatedArrivalMovementGenPivot.XX_Relation2ID.MultipleMRNIndicator", ErrorReporter.LastKeyReported);
		ErrorReporter.Clear();

		var childMovementWithoutMultipleMrn = CreateNctsArrivalMovementHeader();
		childMovementWithoutMultipleMrn.MultipleMRNIndicator = false;
		arrivalMovementHeader.RelatedArrivalMovements.AddPivotFor(childMovementWithoutMultipleMrn);
		AssertEquals("It's possible to add a child movement with MultipleMRNIndicator=FALSE", ZString.Empty, ErrorReporter.LastKeyReported);
		ErrorReporter.Clear();
	});

	NctsArrivalMovementHeader CreateNctsArrivalMovementHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.ArrivalMovementHeader;
	}

	protected override BusinessObject GetNewBusinessObject() => RelatedArrivalMovementGenPivot;

	RelatedArrivalMovementGenPivot RelatedArrivalMovementGenPivot => relatedArrivalMovementGenPivot ?? (relatedArrivalMovementGenPivot = Factory.New<RelatedArrivalMovementGenPivot>());
	RelatedArrivalMovementGenPivot relatedArrivalMovementGenPivot;
}
