using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.AE.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.Testing;

[TestedType(typeof(ShipmentDetailsLayouts))]
sealed class ShipmentDetailsLayoutsTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ShipmentDetailsLayoutBuilder();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.OwnersReferenceTextBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.TypeOfGoodsDropEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.TotalNoOfPacksCalcDropEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.MarksAndNumbersNotePopupEdit, ControlWidthClass.Auto);
			yield return (ShipmentDetailsControlBag.Instance.OperationalStatusDropEdit, ControlWidthClass.Auto);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (Customs.GUI.ShipmentDetailsControlBag.Instance.TotalNoOfPiecesCalcEdit, ControlWidthClass.LongNoCaption);
		}
	}

	public void TestAddControlBehaviour()
	{
		var declaration = Factory.New<JobDeclaration>();
		using var form = new ZForm(declaration);
		using var control = new AEJobDeclarationUserControl();
		var commonBag = Customs.GUI.ShipmentDetailsControlBag.Instance;

		form.Controls.Add(control);
		form.Show();

		var marks = (Freight.GUI.ZStmNotePopupEditWithBindableText)control.ShipmentDetailsLayoutPanel.FindSingle<Control>(commonBag.MarksAndNumbersNotePopupEdit.ControlName);
		var behaviours = LayoutForTesting.GetControlBehaviours(commonBag.MarksAndNumbersNotePopupEdit);
		CombineAssertions(() =>
		{
			AssertEquals("Number of Behaviours", 1, behaviours.Count);
			AssertEquals("Expected maximum note length for MarksAndNumbersNotePopupEdit is 350", 350, marks.MaximumNoteLength);
		});
	}
}
