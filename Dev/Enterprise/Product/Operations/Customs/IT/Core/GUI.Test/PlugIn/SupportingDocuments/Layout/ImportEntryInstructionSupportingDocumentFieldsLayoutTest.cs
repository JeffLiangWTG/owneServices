using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ImportEntryInstructionSupportingDocumentFieldsLayout))]
sealed class ImportEntryInstructionSupportingDocumentFieldsLayoutTest : LayoutsAbstractTest
{
	public void TestReferenceNumberCodeVisibility()
	{
		Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("2800", hasPermitAttribute: true), new TestSupportingDocumentCodeList("3200", hasPermitAttribute: false));
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var document = entryInstruction.SupportingDocuments.AddNew();

		using (var form = new ZForm(document))
		using (var userControl = new ImportEntryInstructionLayoutSupportingDocumentsFieldsControl(declaration))
		{
			form.Controls.Add(userControl);
			userControl.SetDataBinding(declaration, "");
			form.Show();

			var referenceNumberCodeFindBox = userControl.Controls.Find(nameof(EU.GUI.PlugIn.SupportingDocumentFieldsControlBag.ReferenceNumberCodeFindBox), true).FirstOrDefault();
			var referenceNumberTextBox = userControl.Controls.Find(nameof(EU.GUI.PlugIn.SupportingDocumentFieldsControlBag.ReferenceNumberTextBox), true).FirstOrDefault();

			document.CSI_Code = "2800";
			AssertEquals("ReferenceNumberCodeFindBox Visible", true, referenceNumberCodeFindBox.Visible);
			AssertEquals("ReferenceNumberTextBox Visible", false, referenceNumberTextBox.Visible);

			document.CSI_Code = "3200";
			AssertEquals("ReferenceNumberCodeFindBox Visible", false, referenceNumberCodeFindBox.Visible);
			AssertEquals("ReferenceNumberTextBox Visible", true, referenceNumberTextBox.Visible);
		}
	}

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	protected override int ControlBagCount => 2;

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.GUI.PlugIn.SupportingDocumentFieldsControlBag.Instance.CodeCodeFindBox, ControlWidthClass.Auto);
			yield return (EU.GUI.PlugIn.SupportingDocumentFieldsControlBag.Instance.ReferenceNumberCodeFindBox, ControlWidthClass.Auto);
			yield return (EU.GUI.PlugIn.SupportingDocumentFieldsControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Auto);
			yield return (SupportingDocumentFieldsControlBag.Instance.YearOfIssueTextBox, ControlWidthClass.Auto);
			yield return (SupportingDocumentFieldsControlBag.Instance.CountryCodeCodeFindBox, ControlWidthClass.Auto);
			yield return (SupportingDocumentFieldsControlBag.Instance.IssuingAuthorityTextBox, ControlWidthClass.Auto);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (EU.GUI.PlugIn.SupportingDocumentFieldsControlBag.Instance.QuantityCalcDropEdit, ControlWidthClass.Long);
			yield return (SupportingDocumentFieldsControlBag.Instance.ValueCalcFindBox, ControlWidthClass.Long);
			yield return (EU.GUI.PlugIn.SupportingDocumentFieldsControlBag.Instance.DateOfExpiryDateEdit, ControlWidthClass.Auto);
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new SupportingDocumentFieldsLayoutBuilder<SupportingDocument>();
}
