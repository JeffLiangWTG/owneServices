using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class MultipleSelectionLookupUserControl : RuntimeOptionUserControl
	{
		public MultipleSelectionLookupUserControl()
		{
			InitializeComponent();
#if DEBUG
			TypeDescriptor.AddAttributes(FieldLabel, new SuppressFormsLocalizedTestAttribute());
#endif

			Notification.AllowOverlap(Grid);
		}

		protected override int DesiredCaptionWidthCore
			=> FieldLabel.Width;

		protected override void ChangeLabelSizeForAlignmentCore(int descriptionSize)
		{
			ControlDpiScalingHelper.SetWidth(FieldLabel, descriptionSize - FieldLabel.Left, false);
			ControlDpiScalingHelper.SetWidth(Grid, Width - FieldLabel.Right, false);
			ControlDpiScalingHelper.SetLeft(Grid, FieldLabel.Right, false);
		}

		public override Type ExpectedFilterType()
		{
			return typeof(MultipleSelectionLookup);
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);
			LookupFilterFieldBase multiSelectFilter = (LookupFilterFieldBase)filter;
			FieldLabel.GetExtension<ILabelCaptionRenderer>().Caption = EscapeMnemonics(filter.DisplayNameLocalized);
			Grid.BindToFindBoxList = "BindToFindBoxList";
			Grid.BindToGridList = "BindToList";
			Grid.ModuleID = multiSelectFilter.ModuleID;
			Grid.InnerGrid.ReadOnly = true;

			SetupGridColumns(multiSelectFilter);
			SetDataBinding(filter, "");

			Notification.FilterField = filter;
			multiSelectFilter.BindToList.ListChanged -= new ListChangedEventHandler(BindToList_ListChanged);
			multiSelectFilter.BindToList.ListChanged += new ListChangedEventHandler(BindToList_ListChanged);
		}

		void BindToList_ListChanged(object sender, ListChangedEventArgs e)
		{
			FilterField filterField = DataSource as FilterField;
			if (filterField != null)
			{
				filterField.RunPreSaveValidation();
			}
		}

		void SetupGridColumns(LookupFilterFieldBase multiSelectFilter)
		{
			MultipleSelectionLookup multipleSelectionLookup = (MultipleSelectionLookup)multiSelectFilter;
			if (multipleSelectionLookup.Columns.Count == 0)
			{
				ZString codePropertyName = CodePropertyAttribute.CodePropertyNameFromType(multiSelectFilter.BindToList.TypeOfElements);
				ZString descriptionPropertyName = DescriptionPropertyAttribute.DescriptionPropertyNameFromType(multiSelectFilter.BindToList.TypeOfElements);

				if (!codePropertyName.IsEmpty)
				{
					((ZGridColumnInfo)Grid.ColumnStyles[0]).ColumnName = codePropertyName;
				}

				if (!descriptionPropertyName.IsEmpty)
				{
					((ZGridColumnInfo)Grid.ColumnStyles[1]).ColumnName = descriptionPropertyName;
				}

				if (codePropertyName == descriptionPropertyName)
				{
					if (Grid.ColumnStyles.Count > 1)
					{
						Grid.ColumnStyles.RemoveAt(1);
					}
				}
			}
			else
			{
				Grid.ColumnStyles.Clear();

				foreach (ColumnInfo columnInfo in multipleSelectionLookup.Columns)
				{
					if (columnInfo.ColumnName == null || columnInfo.ColumnName.Length > 0)
					{
						CreateColumn(columnInfo);
					}
					else
					{
						throw new TemplateDefinitionException("ColumnName property value should be specified", new CellReference());
					}
				}
			}
		}

		void CreateColumn(ColumnInfo columnInfo)
		{
			ZGridColumnInfo zGridColumnInfo = GetGridColumn(columnInfo.ColumnType);
			zGridColumnInfo.Caption = columnInfo.CaptionLocalized;
			zGridColumnInfo.ColumnName = columnInfo.ColumnName;
			foreach (KeyValuePair<string, string> propertyItem in columnInfo.Properties)
			{
				PropertyInfo property = typeof(ZGridColumnInfo).GetProperty(propertyItem.Key);
				if (property != null)
				{
					TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);
					property.SetValue(zGridColumnInfo, converter.ConvertFrom(propertyItem.Value), null);
				}
			}
			Grid.ColumnStyles.Add(zGridColumnInfo);
		}

		protected ZGridColumnInfo GetGridColumn(ColumnTypes columnType)
		{
			ZGridColumnInfo result = null;

			switch (columnType)
			{
				case ColumnTypes.ZCalcEditColumnStyleInfo:
					result = new ZCalcEditColumnStyleInfo();
					break;
				case ColumnTypes.ZCheckBoxColumnStyleInfo:
					result = new ZCheckBoxColumnStyleInfo();
					break;
				case ColumnTypes.ZCodeFindBoxColumnStyleInfo:
					result = new ZCodeFindBoxColumnStyleInfo();
					break;
				case ColumnTypes.ZDateEditColumnStyleInfo:
					result = new ZDateEditColumnStyleInfo();
					break;
				case ColumnTypes.ZDropEditColumnStyleInfo:
					result = new ZDropEditColumnStyleInfo();
					break;
				case ColumnTypes.ZGuidDropEditColumnStyleInfo:
					result = new ZGuidDropEditColumnStyleInfo();
					break;
				case ColumnTypes.ZGuidFindBoxColumnStyleInfo:
					result = new ZGuidFindBoxColumnStyleInfo();
					break;
				case ColumnTypes.ZMultiLineTextBoxColumnInfo:
					result = new ZMultiLineTextBoxColumnInfo();
					break;
				case ColumnTypes.ZOrganisationFindBoxColumnStyleInfo:
					result = new ZOrganisationFindBoxColumnStyleInfo();
					break;
				default:
					result = new ZTextBoxColumnStyleInfo();
					break;
			}
			return result;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				LookupFilterFieldBase lookupFilterField = DataSource as LookupFilterFieldBase;
				if (lookupFilterField != null)
				{
					lookupFilterField.BindToList.ListChanged -= new ListChangedEventHandler(BindToList_ListChanged);
				}
			}

			base.Dispose(disposing);
		}
	}
}
