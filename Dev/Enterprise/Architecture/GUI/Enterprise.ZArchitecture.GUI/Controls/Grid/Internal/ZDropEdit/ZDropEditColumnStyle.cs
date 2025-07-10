using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	#region ZDropEditColumnStyleInfo

	public class ZDropEditColumnStyleInfo : ZTextBoxColumnStyleInfo, IBindToList
	{
		public ZDropEditColumnStyleInfo(string columnName, int width, int maxDropDownItems) : base(columnName, width)
		{
			this.MaxDropDownItems = maxDropDownItems;
			BindToList = "";
		}

		public ZDropEditColumnStyleInfo(string columnName, int width) : this(columnName, width, DefaultMaxDropDownItems) { }

		public ZDropEditColumnStyleInfo() { } // required for ZGrid column designer

		[ZColumnBindingMemberType(typeof(ZString))]
		public override string ColumnName
		{
			get { return base.ColumnName; }
			set { base.ColumnName = value; }
		}

		#region BindToList

		[ZColumnBindingMemberType(typeof(IList))]
		[DefaultValue("")]
		public virtual string BindToList
		{
			get { return fBindToList; }
			set { fBindToList = value; }
		}
		protected string fBindToList = "";

		#endregion

		#region ShowInDropDown

		[DefaultValue(ZDropEdit.ShowInDropDownList.ShowCodeAndDescription)]
		public virtual ZDropEdit.ShowInDropDownList ShowInDropDown
		{
			get { return showInDropDown; }
			set { showInDropDown = value; }
		}
		ZDropEdit.ShowInDropDownList showInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;

		#endregion

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZDropEditColumnStyle); }
		}

		#region MaxDropDownItems

		protected const int DefaultMaxDropDownItems = 10;
		protected int fMaxDropDownItems = DefaultMaxDropDownItems;

		[DefaultValue(DefaultMaxDropDownItems)]
		public int MaxDropDownItems
		{
			get
			{
				return fMaxDropDownItems;
			}
			set
			{
				fMaxDropDownItems = value;
			}
		}

		#endregion

		[DefaultValue(false)]
		public bool ShowHorizontalScrollBar { get; set; }
	}

	#endregion

	public class ZDropEditColumnStyle : ZCustomControlColumnStyle, IListColumnStyle
	{
		public ZDropEditColumnStyle(ZDropEditColumnStyleInfo columnInfo) : this(columnInfo, () => new ZGridDropEdit())
		{
		}

		public ZDropEditColumnStyle(ZDropEditColumnStyleInfo columnInfo, Func<Control> editControl) : base(editControl, columnInfo)
		{
			this.columnInfo = columnInfo;
		}
		readonly ZDropEditColumnStyleInfo columnInfo;

		#region INavigatingGridColumn Support

		protected override bool ShouldColumnHandleKey(Keys keyData)
		{
			return base.ShouldColumnHandleKey(keyData) || GridControl.ShouldHandleKey(keyData);
		}

		#endregion

		#region IListColumnStyle Members

		CurrencyManager IListColumnStyle.ListManager
		{
			get { return parentZGrid.ListManager; }
		}

		#endregion

		#region Implementation

		internal protected ZDropEdit DropEdit => (ZDropEdit)EditControl;

		protected override void OnInit(Control control)
		{
			var dropEdit = (ZDropEdit)control;
			base.OnInit(dropEdit);
			((IGridDropEdit)dropEdit).ColumnStyle = this;
			dropEdit.DataPropertyName = columnInfo.ColumnName;
			dropEdit.BindToList = columnInfo.BindToList;
			dropEdit.CharacterCasing = columnInfo.CharacterCasing;
			dropEdit.ShowDescriptionBox = false;
			dropEdit.ShowInDropDown = columnInfo.ShowInDropDown;
			dropEdit.ShowHorizontalScrollBar = columnInfo.ShowHorizontalScrollBar;
			dropEdit.TabStop = false;
			dropEdit.MaxItemsToShowInDropDown = columnInfo.MaxDropDownItems;
			dropEdit.MaxLength = columnInfo.MaxLengthOverride;
		}

		protected override object EditValue
		{
			get { return new ZString(GridControl.Text); }
		}

		protected override void PrepareEditControl(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly)
		{
			base.PrepareEditControl(source, rowNum, bounds, readOnly);
			if (!IsEditing)
			{
				SetCurrentText(source, rowNum);
			}
		}

		protected virtual void SetCurrentText(CurrencyManager source, int rowNum)
		{
			GridControl.Text = GetColumnValueAtRow(source, rowNum).ToString();
		}
#if DEBUG
		internal
#endif
		protected override void InitialiseEditControlForNewPosition(BusinessObject bizObj, bool readOnly)
		{
			base.InitialiseEditControlForNewPosition(bizObj, readOnly);
			DropEdit.CurrentItem = bizObj;
			DropEdit.DataPropertyName = MappingName;
			DropEdit.InvalidateList();
		}

		protected override void ColumnTextBoxChanged(object sender, EventArgs e)
		{
			if (!IsCurrentCellReadOnly)
			{
				if (!DropEdit.DropButton.IsDroppedDown)
				{
#if WINZOR
					isEditRequired = false;
#endif
					base.ColumnTextBoxChanged(sender, e);
#if WINZOR
					isEditRequired = true;
#endif
				}
				else
				{
					DropEdit.DropDownClosed += DropEdit_DropDownClosed;
				}
			}
		}

		void DropEdit_DropDownClosed(object sender, EventArgs e)
		{
			base.ColumnTextBoxChanged(sender, e);
			DropEdit.DropDownClosed -= DropEdit_DropDownClosed;
		}
#if DEBUG
		internal
#endif
		protected override void SetEmailCore(string emailAddress)
		{
			DropEdit.Text = emailAddress;
		}

		#endregion
	}
}
