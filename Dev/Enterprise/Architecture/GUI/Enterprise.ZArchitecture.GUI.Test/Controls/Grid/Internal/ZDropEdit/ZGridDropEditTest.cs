using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;

namespace Enterprise.Core.Forms.Testing
{
	sealed class ZGridDropEditTest : TestCaseWithDummy
	{
		public void TestTranslationFeedbackManager()
		{
			var translationFeedbackProviderMock = new Mock<ITranslationFeedbackProvider>();
			using (ObjectFactory.Substitute(translationFeedbackProviderMock.Object))
			{
				using var dropEdit = new ZGridDropEditForTesting();
				((IGridControl)dropEdit).ActivateEditControl();

				AssertNull("No item is selected.", dropEdit.LastSelectedItem);
				translationFeedbackProviderMock.Verify(m => m.Feedback(It.IsAny<Control>(), It.IsAny<ICodeDescription>()), Times.Never);

				using (TranslationFeedbackManager.EnableTranslationFeedbackModeForTest())
				{
					((IGridControl)dropEdit).ActivateEditControl();
					AssertNull("No item is selected.", dropEdit.LastSelectedItem);
					translationFeedbackProviderMock.Verify(m => m.Feedback(It.IsAny<Control>(), It.IsAny<ICodeDescription>()), Times.Never);
				}

				using (TranslationFeedbackManager.EnableTranslationFeedbackModeForTest())
				{
					dropEdit.ButtonClicked = true;
					((IGridControl)dropEdit).ActivateEditControl();
					AssertNull("No item is selected.", dropEdit.LastSelectedItem);
					translationFeedbackProviderMock.Verify(m => m.Feedback(It.IsAny<Control>(), It.IsAny<ICodeDescription>()), Times.Never);
				}

				using (TranslationFeedbackManager.EnableTranslationFeedbackModeForTest())
				{
					dropEdit.IsMouseOnAValidRowOverride = true;
					((IGridControl)dropEdit).ActivateEditControl();
					AssertNull("No item is selected.", dropEdit.LastSelectedItem);
					translationFeedbackProviderMock.Verify(m => m.Feedback(It.IsAny<Control>(), It.IsAny<ICodeDescription>()), Times.Never);
				}
			}
		}

		public void TestZGridDropEditShouldOpenTranslationFeedbackForm()
		{
			var translationFeedbackProviderMock = new Mock<ITranslationFeedbackProvider>();
			using (ObjectFactory.Substitute(translationFeedbackProviderMock.Object))
			using (TranslationFeedbackManager.EnableTranslationFeedbackModeForTest())
			{
				var collection = new DummyBusinessObjectCollection(Factory);
				var dummy = Factory.New<DummyBizOWithListAttribute>();
				dummy.Z0_Code = "";
				collection.Add(dummy);

				using var form = new ZForm(collection);
				var info = new TestZDropEditColumnStyleInfo("Z0_Code", 100);
				info.BindToList = "List";
				info.Caption = "Z0_Code";
				info.ColumnName = "Z0_Code";

				var grid = new ZGrid();
				grid.BindTo = ".";
				grid.ColumnStyles.Add(info);
				form.Controls.Add(grid);

				form.Show();
				Application.DoEvents();

				var style = (TestZDropEditColumnStyle)grid.Columns[0].ColumnStyle;
				var dropEdit = (ZGridDropEditForTesting)style.EditControl;
				dropEdit.ButtonClicked = true;
				dropEdit.IsMouseOnAValidRowOverride = true;

				dropEdit.Text = "Invalid";

				((IGridControl)dropEdit).ActivateEditControl();
				AssertNotNull("It should select the item in the drop edit.", dropEdit.List[0]);
				translationFeedbackProviderMock.Verify(m => m.Feedback(dropEdit, (ICodeDescription)dropEdit.List[0]), Times.Never);

				dropEdit.Text = "Code";

				((IGridControl)dropEdit).ActivateEditControl();
				AssertNotNull("It should select the item in the drop edit.", dropEdit.List[0]);
				translationFeedbackProviderMock.Verify(m => m.Feedback(dropEdit, (ICodeDescription)dropEdit.List[0]), Times.Once);
			}
		}

		class DummyBizOWithListAttribute : DummyBusinessObject
		{
			public DummyBizOWithListAttribute(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[List("List")]
			public override ZString Z0_Code
			{
				get
				{
					return base.Z0_Code;
				}
				set
				{
					base.Z0_Code = value;
				}
			}

			public CodeDescriptionPairList List
			{
				get
				{
					return list ?? (list = new CodeDescriptionPairList() { new CodeDescriptionPair("Code", "Description") });
				}
			}
			CodeDescriptionPairList list;
		}

		class TestZDropEditColumnStyleInfo : ZDropEditColumnStyleInfo
		{
			public TestZDropEditColumnStyleInfo(string columnName, int width)
				: base(columnName, width)
			{
			}

			public override Type ColumnStyleType
			{
				get { return typeof(TestZDropEditColumnStyle); }
			}
		}

