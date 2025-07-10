using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(Ucc6SupportingDocumentFieldsLayout))]
sealed class Ucc6SupportingDocumentFieldsLayoutTest : LayoutsAbstractTest
{
	public void TestReferenceNumberCodeVisibility()
	{
		Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("2800", hasPermitAttribute: true), new TestSupportingDocumentCodeList("3200", hasPermitAttribute: false));
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoice = declaration.Invoices.AddNew();
		var document = invoice.SupportingDocuments.AddNew();

		using (var form = new ZForm(document))
		using (var userControl = new LayoutSupportingDocumentsFieldsControl(declaration))
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

	public void TestLineNoCalcEditVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			var invoice = declaration.Invoices.AddNew();
			var document = invoice.SupportingDocuments.AddNew();

			using (var form = new ZForm(document))
			using (var userControl = new LayoutSupportingDocumentsFieldsControl(declaration))
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(declaration, "");
				form.Show();

				var lineNoCalcEdit = userControl.Controls.Find(nameof(SupportingDocumentFieldsControlBag.LineNoCalcEdit), true).FirstOrDefault();

				declaration.JE_MessageType = "IMP";
				AssertEquals("LineNoCalcEdit Visible", false, lineNoCalcEdit.Visible);

				declaration.JE_MessageType = "EXP";
				AssertEquals("LineNoCalcEdit Visible", true, lineNoCalcEdit.Visible);
			}
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
			yield return (SupportingDocumentFieldsControlBag.Instance.LineNoCalcEdit, ControlWidthClass.Auto);
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
