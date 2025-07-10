using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using CusEntryInstruction = Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(EntryInstructionDetailsBasicUserControlLayout))]
sealed class EntryInstructionBasicDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestT2LFieldsVisibility() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		using var form = new ZForm(declaration);
		using var control = new EntryInstructionDetailsUserControl();
		control.JobDeclaration = declaration;
		control.InitializeGridLayout();

		form.Controls.Add(control);
		form.Show();

		var instruction = declaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;

		void AssertControl<T>(string message, bool expectedVisibility, string controlName) where T : Control
		{
			var field = control.FindSingle<T>(controlName);
			AssertEquals(message, expectedVisibility, field.Visible);
		}

		AssertControl<ZLabel>("Not Visible for IMP and T2C", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.RequestLabel));
		AssertControl<ZDropEdit>("Not Visible for IMP and T2C", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.RequestTypeDropEdit));
		AssertControl<ZCheckBox>("Not Visible for IMP and T2C", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.NationalCheckBox));
		AssertControl<ZCalcEdit>("Not Visible for IMP and T2C", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.NumberOfDaysCalcEdit));
		AssertControl<ZTextBox>("Not Visible for IMP and T2C", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.JustificationTextBox));
		AssertControl<ZDropEdit>("Not Visible for IMP and T2C", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.IndirectTypeDropEdit));

		instruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		AssertControl<ZLabel>("Visible for IMP and T2L", expectedVisibility: true, nameof(EntryInstructionDetailBasicUserControl.RequestLabel));
		AssertControl<ZDropEdit>("Visible for IMP and T2L", expectedVisibility: true, nameof(EntryInstructionDetailBasicUserControl.RequestTypeDropEdit));
		AssertControl<ZCheckBox>("Not Visible for IMP and T2L", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.NationalCheckBox));
		AssertControl<ZCalcEdit>("Visible for IMP and T2L", expectedVisibility: true, nameof(EntryInstructionDetailBasicUserControl.NumberOfDaysCalcEdit));
		AssertControl<ZTextBox>("Visible for IMP and T2L", expectedVisibility: true, nameof(EntryInstructionDetailBasicUserControl.JustificationTextBox));
		AssertControl<ZDropEdit>("Visible for IMP and T2L", expectedVisibility: true, nameof(EntryInstructionDetailBasicUserControl.IndirectTypeDropEdit));

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		instruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
		AssertControl<ZLabel>("Not Visible for EXP and T2C", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.RequestLabel));
		AssertControl<ZDropEdit>("Not Visible for EXP and T2C", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.RequestTypeDropEdit));
		AssertControl<ZCheckBox>("Not Visible for EXP and T2C", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.NationalCheckBox));
		AssertControl<ZCalcEdit>("Not Visible for EXP and T2C", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.NumberOfDaysCalcEdit));
		AssertControl<ZTextBox>("Not Visible for EXP and T2C", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.JustificationTextBox));
		AssertControl<ZDropEdit>("Not Visible for EXP and T2C", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.IndirectTypeDropEdit));

		instruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		AssertControl<ZLabel>("Visible for EXP and T2L", expectedVisibility: true, nameof(EntryInstructionDetailBasicUserControl.RequestLabel));
		AssertControl<ZDropEdit>("Visible for EXP and T2L", expectedVisibility: true, nameof(EntryInstructionDetailBasicUserControl.RequestTypeDropEdit));
		AssertControl<ZCheckBox>("Visible for EXP and T2L", expectedVisibility: true, nameof(EntryInstructionDetailBasicUserControl.NationalCheckBox));
		AssertControl<ZCalcEdit>("Visible for EXP and T2L", expectedVisibility: true, nameof(EntryInstructionDetailBasicUserControl.NumberOfDaysCalcEdit));
		AssertControl<ZTextBox>("Visible for EXP and T2L", expectedVisibility: true, nameof(EntryInstructionDetailBasicUserControl.JustificationTextBox));
		AssertControl<ZDropEdit>("Not Visible for EXP and T2L", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.IndirectTypeDropEdit));
	});

	public void TestUCC6FieldsVisibility() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var instruction = declaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_Style = IMPDeclarationTypeList.Codes.IM;
		instruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		using (var form = new ZForm(declaration))
		using (var control = new EntryInstructionDetailsUserControl())
		{
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();
			form.Controls.Add(control);
			form.Show();

			AssertControl<ZCheckBox>(control, "ActivateByOperatorCheckBox Visible for IMP and FUNCS ImportMessageVersionUCC6 enabled, different to T2C and T2L substyle and H2 style", expectedVisibility: true, nameof(EntryInstructionDetailBasicUserControl.ActivateByOperatorCheckBox));
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertControl<ZCheckBox>(control, "ActivateByOperatorCheckBox not Visible for IMP and FUNCS ImportMessageVersionUCC6 enabled, equal to T2C substyle and different to H2 style", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.ActivateByOperatorCheckBox));
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			AssertControl<ZCheckBox>(control, "ActivateByOperatorCheckBox not Visible for IMP and FUNCS ImportMessageVersionUCC6 enabled, equal to T2L substyle and different to H2 style", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.ActivateByOperatorCheckBox));
			instruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertControl<ZCheckBox>(control, "ActivateByOperatorCheckBox not Visible for IMP and FUNCS ImportMessageVersionUCC6 enabled, different to T2C and T2L substyle and equal to H2 style", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.ActivateByOperatorCheckBox));
			AssertControl<ZCheckBox>(control, "IncludeRoutingSecurityDataCheckBox not Visible for IMP and FUNCS ImportMessageVersionUCC6 enabled", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.IncludeRoutingSecurityDataCheckBox));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertControl<ZCheckBox>(control, "ActivateByOperatorCheckBox not Visible for EXP and FUNCS ImportMessageVersionUCC6 enabled", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.ActivateByOperatorCheckBox));
			AssertControl<ZCheckBox>(control, "IncludeRoutingSecurityDataCheckBox Visible for EXP and FUNCS ImportMessageVersionUCC6 enabled", expectedVisibility: true, nameof(EntryInstructionDetailBasicUserControl.IncludeRoutingSecurityDataCheckBox));
		}

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		using (var form = new ZForm(declaration))
		using (var control = new EntryInstructionDetailsUserControl())
		{
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();
			form.Controls.Add(control);
			form.Show();

			AssertControl<ZCheckBox>(control, "ActivateByOperatorCheckBox not Visible for EXP and FUNCS ImportMessageVersionUCC6 disabled", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.ActivateByOperatorCheckBox));
			AssertControl<ZCheckBox>(control, "IncludeRoutingSecurityDataCheckBox Visible for EXP and FUNCS ImportMessageVersionUCC6 disabled", expectedVisibility: true, nameof(EntryInstructionDetailBasicUserControl.IncludeRoutingSecurityDataCheckBox));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertControl<ZCheckBox>(control, "ActivateByOperatorCheckBox not Visible for IMP and FUNCS ImportMessageVersionUCC6 disabled", expectedVisibility: false, nameof(EntryInstructionDetailBasicUserControl.ActivateByOperatorCheckBox));
			AssertControl<ZCheckBox>(control, "IncludeRoutingSecurityDataCheckBox Visible for IMP and FUNCS ImportMessageVersionUCC6 disabled", expectedVisibility: true, nameof(EntryInstructionDetailBasicUserControl.IncludeRoutingSecurityDataCheckBox));
		}

		void AssertControl<T>(ZUserControl control, string message, bool expectedVisibility, string controlName) where T : Control
		{
			var field = control.FindSingle<T>(controlName);
			AssertEquals(message, expectedVisibility, field.Visible);
		}
	});

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	protected override int ControlBagCount => 3;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.GUI.EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EntryInstructionDetailsBasicUserControlBag.Instance.ActivateByOperatorCheckBox, ControlWidthClass.Long);
			yield return (EntryInstructionDetailsBasicUserControlBag.Instance.IncludeRoutingSecurityDataCheckBox, ControlWidthClass.Long);
			yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.OtherPartiesSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (EU.GUI.EntryInstructionBasicDetailsControlBag.Instance.ToWarehouseLabel, ControlWidthClass.LongNoCaption);
			yield return (EU.GUI.EntryInstructionBasicDetailsControlBag.Instance.ToWarehouseUserControl, ControlWidthClass.LongNoCaption);
			yield return (EU.GUI.EntryInstructionBasicDetailsControlBag.Instance.FromWarehouseLabel, ControlWidthClass.LongNoCaption);
			yield return (EU.GUI.EntryInstructionBasicDetailsControlBag.Instance.FromWarehouseUserControl, ControlWidthClass.LongNoCaption);
			yield return (EntryInstructionDetailsBasicUserControlBag.Instance.BondHolderOrganisationControl, ControlWidthClass.LongNoCaption);
			yield return (EntryInstructionDetailsBasicUserControlBag.Instance.NewOwnerOrganisationControl, ControlWidthClass.LongNoCaption);
			yield return (EntryInstructionDetailsBasicUserControlBag.Instance.RequestLabel, ControlWidthClass.LongNoCaption);
			yield return (EntryInstructionDetailsBasicUserControlBag.Instance.NumberOfDaysCalcEdit, ControlWidthClass.Medium);
			yield return (EntryInstructionDetailsBasicUserControlBag.Instance.JustificationTextBox, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (EntryInstructionDetailsBasicUserControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Long);
			yield return (EntryInstructionDetailsBasicUserControlBag.Instance.RemoverOrganisationControl, ControlWidthClass.Long);
			yield return (EntryInstructionDetailsBasicUserControlBag.Instance.RequestTypeDropEdit, ControlWidthClass.Long);
			yield return (EntryInstructionDetailsBasicUserControlBag.Instance.NationalCheckBox, ControlWidthClass.Long);
			yield return (EntryInstructionDetailsBasicUserControlBag.Instance.IndirectTypeDropEdit, ControlWidthClass.Long);
		}
	}
}
