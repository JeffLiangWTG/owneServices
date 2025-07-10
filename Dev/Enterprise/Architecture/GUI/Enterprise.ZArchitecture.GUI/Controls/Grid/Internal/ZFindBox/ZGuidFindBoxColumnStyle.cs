using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Grid.Internal;
using Enterprise.ZArchitecture.GUI.Internal;
#if WINZOR
using Graphics = System.Drawing.BGraphics;
#endif

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZGuidFindBoxColumnStyle : ZCodeFindBoxColumnStyle
	{
		public ZGuidFindBoxColumnStyle(ZGuidFindBoxColumnStyleInfo columnInfo)
			: base(() => new ZGridGuidFindBox(), columnInfo)
		{
		}

		protected ZGuidFindBoxColumnStyle(Func<ZGridGuidFindBox> gridGuidFindBox, ZGuidFindBoxColumnStyleInfo columnInfo)
			: base(gridGuidFindBox, columnInfo)
		{
		}

		internal ZGridGuidFindBox GuidFindBox
		{
			get { return (ZGridGuidFindBox)base.FindBox; }
		}

		protected override void HookControlEvents()
		{
			base.HookControlEvents();
			GuidFindBox.PopupSelected += GuidFindBox_PopupSelected;
		}

		protected override void UnHookControlEvents()
		{
			GuidFindBox.PopupSelected -= GuidFindBox_PopupSelected;
			base.UnHookControlEvents();
		}
#if DEBUG
		internal
#endif
		protected override void InitialiseEditControlForNewPosition(BusinessObject bizObj, bool readOnly)
		{
			base.InitialiseEditControlForNewPosition(bizObj, readOnly);
			maxLengthCache = null;
		}

		void GuidFindBox_PopupSelected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			IsEditing = true;
		}

		#region Implementation

		protected override object EditValue
		{
			get { return GuidFindBox.Guid; }
		}

		bool initialised;

		#region Test stuff
#if DEBUG
		internal void ResetInitializedForTest()
		{
			initialised = false;
		}
#endif
		#endregion

		protected override object GetColumnValueAtRow(CurrencyManager source, int rowNum)
		{
			object result = "";
			if (initialised) // prevents DataGridTextBox displaying GUID
			{
				result = base.GetColumnValueAtRow(source, rowNum);
			}

			return result;
		}

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			initialised = false;
			base.Edit(source, rowNum, bounds, readOnly, instantText, cellIsVisible);

			readOnly = IsCellReadOnly(source, rowNum);
			if (readOnly && !(parentDataGrid is ZDisplayGrid))
			{
				InitialiseEditControlForNewPositionWithCurrent(source, readOnly);
				TextBox.Text = ColumnTextAtRow(source, rowNum);
				TextBox.SelectAll();
			}
		}

		protected override void UpdateUI(CurrencyManager source, int rowNum, string displayText)
		{
			TextBox.Text = ColumnTextAtRow(source, rowNum);
		}

		protected override void SetColumnValueAtRow(CurrencyManager source, int rowNum, object value)
		{
			if (!((ZGuid)value).IsValid)
			{
				FieldInvalidTextMemory.SetInvalidText(source.GetCurrent(), PropertyDescriptor.Name, FindBox.Code);
			}

			base.SetColumnValueAtRow(source, rowNum, value);
		}

		protected override bool ShouldSetValue(object currentEditValue, CurrencyManager source, int rowNum)
		{
			return base.ShouldSetValue(currentEditValue, source, rowNum) ||
				(!((ZGuid)currentEditValue).IsValid && FieldInvalidTextMemory.GetInvalidText(source.List[rowNum], MappingName) != FindBox.Code);
		}

		protected override void SetValueInFindBox(CurrencyManager source, int rowNum)
		{
			FindBox.Code = ColumnTextAtRow(source, rowNum);
		}

		protected override void OnGettingColumnTextAtRow(CurrencyManager source, int rowNum)
		{
			base.OnGettingColumnTextAtRow(source, rowNum);
			initialised = true;
		}

		protected override string FormatValueObjectCore(object source, object propertyValue)
		{
			if (propertyValue is ZGuid)
			{
				var value = (ZGuid)propertyValue;
				if (value.IsEmpty)
				{
					return String.Empty;
				}
				else if (value.IsValid)
				{
					((ZGridFindBox)EditControl).CurrentItem = source;
					((ZGridFindBox)EditControl).DataPropertyName = MappingName;
					((ZGridFindBox)EditControl).PullList();

					return FindBox.ListProvider.CodeFromPrimaryKey(value);
				}
				else
				{
					return FieldInvalidTextMemory.GetInvalidText(source, PropertyDescriptor.Name);
				}
			}
			else if (propertyValue == null || Equals(propertyValue, String.Empty))
			{
				return String.Empty;
			}
			else
			{
				ErrorReporter.ReportOnce("ColumnTextAtRow" + MappingName + "not ZGuid", "Not a ZGuid, column " + MappingName + ", value " + propertyValue.ToString() + ", Type = " + propertyValue.GetType().Name);
				return String.Empty;
			}
		}

