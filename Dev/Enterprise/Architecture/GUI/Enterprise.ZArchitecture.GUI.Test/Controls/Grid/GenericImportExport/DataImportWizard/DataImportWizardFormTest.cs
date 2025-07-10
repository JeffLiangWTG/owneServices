using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.DataMapping.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.DataMapping.Testing
{
	[TestedType(typeof(DataImportWizardForm))]
	sealed class DataImportWizardFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new DataImportWizardForm(Helper.CollectionInfo, "");
		}

		ImportWizardTestHelper Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new ImportWizardTestHelper(Factory);
				}

				return helper;
			}
		}

		ImportWizardTestHelper helper;

		ImportWizardTestHelperReadOnly ReadOnlyHelper
		{
			get
			{
				if (readOnlyHelper == null)
				{
					readOnlyHelper = new ImportWizardTestHelperReadOnly(Factory);
				}
				return readOnlyHelper;
			}
		}

		ImportWizardTestHelperReadOnly readOnlyHelper;

		#endregion

		public void TestCaptionName()
		{
			using (var form = new DataImportWizardForm(Helper.CollectionInfo, "", "TestModule"))
			{
				AssertEquals("Form Caption Name should have Module Name", "Data Import Wizard - TestModule", form.FormCaption);
			}

			using (var form = new DataImportWizardForm(Helper.CollectionInfo, ""))
			{
				AssertEquals("Form Caption Name should not have Module Name", "Data Import Wizard", form.FormCaption);
			}
		}

		public void TestGetColumnInfoType()
		{
			using (var form = (DataImportWizardForm)GetFormToBash())
			{
				foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(typeof(MyObject)))
				{
					IImportPropertyInfo property = new ImportPropertyInfoImpl(propertyDescriptor);
					AssertNoExceptionThrown(() => DataImportWizardForm.GetColumnInfoType(Helper.Wizard, property));
				}
			}
		}

		public void TestDoImportOnFactorylessCollection()
		{
			IImportCollectionInfo collectionInfo = new ImportCollectionInfoImpl(new DummyNonPersistentBusinessObjectCollection(null));
			using (var form = new DataImportWizardForm(collectionInfo, ""))
			{
				AssertNoExceptionThrown("Even though factory is null, import should proceed", () => form.DoImport());
			}
		}

		class DummyNonPersistentBusinessObjectCollectionForTest : NonPersistentBusinessObjectCollection<DummyNonPersistentBusinessObject>
		{
			protected override bool AllowNewCore => false;

			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new DummyNonPersistentBusinessObject();
			}
		}

		public void TestDoImportWhenCollectionAllowNewIsFalseAndNoUpdateExistingColumnTicked()
		{
			var testCollection = new DummyNonPersistentBusinessObjectCollectionForTest();
			testCollection.AddNew();
			var collectionInfo = new ImportCollectionInfoImpl(testCollection);
			using (var form = new DataImportWizardForm(collectionInfo, ""))
			{
				var wizardMock = new Mock<ImportWizard>(collectionInfo, null, new FileMapperForTest()) { CallBase = true };
				wizardMock.Setup(m => m.LoadFile(1, -1, false))
					.Returns(new List<string[]>() { new string[] { "1", "AAA", "Y", "12", "43.3", "2011-3-10", "value" } });

				((KForm)form).DataSource = wizardMock.Object;
				AssertNoExceptionThrown("Error message should be shown", () => form.DoImport());
				AssertEquals("Adding new records is not supported by this collection. Please try to use 'Match for Update' functionality.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestReadOnlyWarnings()
		{
			ReadOnlyHelper.Wizard.GenerateReadOnlyWarnings();
			var mapping = ReadOnlyHelper.Wizard.Mapping.OfType<ImportWizardMapping>();
			var readOnlyMapping = mapping.First(x => x.MappingName == "Z0_AnotherNumber");
			AssertEquals("Z0_AnotherNumber is marked read only", true, readOnlyMapping.PropertyIsReadOnly);
			AssertEquals("Has a read only warning", 1, readOnlyMapping.GetWarnings().Count());
			AssertEquals("Mapping should have 6 read only warnings", 5, ReadOnlyHelper.Wizard.Mapping.GetWarnings().Count());
		}

		public void TestFromListViewRetrieveVirtualItem()
		{
			using (var form = new DataImportWizardForm())
			{
				var arg = new RetrieveVirtualItemEventArgs(0);
				form.FromListView_RetrieveVirtualItem(this, arg);
				AssertEquals("List view item should ALWAYS add sub item while retrieving virtual item", 2, arg.Item.SubItems.Count);
			}
		}

		public void TestShouldDisableImportDataMenuItemForToGrid()
		{
			using (var form = new DataImportWizardFormForTest())
			{
				AssertEquals("Should disable import data menu item", true, form.ToGridForTest.DisableImportDataMenuItem);
			}
		}

		class MyObject
		{
			public ZByte ZByteProp { get; set; }
			public ZShort ZShortProp { get; set; }
			public ZInt ZIntProp { get; set; }
			public ZLong ZLongProp { get; set; }
			public ZDecimal ZDecimalProp { get; set; }
			public ZString ZStringProp { get; set; }
			public MultilingualString MultilingualStringProp { get; set; }
			public ZDate ZDateProp { get; set; }
			public ZDateTime ZDateTimeProp { get; set; }
			public ZDateTimeOffset ZDateTimeOffsetProp { get; set; }
			public ZGuid ZGuidProp { get; set; }
			public ZBool ZBoolProp { get; set; }
		}

		class DataImportWizardFormForTest : DataImportWizardForm
		{
			public ZGrid ToGridForTest => base.ToGrid;
		}
	}
}
