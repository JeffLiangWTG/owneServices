using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public abstract class ZBaseFindBoxColumnStyle : ZCustomControlColumnStyle
	{
		protected ZBaseFindBoxColumnStyle(Func<ZFindBoxUserControl> findBox, ZBaseFindBoxColumnStyleInfo columnInfo)
			: base(findBox, columnInfo)
		{
			this.columnInfo = columnInfo;
		}
		readonly ZBaseFindBoxColumnStyleInfo columnInfo;

		#region Implementation

		protected override void OnInit(Control control)
		{
			base.OnInit(control);
			var zGridFindBox = (ZGridFindBox)control;
			zGridFindBox.BindToList = columnInfo.BindToList;
			zGridFindBox.ShowNewFormWhenEmpty = columnInfo.ShowNewFormWhenEmpty;
			zGridFindBox.AllowNewForm = columnInfo.AllowNewForm;
			var codeBox = zGridFindBox.CodeBox;
			if (codeBox != null)
			{
				codeBox.CharacterCasing = columnInfo.CharacterCasing;
			}
		}

		protected internal IFindBox FindBox => (IFindBox)EditControl;

		protected override object EditValue
		{
			get { return new ZString(FindBox.Code); }
		}

		protected override void PrepareEditControl(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly)
		{
			base.PrepareEditControl(source, rowNum, bounds, readOnly);

			if (!IsEditing && rowNum < source.Count)
			{
				SetValueInFindBox(source, rowNum);
			}
		}

		protected virtual void SetValueInFindBox(CurrencyManager source, int rowNum)
		{
			FindBox.Code = GetColumnValueAtRow(source, rowNum).ToString();
		}

		protected override void ActivateEditControl()
		{
			((ZFindBoxUserControl)EditControl).CodeBox.Focus();
		}

		#endregion
	}

	#region ZBaseFindBoxColumnStyleInfo

	public abstract class ZBaseFindBoxColumnStyleInfo : ZTextBoxColumnStyleInfo, IBindToList
	{
		protected ZBaseFindBoxColumnStyleInfo()
		{
			ShowNewFormWhenEmpty = true;
			AllowNewForm = true;
			CharacterCasing = CharacterCasing.Upper;
		}

		[DefaultValue(true)]
		public bool ShowNewFormWhenEmpty { get; set; }

		[DefaultValue(true)]
		public bool AllowNewForm { get; set; }

		[DefaultValue(CharacterCasing.Upper)]
		public override CharacterCasing CharacterCasing
		{
			get { return base.CharacterCasing; }
			set { base.CharacterCasing = value; }
		}

		#region BindToList

		protected string fBindToList = "";

		[DefaultValue("")]
		[ZColumnBindingMemberType(typeof(IList))]
		public string BindToList
		{
			get { return fBindToList; }
			set { fBindToList = value; }
		}

		#endregion

		#region PopupCaption

		protected string fPopupCaption = "";

		[DefaultValue("")]
		public string PopupCaption
		{
			get { return fPopupCaption; }
			set { fPopupCaption = value; }
		}

		#endregion
	}

	#endregion
}
