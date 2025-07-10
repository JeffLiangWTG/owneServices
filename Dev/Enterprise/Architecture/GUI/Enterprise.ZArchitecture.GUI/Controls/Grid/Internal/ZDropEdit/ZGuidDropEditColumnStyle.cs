using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI.Controls;
using Enterprise.ZArchitecture.GUI.Grid.Internal;
using Enterprise.ZArchitecture.GUI.Internal;

#if WINZOR
using System.Net;
using System.Web;
using Microsoft.AspNetCore.Components;
#endif

namespace Enterprise.ZArchitecture.GUI
{
	#region ZGuidDropEditColumnStyleInfo

	public class ZGuidDropEditColumnStyleInfo : ZDropEditColumnStyleInfo, IMultiTypeColumnStyleInfo
	{
		[ZColumnBindingMemberType(typeof(ZGuid))]
		public override string ColumnName
		{
			get { return base.ColumnName; }
			set { base.ColumnName = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZGuidDropEditColumnStyle); }
		}

		internal ZGuid ParseCode(string code)
		{
			if (Parse != null)
			{
				return Parse(code);
			}
			return ZGuid.Invalid;
		}

		/// <summary>
		/// Parsing the string code into a guid pk
		/// </summary>
		public Converter<ZString, ZGuid> Parse;
	}

	#endregion

	public class ZGuidDropEditColumnStyle : ZDropEditColumnStyle
	{
		public ZGuidDropEditColumnStyle(ZGuidDropEditColumnStyleInfo columnInfo)
			: this(columnInfo, () => new ZGridGuidDropEdit())
		{
		}

		public ZGuidDropEditColumnStyle(ZGuidDropEditColumnStyleInfo columnInfo, Func<ZGridGuidDropEdit> gridGuidDropEdit)
			: base(columnInfo, gridGuidDropEdit)
		{
			this.columnInfo = columnInfo;
		}

		readonly ZGuidDropEditColumnStyleInfo columnInfo;

		protected override void OnInit(Control control)
		{
			base.OnInit(control);
			var dropEdit = (ZGridGuidDropEdit)control;
			dropEdit.DataPropertyName = columnInfo.ColumnName;
			dropEdit.BindToList = columnInfo.BindToList;
			dropEdit.LastSelectedItemChanged += GridGuidDropEdit_LastSelectedItemChanged;
			initialised = true;
		}

		void GridGuidDropEdit_LastSelectedItemChanged(object sender, EventArgs e)
		{
			ColumnTextBoxChanged(null, null);
		}

		bool initialised;

		protected override void Dispose(bool disposing)
		{
			if (initialised)
			{
				DropEdit.LastSelectedItemChanged -= GridGuidDropEdit_LastSelectedItemChanged;
			}
			base.Dispose(disposing);
		}

		protected override void PrepareEditControl(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly)
		{
			base.PrepareEditControl(source, rowNum, bounds, readOnly);
			if (!IsEditing)
			{
				var guidValue = GetColumnGuidValue(source, rowNum);
				var dropEditList = DropEdit.List;

				// WI00204623, WI00450647
				if (dropEditList == null)
				{
					ErrorReporter.ReportOnce(FormattableString.Invariant($"{DropEdit.Name}_DropEditListIsNull"), FormattableString.Invariant(
						   $"Control is:\r\n{ControlDescription.GetControlPath(DropEdit)}\r\nrowNum: {rowNum}\r\nsource.Count: {source?.Count.ToString() ?? "null"}\r\nDropEdit.CurrentItem != null: {DropEdit.CurrentItem != null}"));
				}
				else
				{
					var selectedItem = dropEditList.OfType<ICodeDescription>().FirstOrDefault(x => x.PK.Equals(guidValue));

					if (selectedItem != null)
					{
						DropEdit.SetSelection(selectedItem, 0);
					}
				}
			}
		}

		protected override bool Commit(CurrencyManager source, int rowNum)
		{
			if (IsEditing)
			{
				if (source != null && source.Position == rowNum)
				{
					var existingValue = GetColumnGuidValue(source, rowNum);
					var currentEditValue = GetPK(source, rowNum, EditValue.ToString());

					if (existingValue != currentEditValue)
					{
						SetColumnValueAtRow(source, rowNum, currentEditValue);
					}

					if (!currentEditValue.IsValid && !currentEditValue.IsEmpty)
					{
						SaveInvalidCodeOnInfo(source, rowNum, EditValue.ToString());
					}
				}

				IsEditing = false;
			}
			else
			{
				HideEditControl();
			}

			return true;
		}

		protected override void SetCurrentText(CurrencyManager source, int rowNum)
		{
			GridControl.Text = ColumnTextAtRow(source, rowNum);
		}

		#region Painting

		protected string CurrentCellText;

#if !WINZOR

		protected
#if DEBUG
		internal
#endif
		override void PaintText(Graphics graphics, Rectangle bounds, CurrencyManager source, int rowNum, string cellText, Font cellFont, Brush backBrush, Brush foreBrush, bool isRightToLeft)
		{
			cellText = CurrentCellText;
			base.PaintText(graphics, bounds, source, rowNum, cellText, cellFont, backBrush, foreBrush, isRightToLeft);
		}

		protected override void Paint(Graphics rraphics, Rectangle bounds, CurrencyManager source, int paintingRowNum, Brush backBrush, Brush foreBrush, bool isAlignedToRight)
		{
			CurrentCellText = GetCode(source, paintingRowNum);
			base.Paint(rraphics, bounds, source, paintingRowNum, backBrush, foreBrush, isAlignedToRight);
		}
#else
		protected override MarkupString GetRenderContent(CurrencyManager source, int rowNum, bool alignToRight)
		{
			CurrentCellText = GetCode(source, rowNum);
			return (MarkupString)WebUtility.HtmlEncode(CurrentCellText);
		}
#endif

		#endregion

		#region Get Code / PK

		protected string GetCode(CurrencyManager source, int rowNum)
		{
			return source != null && rowNum > -1 && rowNum < source.List.Count
				? GetCode(source.List[rowNum], GetColumnGuidValue(source, rowNum))
				: string.Empty;
		}

		protected string GetCode(object rowObject, ZGuid pk)
		{
			if (pk.IsValid)
			{
				var element = GetList(rowObject).FirstOrDefault(e => Equals(e.PK, pk));

				return element != null ? element.Code : string.Empty;
			}
			else if (pk.IsEmpty)
			{
				return string.Empty;
			}
			else
			{
				return GetInvalidCodeFromInfo(rowObject);
			}
		}

		internal protected ZGuid GetPK(CurrencyManager source, int rowNum, string code)
		{
			var result = ZGuid.Empty;

			var sourceList = (source == null) ? null : source.List;
			if (!string.IsNullOrEmpty(code) && sourceList != null && rowNum > -1 && rowNum < sourceList.Count)
			{
				var list = GetList(sourceList[rowNum]);
				result = ((ZGuidDropEdit)DropEdit).GetPK(code, list);

				if (result.IsValid)
				{
					return result;
				}

				var guidColumnInfo = ColumnInfo as ZGuidDropEditColumnStyleInfo;
				if (guidColumnInfo != null)
				{
					return guidColumnInfo.ParseCode(code);
				}

				result = ZGuid.Invalid;
			}

			return result;
		}

#if DEBUG
		protected virtual
#endif
 IEnumerable<ICodeDescription> GetList(object valueAtRow)
		{
			IEnumerable result = null;
			if (DropEdit != null)
			{
				var previousItem = DropEdit.CurrentItem;
				var previousList = DropEdit.List;

				try
				{
					DropEdit.CurrentItem = valueAtRow;
					DropEdit.PullList();
					result = DropEdit.List;
				}
				finally
				{
					DropEdit.CurrentItem = previousItem;
					DropEdit.List = previousList;
				}
			}

			return result == null ? Enumerable.Empty<ICodeDescription>() : result.OfType<ICodeDescription>();
		}

		protected override string FormatValueObjectCore(object source, object value)
		{
			//May be the code itself or the CodeDescription Pk
			var stringValue = (value is ZGuid || value is Guid)
				? GetCode(source, new ZGuid(value))
				: value.ToString();

			return base.FormatValueObjectCore(source, stringValue);
		}

		protected override object GetColumnValueAtRow(CurrencyManager source, int rowNum)
		{
			return GetCode(source, rowNum);
		}

		protected override object GetEmptyColumnValue()
		{
			return ZGuid.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
		internal ZGuid GetColumnGuidValue(CurrencyManager source, int rowNum)
		{
			var value = base.GetColumnValueAtRow(source, rowNum);

			if (!(value is ZGuid))
			{
				ErrorReporter.ReportOnce(
					string.Format("Expected value [{0}] for ColumnName [{1}] to be a ZGuid{2}.\r\nsource.Position = {3}, source.Count = {4}, rowNum = {5}." +
							"\r\nPropertyDescriptor Type: {6}, Name: {7}",
						value,
						ColumnInfo.ColumnName,
						value != null ? " but it was a " + value.GetType().FullName : "",
						source.Position,
						source.Count,
						rowNum,
						PropertyDescriptor != null ? PropertyDescriptor.GetType().FullName : "<null>",
						PropertyDescriptor != null ? PropertyDescriptor.Name : ""
					));

				return ZGuid.Empty;
			}

			return (ZGuid)value;
		}

		#endregion
	}
}