		class TestZDropEditColumnStyle : ZDropEditColumnStyle
		{
			public TestZDropEditColumnStyle(ZDropEditColumnStyleInfo columnInfo)
				: base(columnInfo, () => new ZGridDropEditForTesting())
			{
			}

			public TestZDropEditColumnStyle(ZDropEditColumnStyleInfo columnInfo, Func<Control> editControl)
				: base(columnInfo, editControl)
			{
			}
		}

		public void TestGetList_WhenParentIsZMultiCombinationControl()
		{
			var dummyChild = Dummy.Collection.AddNew();
			dummyChild.Z0_NVarChar = nameof(FieldType.Text);
			var columnStyleInfo = new ZMultiControlColumnStyleInfo();
			columnStyleInfo.ColumnName = "__CONTRACTNUMBER__prop__ZString";
			columnStyleInfo.Caption = "ContractNumber";
			((IOverridablePropertyDescriptor)columnStyleInfo).PropertyDescriptor = new WorkflowCustomPropertyDescriptorForTesting("__CONTRACTNUMBER__prop__ZString", typeof(ZString), true);

			using (var testForm = new ZTestForm())
			{
				testForm.Grid.ColumnStyles.Add(columnStyleInfo);
				testForm.Grid.SetDataBinding(Dummy, "Collection");

				var propertyCollection = new CustomPropertyCollectionImpl(propertyName => "A", null);
				propertyCollection.Add(typeof(ZString), "__CONTRACTNUMBER__prop__ZString", DynamicMetaData.ListDataSource(new[] { "A", "B" }));
				var customBusinessObject = new CustomBusinessObject(Factory, null, propertyCollection);
				dummyChild.RegisterEditableChildObject(customBusinessObject);

				testForm.Show();
				Application.DoEvents();

				using (var columnStyle = (ZMultiControlColumnStyle)testForm.Grid.TableStyles[0].GridColumnStyles[columnStyleInfo.ColumnName])
				{
					AssertNotNull(columnStyle);

					var multiCombinationControl = columnStyle.EditControl;
					AssertNotNull(multiCombinationControl);

					var dropEditControl = (ZGridDropEdit)multiCombinationControl.GetControlForControlType(columnStyle.ControlTypeForBizObj(dummyChild));
					AssertNotNull(dropEditControl);

					testForm.Grid.Controls.Add(multiCombinationControl);
					testForm.Grid.CurrentCell = new DataGridCell(0, 2);
					multiCombinationControl.Controls.Add(dropEditControl);
					dropEditControl.DataPropertyName = "__CONTRACTNUMBER__prop__ZString";
					dropEditControl.CurrentItem = dummyChild;
					var list = dropEditControl.List;

					AssertNotNull("List should not be null", list);
					AssertEquals("List contains two elements", 2, list.Count);
					AssertEquals("A", list[0]);
					AssertEquals("B", list[1]);
				}
			}
		}

		public void TestGetList_WhenControlIsNotFocusedOrIsReadOnly()
		{
			var dummyChild = Dummy.Collection.AddNew();
			dummyChild.Z0_NVarChar = nameof(FieldType.Text);
			var columnStyleInfo = new ZMultiControlColumnStyleInfo();
			columnStyleInfo.ColumnName = "__CONTRACTNUMBER__prop__ZString";
			columnStyleInfo.Caption = "ContractNumber";
			((IOverridablePropertyDescriptor)columnStyleInfo).PropertyDescriptor = new WorkflowCustomPropertyDescriptorForTesting("__CONTRACTNUMBER__prop__ZString", typeof(ZString), true);

			using (var testForm = new ZTestForm())
			{
				testForm.Grid.ColumnStyles.Add(columnStyleInfo);
				testForm.Grid.SetDataBinding(Dummy, "Collection");

				var propertyCollection = new CustomPropertyCollectionImpl(propertyName => "A", null);
				propertyCollection.Add(typeof(ZString), "__CONTRACTNUMBER__prop__ZString", DynamicMetaData.ListDataSource(new[] { "A", "B" }));
				var customBusinessObject = new CustomBusinessObject(Factory, null, propertyCollection);
				dummyChild.RegisterEditableChildObject(customBusinessObject);

				testForm.Show();
				Application.DoEvents();

				using (var columnStyle = (ZMultiControlColumnStyle)testForm.Grid.TableStyles[0].GridColumnStyles[columnStyleInfo.ColumnName])
				{
					AssertNotNull(columnStyle);

					var multiCombinationControl = columnStyle.EditControl;
					AssertNotNull(multiCombinationControl);

					var dropEditControl = (ZGridDropEdit)multiCombinationControl.GetControlForControlType(columnStyle.ControlTypeForBizObj(dummyChild));
					AssertNotNull(dropEditControl);

					testForm.Grid.Controls.Add(multiCombinationControl);
					testForm.Grid.CurrentCell = new DataGridCell(0, 1);
					multiCombinationControl.Controls.Add(dropEditControl);
					dropEditControl.DataPropertyName = "__CONTRACTNUMBER__prop__ZString";
					dropEditControl.CurrentItem = dummyChild;
					dropEditControl.CodeBox.ReadOnly = true;
					var list = dropEditControl.List;

					AssertNull("List is null", list);
				}
			}
		}
	}
}
