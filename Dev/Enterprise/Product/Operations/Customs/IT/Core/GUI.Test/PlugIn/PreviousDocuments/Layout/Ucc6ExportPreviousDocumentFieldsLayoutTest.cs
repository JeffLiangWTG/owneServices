using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(Ucc6ExportPreviousDocumentFieldsLayout))]
sealed class Ucc6ExportPreviousDocumentFieldsLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new Ucc6ExportPreviousDocumentsFieldsLayoutBuilder<PreviousDocument>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.CodeDropEdit, ControlWidthClass.Auto);
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.ReferenceTextBox, ControlWidthClass.Auto);
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.PackageQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.QuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (PreviousDocumentsFieldsControlBag.Instance.ItemNumberCalcEdit, ControlWidthClass.Auto);
		}
	}

	public void TestAddControlBehaviour()
	{
		var declaration = Factory.New<JobDeclaration>();
		var previousDocument = declaration
			.Invoices.AddNew()
			.InvoiceLines.AddNew()
			.PreviousDocuments.AddNew();

		using (var form = new ZForm())
		using (var dynamicPanel = new DynamicLayoutPanel())
		{
			form.Controls.Add(dynamicPanel);
			form.Show();
			dynamicPanel.SetDataBinding(previousDocument, "");
			dynamicPanel.UpdateLayout(new Ucc6ExportPreviousDocumentFieldsLayout());

			CombineAssertions(() =>
			{
				AssertControlWidth(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.CodeDropEdit, expectedWidth: 980);
				AssertControlWidth(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.ReferenceTextBox, expectedWidth: 980);
				AssertControlWidth(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.PackageQuantityCalcDropEdit, expectedWidth: 980);
				AssertControlWidth(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.QuantityCalcDropEdit, expectedWidth: 980);
				AssertControlWidth(PreviousDocumentsFieldsControlBag.Instance.ItemNumberCalcEdit, expectedWidth: 980);
			});

			void AssertControlWidth(ControlReference controlReference, int expectedWidth)
			{
				var userControl = dynamicPanel.FindSingle<Control>(controlReference.ControlName);
				AssertEquals("Width", expectedWidth, userControl.Width);
			}
		}
	}
}
