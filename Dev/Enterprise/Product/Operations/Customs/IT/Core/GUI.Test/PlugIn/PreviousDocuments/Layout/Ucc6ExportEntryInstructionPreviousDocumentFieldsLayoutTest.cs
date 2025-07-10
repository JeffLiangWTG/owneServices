using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(Ucc6ExportEntryInstructionPreviousDocumentFieldsLayout))]
sealed class Ucc6ExportEntryInstructionPreviousDocumentFieldsLayoutTest : LayoutsAbstractTest
{
	public void TestAddControlBehaviour()
	{
		var declaration = Factory.New<JobDeclaration>();
		var previousDocument = declaration
			.CustomsEntryInstructions.AddNew()
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
			});

			void AssertControlWidth(ControlReference controlReference, int expectedWidth)
			{
				var userControl = dynamicPanel.FindSingle<Control>(controlReference.ControlName);
				AssertEquals("Width", expectedWidth, userControl.Width);
			}
		}
	}

	public void TestCodeDropEditCaption()
	{
		var previousDocument = Factory.New<PreviousDocument>();
		var layout = new Ucc6ExportEntryInstructionPreviousDocumentFieldsLayout().Layout;
		layout.TryGetCaption(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.CodeDropEdit, previousDocument, out var resourceStringData);

		AssertNotNull("ResourceData", resourceStringData);
		AssertEquals("Caption", "Type", resourceStringData.Caption);
		AssertEquals("FullDescription", "[12 01 002 000] Type", resourceStringData.FullDescription);
	}

	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.GUI.PlugIn.PreviousDocumentsFieldsLayoutBuilder<PreviousDocument>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.CodeDropEdit, ControlWidthClass.Auto);
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.ReferenceTextBox, ControlWidthClass.Auto);
		}
	}
}
