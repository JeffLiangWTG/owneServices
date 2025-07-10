using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Grid.Testing
{
	[TestedType(typeof(GridRowFinderBusinessObject))]
	[GuiTest]
	sealed class GridRowFinderBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestColumnsToSearch()
		{
			var bizo = Factory.New<DummyBusinessObjectForRowFinderTest>();

			using (var form = new TestFormForGridRowFinder(bizo))
			{
				var integerInfo = new ZTextBoxColumnStyleInfo { ColumnName = "Z0_AnotherNumber", Caption = "IntegerTest" };
				var dateTimeInfo = new ZTextBoxColumnStyleInfo { ColumnName = "Z0_SmallDateTime", Caption = "DateTimeTest" };

				form.Grid.ColumnStyles.Add(integerInfo);
				form.Grid.ColumnStyles.Add(dateTimeInfo);
				form.Show();

				var rowFinderBizo = new GridRowFinderBusinessObject(form.Grid);

				AssertEquals(5, rowFinderBizo.ColumnsToSearch.Count);

				AssertEquals("Test", rowFinderBizo.ColumnsToSearch[0].Description);
				AssertEquals(true, rowFinderBizo.ColumnsToSearch[0].Value);

				AssertEquals("Test2", rowFinderBizo.ColumnsToSearch[1].Description);
				AssertEquals(true, rowFinderBizo.ColumnsToSearch[1].Value);

				AssertEquals("MultilingualTest", rowFinderBizo.ColumnsToSearch[2].Description);
				AssertEquals(true, rowFinderBizo.ColumnsToSearch[2].Value);

				AssertEquals("LOL", rowFinderBizo.ColumnsToSearch[3].Description);
				AssertEquals(true, rowFinderBizo.ColumnsToSearch[3].Value);

				AssertEquals("IntegerTest", rowFinderBizo.ColumnsToSearch[4].Description);
				AssertEquals(true, rowFinderBizo.ColumnsToSearch[4].Value);
			}
		}

		public void TestColumnsToSearch_NullRef()
		{
			DummyBusinessObjectForRowFinderTest bizo = Factory.New<DummyBusinessObjectForRowFinderTest>();
			using (TestFormForGridRowFinder form = new TestFormForGridRowFinder(bizo))
			{
				form.Show();

				var rowFinder = new GridRowFinderBusinessObject(form.Grid);
				rowFinder.ToggleSearchColumns();
				rowFinder.ColumnsToSearch["Test"].Value = true;

				var info = new ZTextBoxColumnStyleInfo { Caption = "AAA", ColumnName = "AAA" };
				var column = new ZTextBoxColumnStyle(info) { HeaderText = "BBB" };
				form.Grid.TableStyles[0].GridColumnStyles.Add(column);

				rowFinder.TextToSearchFor = "hello";
				AssertNoExceptionThrown(() => rowFinder.DoSearch());
			}
		}

		public void TestColumnsToSearch2()
		{
			DummyBusinessObjectForRowFinderTest bizo = Factory.New<DummyBusinessObjectForRowFinderTest>();
			using (TestFormForGridRowFinder form = new TestFormForGridRowFinder(bizo))
			{
				form.Show();

				var rowFinder = new GridRowFinderBusinessObject(form.Grid);

				form.Grid.CurrentRowIndex = 0;
				rowFinder.ToggleSearchColumns();
				rowFinder.ColumnsToSearch["Test2"].Value = true;
				rowFinder.TextToSearchFor = "hello";
				rowFinder.DoSearch();
				AssertEquals(1, form.Grid.SelectedElements.Length);
				AssertEquals(bizo.Dummy2, form.Grid.SelectedElements[0]);

				rowFinder.DoSearch();
				AssertEquals(bizo.Dummy2, form.Grid.SelectedElements[0]);

				rowFinder.DoSearch();
				AssertEquals(bizo.Dummy2, form.Grid.SelectedElements[0]);
			}
		}

		public void TestSearchMultilingualString()
		{
			DummyBusinessObjectForRowFinderTest bizo = Factory.New<DummyBusinessObjectForRowFinderTest>();
			using (TestFormForGridRowFinder form = new TestFormForGridRowFinder(bizo))
			{
				form.Show();

				var rowFinder = new GridRowFinderBusinessObject(form.Grid);

				form.Grid.CurrentRowIndex = 0;
				rowFinder.ToggleSearchColumns();
				rowFinder.ColumnsToSearch["MultilingualTest"].Value = true;
				rowFinder.TextToSearchFor = "J";
				rowFinder.DoSearch();
				AssertEquals(1, form.Grid.SelectedElements.Length);
				AssertEquals(bizo.Dummy1, form.Grid.SelectedElements[0]);
				AssertEquals(2, rowFinder.matchingRows.Count);
				rowFinder.DoSearch();
				AssertEquals(1, form.Grid.SelectedElements.Length);
				AssertEquals(bizo.Dummy2, form.Grid.SelectedElements[0]);

				rowFinder.TextToSearchFor = "K";
				rowFinder.DoSearch();
				AssertEquals(-1, rowFinder.currentIndex);
			}
		}

		public void TestSearchInteger()
		{
			var bizo = Factory.New<DummyBusinessObjectForRowFinderTest>();
			bizo.Dummy1.Z0_AnotherNumber = 100;
			bizo.Dummy2.Z0_AnotherNumber = 200;

			Factory.Save();

			using (var form = new TestFormForGridRowFinder(bizo))
			{
				var integerInfo = new ZTextBoxColumnStyleInfo { ColumnName = "Z0_AnotherNumber", Caption = "IntegerTest" };
				form.Grid.ColumnStyles.Add(integerInfo);
				form.Show();

				var rowFinder = new GridRowFinderBusinessObject(form.Grid);
				rowFinder.ToggleSearchColumns();
				rowFinder.ColumnsToSearch["IntegerTest"].Value = true;
				rowFinder.TextToSearchFor = "200";
				rowFinder.DoSearch();

				AssertEquals(1, form.Grid.SelectedElements.Length);
				AssertEquals(bizo.Dummy2, form.Grid.SelectedElements[0]);

				rowFinder.TextToSearchFor = "400";
				rowFinder.DoSearch();
				AssertEquals(-1, rowFinder.currentIndex);
			}
		}

		public void TestSearchGuid_PropertyValueIsString()
		{
			var bizo = Factory.New<DummyBusinessObjectForRowFinderTest>();
			using (var form = new TestFormForGridRowFinder(bizo))
			{
				var guidInfo = new ZGuidDropEditColumnStyleInfo { ColumnName = "ZGuidValue", Caption = "ZGuidAsString" };
				form.Grid.ColumnStyles.Add(guidInfo);
				form.Show();

				var rowFinder = new GridRowFinderBusinessObject(form.Grid);
				form.Grid.CurrentRowIndex = 0;
				rowFinder.ToggleSearchColumns();
				rowFinder.ColumnsToSearch["ZGuidAsString"].Value = true;
				rowFinder.TextToSearchFor = "DUM2";
				rowFinder.DoSearch();

				AssertEquals(1, form.Grid.SelectedElements.Length);
				AssertEquals(bizo.Dummy2, form.Grid.SelectedElements[0]);
			}
		}

		public void TestSearchGuid_PropertyInfoIsNull()
		{
			DummyBusinessObjectForRowFinderTest bizo = Factory.New<DummyBusinessObjectForRowFinderTest>();
			using (TestFormForGridRowFinder form = new TestFormForGridRowFinder(bizo))
			{
				form.Show();

				var rowFinder = new GridRowFinderBusinessObject(form.Grid);
				rowFinder.ToggleSearchColumns();

				var propertyDescriptors = typeof(ZPropertyInfoHashtable).GetProperty("PropertyNamesToPropertyDescriptors", BindingFlags.NonPublic | BindingFlags.Instance);
				AssertNotNull(propertyDescriptors);
				var descriptors = (System.Collections.Generic.IDictionary<string, System.ComponentModel.PropertyDescriptor>)propertyDescriptors.GetValue(bizo.Dummy1.ZPropertyInfoHash);
				AssertNotNull(descriptors);
				descriptors.Remove("Z0_Guid");

				var fieldProperties = typeof(ZCustomTypeDescriptor).GetField("properties", BindingFlags.NonPublic | BindingFlags.Instance);
				AssertNotNull(fieldProperties);
				var properties = (CargoWise.ComponentModel.KPropertyDescriptorCollection)fieldProperties.GetValue(bizo.Dummy1);
				AssertNotNull(properties);
				foreach (System.ComponentModel.PropertyDescriptor p in properties)
				{
					if (p.Name == "Z0_GuidInfo")
					{
						properties.Remove(p);
					}
				}

				form.Grid.CurrentRowIndex = 0;

				rowFinder.ColumnsToSearch["LOL"].Value = true;
				rowFinder.TextToSearchFor = "A";
				AssertNoExceptionThrown(() => rowFinder.DoSearch());
			}
		}

		public void TestColumnsToSearch_AllColumns()
		{
			DummyBusinessObjectForRowFinderTest bizo = Factory.New<DummyBusinessObjectForRowFinderTest>();
			using (TestFormForGridRowFinder form = new TestFormForGridRowFinder(bizo))
			{
				form.Show();

				var rowFinder = new GridRowFinderBusinessObject(form.Grid);

				rowFinder.DoSearch();
				rowFinder.TextToSearchFor = "hello";
				rowFinder.DoSearch();
				AssertEquals("hello", rowFinder.TextToSearchFor);

				AssertEquals(bizo.Dummy1, form.Grid.SelectedElements[0]);

				rowFinder.DoSearch();
				AssertEquals("Moves to next item", bizo.Dummy2, form.Grid.SelectedElements[0]);

				rowFinder.DoSearch();
				AssertEquals("Wraps around", bizo.Dummy1, form.Grid.SelectedElements[0]);
			}
		}

		public void TestSearch_MatchCase()
		{
			DummyBusinessObjectForRowFinderTest bizo = Factory.New<DummyBusinessObjectForRowFinderTest>();
			using (TestFormForGridRowFinder form = new TestFormForGridRowFinder(bizo))
			{
				form.Show();

				var rowFinder = new GridRowFinderBusinessObject(form.Grid);
				rowFinder.MatchCase = true;
				rowFinder.TextToSearchFor = "hello";
				rowFinder.DoSearch();
				AssertEquals(bizo.Dummy1, form.Grid.SelectedElements[0]);
				AssertEquals(1, rowFinder.matchingRows.Count);

				rowFinder.TextToSearchFor = "HELLo";
				rowFinder.DoSearch();
				AssertEquals(bizo.Dummy2, form.Grid.SelectedElements[0]);
				AssertEquals(1, rowFinder.matchingRows.Count);

				rowFinder.TextToSearchFor = "Hello";
				rowFinder.DoSearch();
				AssertEquals(0, rowFinder.matchingRows.Count);
			}
		}

		public void TestSearch_SelectAllRows()
		{
			DummyBusinessObjectForRowFinderTest bizo = Factory.New<DummyBusinessObjectForRowFinderTest>();
			using (TestFormForGridRowFinder form = new TestFormForGridRowFinder(bizo))
			{
				form.Show();

				var rowFinder = new GridRowFinderBusinessObject(form.Grid);
				rowFinder.SelectAllRows = true;
				rowFinder.TextToSearchFor = "hello";
				rowFinder.DoSearch();
				AssertEquals(bizo.Dummy1, form.Grid.SelectedElements[0]);
				AssertEquals(bizo.Dummy2, form.Grid.SelectedElements[1]);
				AssertEquals(2, rowFinder.matchingRows.Count);
			}
		}

		public void TestSearch_SearchFromSelectedRow()
		{
			DummyBusinessObjectForRowFinderTest2 bizo = Factory.New<DummyBusinessObjectForRowFinderTest2>();
			using (TestFormForGridRowFinder form = new TestFormForGridRowFinder(bizo))
			{
				form.Show();

				var rowFinder = new GridRowFinderBusinessObject(form.Grid);
				rowFinder.SearchFromSelectedRow = true;
				rowFinder.TextToSearchFor = "hello";
				form.Grid.Select(1);
				rowFinder.DoSearch();
				AssertEquals(bizo.Dummy4, form.Grid.SelectedElements[0]);
				AssertEquals(3, rowFinder.matchingRows.Count);
				rowFinder.DoSearch();
				AssertEquals(bizo.Dummy7, form.Grid.SelectedElements[0]);
				AssertEquals(3, rowFinder.matchingRows.Count);
				rowFinder.DoSearch();
				AssertEquals(bizo.Dummy1, form.Grid.SelectedElements[0]);
				AssertEquals(3, rowFinder.matchingRows.Count);
				form.Grid.CurrentRowIndex = 4;
				rowFinder.DoSearch();
				AssertEquals(bizo.Dummy7, form.Grid.SelectedElements[0]);
				AssertEquals(3, rowFinder.matchingRows.Count);
				form.Grid.CurrentRowIndex = 2;
				rowFinder.DoSearch(false);
				AssertEquals(bizo.Dummy1, form.Grid.SelectedElements[0]);
				AssertEquals(3, rowFinder.matchingRows.Count);
				rowFinder.MatchCase = true;
				form.Grid.CurrentRowIndex = 5;
				rowFinder.DoSearch();
				AssertEquals(bizo.Dummy1, form.Grid.SelectedElements[0]);
				AssertEquals(2, rowFinder.matchingRows.Count);
				form.Grid.CurrentRowIndex = 5;
				rowFinder.DoSearch(false);
				AssertEquals(bizo.Dummy4, form.Grid.SelectedElements[0]);
				AssertEquals(2, rowFinder.matchingRows.Count);
			}
		}

		public void TestSearch_NotIncludeIsSensitiveValue()
		{
			var bizo = Factory.New<DummyBusinessObjectForRowFinderTest>();

			using (var form = new TestFormForGridRowFinder(bizo))
			{
				var sensitiveColInfo = new ZTextBoxColumnStyleInfo { ColumnName = "Z0_NVarChar", Caption = "SensitiveValue", PasswordChar = '*' };
				AssertEquals(true, sensitiveColInfo.IsSensitiveValue);

				form.Grid.ColumnStyles.Add(sensitiveColInfo);
				form.Show();

				var rowFinderBizo = new GridRowFinderBusinessObject(form.Grid);

				AssertEquals(4, rowFinderBizo.ColumnsToSearch.Count);

				AssertEquals("Test", rowFinderBizo.ColumnsToSearch[0].Description);
				AssertEquals(true, rowFinderBizo.ColumnsToSearch[0].Value);

				AssertEquals("Test2", rowFinderBizo.ColumnsToSearch[1].Description);
				AssertEquals(true, rowFinderBizo.ColumnsToSearch[1].Value);

				AssertEquals("MultilingualTest", rowFinderBizo.ColumnsToSearch[2].Description);
				AssertEquals(true, rowFinderBizo.ColumnsToSearch[2].Value);

				AssertEquals("LOL", rowFinderBizo.ColumnsToSearch[3].Description);
				AssertEquals(true, rowFinderBizo.ColumnsToSearch[3].Value);
			}
		}

		public void TestSearch_SensitiveValueNotSearchable()
		{
			var bizo = Factory.New<DummyBusinessObjectForRowFinderTest>();

			using (var form = new TestFormForGridRowFinder(bizo))
			{
				var sensitiveColInfo = new ZTextBoxColumnStyleInfo { ColumnName = "Z0_NVarChar", Caption = "SensitiveValue", PasswordChar = '*' };
				AssertEquals(true, sensitiveColInfo.IsSensitiveValue);

				bizo.Dummy1.Z0_NVarChar = "Password1";
				bizo.Dummy2.Z0_NVarChar = "Password2";
				Factory.Save();

				form.Grid.ColumnStyles.Add(sensitiveColInfo);
				form.Show();

				var rowFinderBizo = new GridRowFinderBusinessObject(form.Grid);

				AssertEquals(4, rowFinderBizo.ColumnsToSearch.Count);

				rowFinderBizo.TextToSearchFor = "Pass";
				rowFinderBizo.DoSearch();

				AssertEquals(0, form.Grid.SelectedElements.Length);
				AssertEquals(0, rowFinderBizo.matchingRows.Count);
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var bizo = Factory.New<DummyBusinessObjectForRowFinderTest>();
			using (var form = new TestFormForGridRowFinder(bizo))
			{
				form.Show();
				return new GridRowFinderBusinessObject(form.Grid);
			}
		}

		#endregion
	}
}
