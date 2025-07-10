using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.DataMapping.Testing;
using Enterprise.ZArchitecture.GUI.Grid.Internal;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.DataMapping.Testing
{
	class DummyBusinessObjectWithDifferentType : DummyBusinessObject
	{
		public DummyBusinessObjectWithDifferentType(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[List("FilteredCollection")]
		public ZString TestNameOrPK
		{
			get { return testNameOrPK; }
			set { testNameOrPK = value; }
		}

		ZString testNameOrPK;

		public ZPropertyInfo TestNameOrPKInfo
		{
			get { return GetZPropertyInfo(nameof(TestNameOrPK)); }
		}

		public bool Override { get; set; }

		public ZString TestNameOrPKFieldType => Override ? nameof(FieldType.Text) : nameof(FieldType.OrganisationGuid);

		public ZPropertyInfo TestNameOrPKFieldTypeInfo
		{
			get { return GetZPropertyInfo(nameof(TestNameOrPKFieldType)); }
		}

		public ChildClassForTest ChildClassForTest { get; set; }
	}

	class ChildClassForTest : DummyBusinessObject
	{
		public ChildClassForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString TestNameOrPK1
		{
			get { return testNameOrPK1; }
			set { testNameOrPK1 = value; }
		}

		ZString testNameOrPK1;

		public ZString TestNameOrPKFieldType1 => nameof(FieldType.Text);
	}

	class PropertyDescriptorForTest : KPropertyDescriptor
	{
		public PropertyDescriptorForTest(string name) : base(null, name, null) { }

		public override Type PropertyType => typeof(ZString);

		public override Type ComponentType => typeof(ZString);

		public bool IsReadOnlyOnComponent(object component)
		{
			return component.ToString() == "readonly";
		}
	}
	[TestedType(typeof(MassUpdateWizardForm))]
	class MassUpdateWizardFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var form = new MassUpdateWizardForm(Helper.CollectionInfo);
			form.Size = form.MinimumSize;
			return form;
		}

		ImportWizardTestHelper Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new ImportWizardTestWithLongPropertyHelper(Factory);
				}

				return helper;
			}
		}

		ImportWizardTestHelper helper;

		MassUpdateWizardTestHelper InfoHelper
		{
			get
			{
				if (infoHelper == null)
				{
					infoHelper = new MassUpdateWizardTestHelper(Factory);
				}

				return infoHelper;
			}
		}

		MassUpdateWizardTestHelper infoHelper;

		#endregion

		public void TestGetColumnInfoType()
		{
			using (var form = (MassUpdateWizardForm)GetFormToBash())
			{
				Type[] unsupportedTypes = { typeof(ZBlob) };
				var found = 0;
				foreach (var assembly in new[] { Assembly.Load("CargoWise.Types"), Assembly.Load("CargoWise.ResourceStrings.Cache") })
				{
					foreach (var type in assembly.GetTypes())
					{
						if (!type.IsInterface && (typeof(IZType)).IsAssignableFrom(type)
							&& !(Array.IndexOf(type.GetInterfaces(), typeof(IZType)) > 0 && type.Name.EndsWith("Contract")))
						{
							found++;
							var property = new MyPropertyInfo(type);
							if (!unsupportedTypes.Any(x => x == type))
							{
								AssertNoExceptionThrown(() => MassUpdateWizardForm.GetColumnInfoType(InfoHelper.Wizard, property));
							}
							else
							{
								AssertExceptionThrown(typeof(NotSupportedException), () => MassUpdateWizardForm.GetColumnInfoType(InfoHelper.Wizard, property));
							}
						}
					}
				}
				AssertEquals("Number of ZTypes found", 19, found);
			}
		}

		public void TestControlTypeForBizObj_WhenPropertyIsMultiControl()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var dummyChild1 = Factory.NewWithValidTestData<DummyBusinessObjectWithDifferentType>();
			dummyChild1.Override = true;
			dummy.Collection.Add(dummyChild1);
			var dummyChild2 = Factory.NewWithValidTestData<DummyBusinessObjectWithDifferentType>();
			dummyChild2.Factory.Save();
			dummy.Collection.Add(dummyChild2);
			Factory.Save();

			using (var form = new ZForm(dummy))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				grid.BindTo = "Collection";
				var zMultiControlColumnStyleInfo = new ZMultiControlColumnStyleInfo()
				{
					ColumnName = "TestNameOrPK",
					FieldTypeColumnName = "TestNameOrPKFieldType",
					BindToList = "FilteredCollection"
				};
				var descriptor = new PropertyDescriptorForTest("abc");
				((IOverridablePropertyDescriptor)zMultiControlColumnStyleInfo).PropertyDescriptor = descriptor;
				grid.ColumnStyles.Add(zMultiControlColumnStyleInfo);
				form.Controls.Add(grid);
				form.Show();

				var massUpdateWizard = new MassUpdateWizard(new ZGridImportCollectionInfo(grid));

				using (var massUpdateWizardForm = new MassUpdateWizardForm())
				{
					massUpdateWizardForm.SetDataBinding(massUpdateWizard, "");
					var type = massUpdateWizardForm.GetType();
					var field = type.GetField("BizObjsToUpdateGrid", BindingFlags.NonPublic | BindingFlags.Instance);
					var bizObjsToUpdateGrid = (ZGrid)field.GetValue(massUpdateWizardForm);
					var columnStyle = bizObjsToUpdateGrid.ColumnStyles[0];

					AssertEquals("ZMultiControlColumnStyle", ((ZGridColumnInfo)columnStyle).ColumnStyleType.Name);
					AssertEquals("TestNameOrPKFieldType", (columnStyle as ZMultiControlColumnStyleInfo).FieldTypeColumnName);

					massUpdateWizard.LoadBizObjsToUpdate();

					AssertEquals("Text", massUpdateWizard.BizObjsToUpdate[0]["TestNameOrPKFieldType"]);
					AssertEquals("OrganisationGuid", massUpdateWizard.BizObjsToUpdate[1]["TestNameOrPKFieldType"]);

					using (var zMulti = new ZMultiControlColumnStyle(columnStyle as ZMultiControlColumnStyleInfo))
					{
						AssertEquals(FieldType.Text, zMulti.ControlTypeForBizObj(massUpdateWizard.BizObjsToUpdate[0]));
						AssertEquals(FieldType.OrganisationGuid, zMulti.ControlTypeForBizObj(massUpdateWizard.BizObjsToUpdate[1]));
					}
				}
			}
			ErrorReporter.Clear();
		}

		[RequiresSTA]
		public void TestModuleIDForBizObj_WhenPropertyIsMultiControl()
		{
			var dummy = new DummyBusinessObjectCollection(Factory);

			var dummyChild1 = dummy.AddNew();
			dummyChild1.Z0_Code = "CD1";
			dummyChild1.Z0_Guid = ZGuid.NewZGuid();
			dummyChild1.Z0_FK_Code = "FK1";

			var dummyChild2 = dummy.AddNew();
			dummyChild1.Z0_Code = "CD2";
			dummyChild2.Z0_Guid = ZGuid.NewZGuid();
			dummyChild2.Z0_FK_Code = "FK2";

			var dummyChild3 = dummy.AddNew();
			dummyChild3.Z0_Code = "CD3";
			dummyChild3.Z0_Guid = ZGuid.NewZGuid();
			dummyChild3.Z0_FK_Code = "FK3";

			Factory.Save();

			using (var form = new ZForm(dummy))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				grid.BindTo = ".";

				var columnStyle1 = new ZCodeFindBoxColumnStyleInfo()
				{
					ColumnName = "Z0_Code",
					BindToList = ".",
					ModuleID = Modules.Testing.DummyModuleIDs.Dummy
				};

				var columnStyle2 = new ZCodeFindBoxColumnStyleInfo()
				{
					ColumnName = "Z0_Guid",
					ModuleID = Modules.Testing.DummyModuleIDs.DummyWithTemplates
				};

				var columnStyle3 = new ZCodeFindBoxColumnStyleInfo()
				{
					ColumnName = "Z0_FK_Code",
					BindToList = ".",
					ModuleID = Modules.Testing.DummyModuleIDs.DummyThatHitsFilterBizoOnDispose
				};

				grid.ColumnStyles.Add(columnStyle1);
				grid.ColumnStyles.Add(columnStyle2);
				grid.ColumnStyles.Add(columnStyle3);

				form.Controls.Add(grid);
				form.Show();

				using (var massUpdateWizardForm = new MassUpdateWizardForm(new ZGridImportCollectionInfo(grid)))
				{
					var massWizard = (MassUpdateWizard)massUpdateWizardForm.BusinessEntity;
					var filter1 = massWizard.MatchingFilters.AddNew();
					filter1.Field = "Code (Z0_Code)";
					var filter2 = massWizard.MatchingFilters.AddNew();
					filter2.Field = "Id (Z0_Guid)";
					var filter3 = massWizard.MatchingFilters.AddNew();
					filter3.Field = "TestTable|Z0_FK_Code (Z0_FK_Code)";

					massUpdateWizardForm.Show();

					var field = massUpdateWizardForm.GetType().GetField("MachingFiltersGrid", BindingFlags.NonPublic | BindingFlags.Instance);
					var filterGrid = (ZGrid)field.GetValue(massUpdateWizardForm);
					var multiStyle = (ZMultiControlColumnStyle)filterGrid.Columns[2].ColumnStyle;
					var comboControl = multiStyle.EditControl;

					CheckCurrentEditor(filterGrid, comboControl, 0, typeof(ZGridFindBox), DummyModuleIDs.Dummy, out var currentEditor1);
					CheckCurrentEditor(filterGrid, comboControl, 1, typeof(ZGridGuidFindBox), DummyModuleIDs.DummyWithTemplates, out var currentEditor2);
					CheckCurrentEditor(filterGrid, comboControl, 2, typeof(ZGridFindBox), DummyModuleIDs.DummyThatHitsFilterBizoOnDispose, out var currentEditor3);

					Assert("Field type of Guid and TextCodeFindBox should use different editor", !ReferenceEquals(currentEditor1, currentEditor2));
					Assert("Two TextCodeFindBox fields should match the same cached editor", ReferenceEquals(currentEditor1, currentEditor3));
				}
			}
		}

		public void TestCascadingFieldTypesNotThrowExceptions()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var dummyChild1 = Factory.NewWithValidTestData<DummyBusinessObjectWithDifferentType>();
			dummyChild1.ChildClassForTest = Factory.NewWithValidTestData<ChildClassForTest>();
			dummy.Collection.Add(dummyChild1);

			using (var form = new ZForm(dummy))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				grid.BindTo = "Collection";
				var zMultiControlColumnStyleInfo = new ZMultiControlColumnStyleInfo()
				{
					ColumnName = "TestNameOrPK1",
					FieldTypeColumnName = "ChildClassForTest.TestNameOrPKFieldType1",
					BindToList = "FilteredCollection"
				};
				var descriptor = new PropertyDescriptorForTest("abc");
				((IOverridablePropertyDescriptor)zMultiControlColumnStyleInfo).PropertyDescriptor = descriptor;
				grid.ColumnStyles.Add(zMultiControlColumnStyleInfo);
				form.Controls.Add(grid);
				form.Show();

				var massUpdateWizard = new MassUpdateWizard(new ZGridImportCollectionInfo(grid));

				using (var massUpdateWizardForm = new MassUpdateWizardForm())
				{
					massUpdateWizardForm.SetDataBinding(massUpdateWizard, "");

					AssertNoExceptionThrown(() => massUpdateWizard.LoadBizObjsToUpdate());
					AssertEquals("Text", massUpdateWizard.BizObjsToUpdate[0]["ChildClassForTest.TestNameOrPKFieldType1"]);
				}
			}
			ErrorReporter.Clear();
		}

		void CheckCurrentEditor(ZGrid filterGrid, ZMultiCombinationControl comboControl, int row, Type expectedEditorType, ModuleIdentifier expectedModuleIdentifier, out ZPopupFindBox currentEditor)
		{
			filterGrid.CurrentCell = new DataGridCell(row, 2);
			currentEditor = (ZPopupFindBox)comboControl.CurrentEditor;
			currentEditor.SelectFromPopupForm();
			var popupForm = (EmbeddedModulePopup)currentEditor.PopupForm;
			AssertType("Current editor type should match expected", expectedEditorType, currentEditor);
			AssertEquals("Module id of popup form should match expected", expectedModuleIdentifier, popupForm.Module.ID);
			AssertEquals("The current editor module ID should change back to the default value", currentEditor.ModuleID, ModuleIDs.NotAssigned);
			popupForm.Close();
		}
	}
}