#if !WINZOR

		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int paintingRowNum, Brush backBrush, Brush foreBrush, bool alignedToRight)
		{
			var oldCurrentItem = ((ZFindBoxUserControl)FindBox).CurrentItem;
			var oldList = ((ZFindBoxUserControl)FindBox).List;
			try
			{
				if (source.List != null && source.List.Count > paintingRowNum)
				{
					((ZFindBoxUserControl)FindBox).CurrentItem = source.List[paintingRowNum];
					((ZFindBoxUserControl)FindBox).DataPropertyName = MappingName;
					((ZFindBoxUserControl)FindBox).PullList();
				}
				base.Paint(g, bounds, source, paintingRowNum, backBrush, foreBrush, alignedToRight);
			}
			finally
			{
				((ZFindBoxUserControl)FindBox).List = oldList;
				((ZFindBoxUserControl)FindBox).CurrentItem = oldCurrentItem;
			}
		}

#endif

		protected override Size GetPreferredSize(Graphics g, object value)
		{
			if (parentZGrid.ListManager != null && parentZGrid.ListManager.Count > 0)
			{
				var bizObj = parentZGrid.ListManager.List[0] as BusinessObject;
				if (bizObj != null)
				{
					var maxLength = GetMaxLength(bizObj);
					if (maxLength > 0)
					{
						return base.GetPreferredSize(g, new string('B', maxLength)); // W is too large, doesn't represent a sensible average.
					}
				}
			}
			return base.GetPreferredSize(g, value);
		}
#if DEBUG
		internal
#endif
		protected override int GetMaxLength(BusinessObject bizObj)
		{
			var result = -1;
			if (!MaxLengthCache.TryGetValue(bizObj.PK, out result))
			{
				var findBox = FindBox as ZGridFindBox;
				if (findBox != null)
				{
					var oldCurrentItem = findBox.CurrentItem;
					var oldList = findBox.List;
					try
					{
						findBox.CurrentItem = bizObj;
						findBox.DataPropertyName = MappingName;
						findBox.PullList();
						result = ZGuidFindBox.GetMaxLengthFromListProvider(FindBox.ListProvider);
						MaxLengthCache[bizObj.PK] = result;
					}
					finally
					{
						findBox.CurrentItem = oldCurrentItem;
						findBox.List = oldList;
					}
				}
			}
			return result;
		}

		Dictionary<ZGuid, int> MaxLengthCache
		{
			get
			{
				if (maxLengthCache == null)
				{
					maxLengthCache = new Dictionary<ZGuid, int>();
				}

				return maxLengthCache;
			}
		}

		Dictionary<ZGuid, int> maxLengthCache;

		#endregion
	}

	public class ZGuidFindBoxColumnStyleInfo : ZCodeFindBoxColumnStyleInfo, IMultiTypeColumnStyleInfo
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
			get { return typeof(ZGuidFindBoxColumnStyle); }
		}
	}
}
