using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class NonPersistentBusinessObjectBindingRegistryItemEditorTest : TestCase
	{
		#region TestSetValueFromEditorPane

		class SimpleBizo : NonPersistentBusinessObject, IRegistryBusiness
		{
			public bool HasBeenTransformed;

			public IRegistryBusiness Clone(FallbackLevel currentFallbackLevel, BusinessObjectFactory factory)
			{
				return new SimpleBizo { HasBeenTransformed = HasBeenTransformed };
			}

			public FallbackLevel CurrentFallbackLevel { get; set; }
		}

		class EditorThatTransformsBeforeBinding : NonPersistentBusinessObjectBindingRegistryItemEditor
		{
			public EditorThatTransformsBeforeBinding(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
				: base(dataType, fallbackLevel, factory)
			{
			}

			protected override IRegistryBusiness PrepareDataForBinding(IRegistryBusiness data)
			{
				data = base.PrepareDataForBinding(data);
				((SimpleBizo)data).HasBeenTransformed = true;
				return data;
			}

			protected override RegistryZUserControl NewBoundWinFormsEditorPane()
			{
				return new RegistryZUserControl();
			}
		}

		public void TestSetValueFromEditorPaneCallsTransform()
		{
			var factory = new BusinessObjectFactory();
			var fallbackLevel = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
			var dataType = new Mock<IRegistryDataType>();

			var bizo = new SimpleBizo();
			var editor = new EditorThatTransformsBeforeBinding(dataType.Object, fallbackLevel, factory);

			using (var form = new ZForm())
			using (var control = (ZUserControl)editor.NewWinFormsEditorPane())
			{
				AssertNull("PRE: We never set the bizo's fallback", bizo.CurrentFallbackLevel);
				editor.SetValueFromEditorPane(control, bizo);
				AssertEquals("Fallback should be set", fallbackLevel, bizo.CurrentFallbackLevel);

				form.Controls.Add(control);
				form.Show();

				var dataItem = (SimpleBizo)control.CurrentDataItem;
				Assert("The data should have been prepared first", dataItem.HasBeenTransformed);
			}
		}

		[RequiresSTA]
		public void TestSetValueFromEditorPane()
		{
			DummyRegistryBusinessObject dummy = new DummyRegistryBusinessObject(null, null);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			FallbackLevel fallbackLevel = new FallbackLevel(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
			TestRegistryItemEditor editor = new TestRegistryItemEditor(new NonPersistentBusinessObjectRegistryDataTypeTest(), fallbackLevel, factory);
			editor.ReturnEmptyZUserControl = true;

			using (ZUserControl editorPane = (ZUserControl)editor.NewWinFormsEditorPane())
			using (ZForm form = new ZForm())
			{
				form.Show();
				form.Controls.Add(editorPane);
				editor.SetValueFromEditorPane(editorPane, dummy);

				DummyRegistryBusinessObject clone = (DummyRegistryBusinessObject)editorPane.CurrentDataItem;

				AssertEquals("Clone.CurrentFactory", factory, clone.CurrentFactory);
				AssertEquals("Clone.CurrentFallbackLevel", fallbackLevel, clone.CurrentFallbackLevel);
			}
		}

		#endregion

		#region TestBindingToNonPersistentBusinessObject

		public void TestBindingToNonPersistentBusinessObject()
		{
			using (ZForm form = new ZForm())
			{
				TestRegistryItemEditor editor = new TestRegistryItemEditor(new NonPersistentBusinessObjectRegistryDataTypeTest(), null, null);

				Control editorPane = editor.NewWinFormsEditorPane();
				NonPersistentBusinessObjectTest bO = new NonPersistentBusinessObjectTest();
				editor.SetValueFromEditorPane(editorPane, bO);
				form.Show();
				Application.DoEvents();

				form.Controls.Add(editorPane);
				bO.Property = "splaty";
				AssertEquals("Initial bind should work, even though the value was set before the control is shown", bO.Property.ToUpper(), ((BoundUserControlTest)editorPane).TextBox.Text.ToUpper());

				NonPersistentBusinessObjectTest bO2 = new NonPersistentBusinessObjectTest();
				bO2.Property = "splaty2";
				editor.SetValueFromEditorPane(editorPane, bO2);
				AssertEquals("Rebinding should work", bO2.Property.ToUpper(), ((BoundUserControlTest)editorPane).TextBox.Text.ToUpper());
			}
		}

		#endregion

		#region Implementation

		#region class TestRegistryItemEditor

		class TestRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
		{
			public TestRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
				: base(dataType, fallbackLevel, factory)
			{
			}

			public bool ReturnEmptyZUserControl
			{
				get { return returnEmptyZUserControl; }
				set { returnEmptyZUserControl = value; }
			}

			protected override RegistryZUserControl NewBoundWinFormsEditorPane()
			{
				return (ReturnEmptyZUserControl) ? new RegistryZUserControl() : new BoundUserControlTest();
			}

			bool returnEmptyZUserControl;
		}

		#endregion

		#region class DummyRegistryDataType

		class DummyRegistryBusinessObject : RegistryBusinessObjectTemplate
		{
			public DummyRegistryBusinessObject(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
				: base(fallbackLevel, factory)
			{
			}

			protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			{
				return new DummyRegistryBusinessObject(fallbackLevel, factory);
			}

			public new BusinessObjectFactory CurrentFactory
			{
				get { return base.CurrentFactory; }
			}

			protected override void ReadElements(XmlReaderWrapper reader)
			{
			}
		}

		#endregion

		#endregion
	}
}
