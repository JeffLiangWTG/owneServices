using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(EntryDetailsLayouts))]
sealed class EntryDetailsLayoutsTest : LayoutsAbstractTest
{
	public void TestCircuitCanVisibility() => CombineAssertions(() =>
	{
		var helper = new ESUniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "61", "Test 61");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		{
			AssertCircuitCanTextBoxVisibility(true, "CircuitCan is visible when ZG_DestinationState IsCanaryIsland, Import and not UCC6", declaration);

			declaration.ZG_DestinationState = "zz";
			AssertCircuitCanTextBoxVisibility(false, "CircuitCan is not visible when ZG_DestinationState no IsCanaryIsland, Import and not UCC6", declaration);

			declaration.JE_CustomsOffice = "ES003861";
			AssertCircuitCanTextBoxVisibility(false, "CircuitCan is not visible when JE_CustomsOffice = ES003861, Import and not UCC6", declaration);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertCircuitCanTextBoxVisibility(false, "CircuitCan is not visible when ZG_DestinationState no IsCanaryIsland, Export and not UCC6", declaration);

			AssertCircuitCanTextBoxVisibility(false, "CircuitCan is not visible when JE_CustomsOffice = ES003861, Export and not UCC6", declaration);
		}

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		{
			declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
			AssertCircuitCanTextBoxVisibility(false, "CircuitCan is not visible when ZG_DestinationState IsCanaryIsland, Export and UCC6", declaration);

			AssertCircuitCanTextBoxVisibility(false, "CircuitCan is not visible when JE_CustomsOffice = ES003861, Export and UCC6", declaration);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertCircuitCanTextBoxVisibility(true, "CircuitCan is visible when JE_CustomsOffice = ES003861, Import and UCC6", declaration);

			declaration.JE_CustomsOffice = ZString.Empty;
			AssertCircuitCanTextBoxVisibility(false, "CircuitCan is not visible when JE_CustomsOffice = Empty, Import and UCC6", declaration);

			AssertCircuitCanTextBoxVisibility(false, "CircuitCan is not visible when ZG_DestinationState IsCanaryIsland, Import and UCC6", declaration);

			declaration.JE_CustomsOffice = "ES003541";
			AssertCircuitCanTextBoxVisibility(true, "CircuitCan is visible when JE_CustomsOffice = ES003541, Import and UCC6", declaration);

			declaration.JE_CustomsOffice = "ES003712";
			AssertCircuitCanTextBoxVisibility(false, "CircuitCan is not visible when JE_CustomsOffice = ES003712, Import and UCC6", declaration);

			declaration.JE_CustomsOffice = "ES009998";
			AssertCircuitCanTextBoxVisibility(true, "CircuitCan is visible when JE_CustomsOffice = ES009998, Import and UCC6", declaration);
		}
	});

	void AssertCircuitCanTextBoxVisibility(ZBool expectedResult, ZString testMessage, JobDeclaration declaration)
	{
		using (var form = new ZForm(declaration))
		using (var control = new MessageUserControl())
		{
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();

			form.Controls.Add(control);
			form.Show();

			var circuitCanTextBox = control.FindSingle<ZTextBox>("CircuitCanTextBox");
			AssertEquals(testMessage, expectedResult, circuitCanTextBox.Visible);
		}
	}

	protected override int ControlBagCount => 2;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonEntryDetailsLayoutBuilder<JobDeclaration>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonEntryDetailsControlBag.Instance.TotalsLabel, ControlWidthClass.Auto);
			yield return (CommonEntryDetailsControlBag.Instance.NoPacksCalcEdit, ControlWidthClass.Auto);
			yield return (CommonEntryDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonEntryDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonEntryDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (EntryDetailsLayoutsControlBag.Instance.InvoiceAmountCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonEntryDetailsControlBag.Instance.DutyCalcEdit, ControlWidthClass.Auto);
			yield return (CommonEntryDetailsControlBag.Instance.VatCalcEdit, ControlWidthClass.Auto);
			yield return (EntryDetailsLayoutsControlBag.Instance.VATDeferredCalcEdit, ControlWidthClass.Auto);
			yield return (CommonEntryDetailsControlBag.Instance.EntryLinesCountCalcEdit, ControlWidthClass.Auto);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (CommonEntryDetailsControlBag.Instance.CustomsLabel, ControlWidthClass.Auto);
			yield return (CommonEntryDetailsControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Auto);
			yield return (CommonEntryDetailsControlBag.Instance.SubmittedDateDateEdit, ControlWidthClass.Auto);
			yield return (CommonEntryDetailsControlBag.Instance.MRNTextBox, ControlWidthClass.Auto);
			yield return (CommonEntryDetailsControlBag.Instance.AcceptanceDateDateEdit, ControlWidthClass.Auto);
			yield return (EntryDetailsLayoutsControlBag.Instance.CircuitTextBox, ControlWidthClass.Auto);
			yield return (EntryDetailsLayoutsControlBag.Instance.CircuitCanTextBox, ControlWidthClass.Auto);
			yield return (EntryDetailsLayoutsControlBag.Instance.CSVClearanceTextBox, ControlWidthClass.Auto);
			yield return (CommonEntryDetailsControlBag.Instance.ReleaseDateDateEdit, ControlWidthClass.Auto);
			yield return (CommonEntryDetailsControlBag.Instance.EntryStatusDropEdit, ControlWidthClass.Auto);
		}
	}
}
