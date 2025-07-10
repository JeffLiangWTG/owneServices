using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	public static class SupportingDocumentsControlTestHelper
	{
		public static ZGroupBox GetFieldsGroupBox(BaseCustomsEntryUserControl supportingDocumentsFieldControl) => supportingDocumentsFieldControl.FindSingleOrDefault<ZGroupBox>();

		public static void AssertBindingSource(Type controlType, Type bindingSourceType)
		{
			using (var control = (BaseCustomsEntryUserControl)Activator.CreateInstance(controlType))
			{
				TestCaseWithFactory.AssertEquals(bindingSourceType, control.BindingSource.DataSourceType);
			}
		}

		public static void AssertGroupBox(Type supportingDocumentsFieldControlType, bool testForCommonCaptionAsResourceString = true, string expectedCaption = "[44] Supporting Documents")
		{
			using (var supportingDocumentsFieldControl = (BaseCustomsEntryUserControl)Activator.CreateInstance(supportingDocumentsFieldControlType))
			{
				var groupBox = GetFieldsGroupBox(supportingDocumentsFieldControl);
				AssertionWithHtml.CombineAssertions(() =>
				{
					TestCaseWithFactory.AssertEquals("group box: DockStyle is fill", DockStyle.Fill, groupBox.Dock);
					if (testForCommonCaptionAsResourceString)
					{
						TestCaseWithFactory.AssertEquals("caption set by resource string", expectedCaption, groupBox.CaptionResourceString.Caption);
					}
				});
			}
		}

		public static void AssertGridColumns(BusinessObjectFactory factory, Type supportingDocumentsUserControlType, IEnumerable<(string ColumnName, Type ColumnType)> orderedColumnNamesAndColumnStyleTypes)
		{
			var supportingDocument = factory.New<SupportingDocument>();
			var collection = new SupportingDocumentCollection(supportingDocument);
			using (var supportingDocumentsUserControl = (SupportingDocumentsUserControl)Activator.CreateInstance(supportingDocumentsUserControlType))
			{
				AssertGridColumns(supportingDocumentsUserControl, orderedColumnNamesAndColumnStyleTypes, collection);
			}
		}

		public static void AssertGridColumns(SupportingDocumentsUserControl supportingDocumentsUserControl,
			IEnumerable<(string ColumnName, Type ColumnType)> orderedColumnNamesAndColumnStyleTypes, SupportingDocumentCollection collection)
		{
			var grid = supportingDocumentsUserControl.SupportingDocumentsGrid;
			grid.SetDataBinding(collection, "");
			supportingDocumentsUserControl.Show();

			AssertionWithHtml.CombineAssertions(() =>
			{
				TestCaseWithFactory.AssertEquals("columns count", orderedColumnNamesAndColumnStyleTypes.Count(), grid.Columns.Count);

				var indexCounter = 0;
				foreach (var (columnName, columnType) in orderedColumnNamesAndColumnStyleTypes)
				{
					var column = grid.Columns.SingleOrDefault(x => x.ColumnName == columnName);
					if (column == null)
					{
						TestCaseWithFactory.Assert($"Column: {columnName} does not exists", false);
					}
					else
					{
						var actualColumnStyleType = column.ColumnStyle.GetType();
						TestCaseWithFactory.AssertEquals($"Column: {columnName}: exists at expected position '{indexCounter}'", columnName,
							grid.Columns[indexCounter].ColumnName);
						TestCaseWithFactory.AssertEquals($"Column: {columnName}: columnStyleType", columnType, actualColumnStyleType);
						TestCaseWithFactory.Assert($"Column: {columnName}: visible", column.IsVisible);
					}

					indexCounter++;
				}
			});
		}

		public static void AssertFields(Type supportingDocumentsFieldControlType, IEnumerable<(string ControlName, int TabIndex, Type ControlType)> fieldsDetails)
		{
			using (var supportingDocumentsFieldControl = (BaseCustomsEntryUserControl)Activator.CreateInstance(supportingDocumentsFieldControlType))
			{
				var groupBox = GetFieldsGroupBox(supportingDocumentsFieldControl);
				AssertionWithHtml.CombineAssertions(() =>
				{
					foreach (var (controlName, tabIndex, controlType) in fieldsDetails)
					{
						var actualfield = groupBox.FindSingleOrDefault<Control>(x => x.Name == controlName);
						if (actualfield == null)
						{
							TestCaseWithFactory.Assert($"Control: {controlName} does not exists", false);
						}
						else
						{
							TestCaseWithFactory.AssertEquals($"{controlName}: tab index", tabIndex, actualfield.TabIndex);
							TestCaseWithFactory.AssertEquals($"{controlName}: type", controlType, actualfield.GetType());
						}
					}
				});
			}
		}

		public static void AssertSupportingDocumentsFieldsControlType(Type supportingDocumentsUserControlType, Type expectedFieldsControlType)
		{
			using (var supportingDocumentsUserControl = (SupportingDocumentsUserControl)Activator.CreateInstance(supportingDocumentsUserControlType))
			{
				TestCaseWithFactory.AssertEquals(expectedFieldsControlType, supportingDocumentsUserControl.SupportingDocumentsFieldsControl.GetType());
			}
		}

		public static void AssertCaptionRenderingEnabledForUserControlAndFieldsControl(Type supportingDocumentsUserControlType)
		{
			using (var supportingDocumentsUserControl = (SupportingDocumentsUserControl)Activator.CreateInstance(supportingDocumentsUserControlType))
			{
				var supportingDocumentsFieldsControl = supportingDocumentsUserControl.SupportingDocumentsFieldsControl;
				var controlCaptionRenderingEnabled = supportingDocumentsUserControl.CaptionRenderingEnabled ?? false;
				var fieldsControlCaptionRenderingEnabled = supportingDocumentsFieldsControl.CaptionRenderingEnabled ?? false;
				AssertionWithHtml.CombineAssertions(() =>
				{
					TestCaseWithFactory.AssertEquals(supportingDocumentsUserControlType.FullName, true, controlCaptionRenderingEnabled);
					TestCaseWithFactory.AssertEquals(supportingDocumentsFieldsControl.GetType().FullName, true, fieldsControlCaptionRenderingEnabled);
				});
			}
		}
	}
}
