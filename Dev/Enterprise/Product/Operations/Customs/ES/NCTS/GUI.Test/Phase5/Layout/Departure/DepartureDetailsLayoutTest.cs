using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing;

[TestedType(typeof(DepartureDetailsLayout))]
sealed class DepartureDetailsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 2;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new DepartureDetailsLayoutBuilder<NctsHeader>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			var euBag = EU.NCTS.GUI.DepartureDetailsControlBag.Instance;
			yield return (euBag.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
			yield return (euBag.CustomerReferenceNumberTextBox, ControlWidthClass.Long);
			yield return (euBag.DeclarationTypeDropEdit, ControlWidthClass.Long);
			yield return (euBag.AdditionalDeclarationTypeDropEdit, ControlWidthClass.Long);
			yield return (euBag.SimplifiedProcedureAndReducedDataSetUserControl, ControlWidthClass.Long);
			yield return (euBag.DateLimitDateEdit, ControlWidthClass.Auto);
			yield return (euBag.SecurityDropEdit, ControlWidthClass.Long);
			yield return (euBag.TirCarnetNumberTextBox, ControlWidthClass.Long);
			yield return (euBag.CountryOfDispatchDropEdit, ControlWidthClass.Long);
			yield return (euBag.CountryOfDestinationDropEdit, ControlWidthClass.Long);
			yield return (euBag.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			yield return (DepartureDetailsControlBag.Instance.DepartureGoodsLocationZCodeFindBox, ControlWidthClass.Long);
			yield return (DepartureDetailsControlBag.Instance.TNNDocumentTypeDropEdit, ControlWidthClass.Long);
			yield return (euBag.CommercialReferenceNumberTextBox, ControlWidthClass.Long);
		}
	}

	public void TestTNNDocumentTypeDropEditVisibility()
	{
		var tnnDocumentTypeDropEdit = DepartureDetailsControlBag.Instance.TNNDocumentTypeDropEdit;
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		CombineAssertions(() =>
		{
			header.MovementHeader.BM_Phase = ZString.Empty;
			AssertEquals("TNNDocumentTypeDropEdit not visible when BM_Phase not TNN", false, LayoutForTesting.IsVisible(tnnDocumentTypeDropEdit, header));

			header.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
			AssertEquals("TNNDocumentTypeDropEdit visible when BM_Phase is TNN", true, LayoutForTesting.IsVisible(tnnDocumentTypeDropEdit, header));
		});
	}
}
