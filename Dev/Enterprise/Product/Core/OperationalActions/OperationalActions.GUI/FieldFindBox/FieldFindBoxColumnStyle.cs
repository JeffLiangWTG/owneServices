using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Services.OperationalActions.GUI
{
	internal sealed partial class FieldFindBoxColumnStyle : ZCustomControlColumnStyle
	{
		public FieldFindBoxColumnStyle(FieldFindBoxColumnStyleInfo columnInfo)
			: base(() => new FieldFindBoxGridControl(), columnInfo)
		{
			FormatInfo = null;
		}

		protected override void OnInit(Control control)
		{
			var findBoxControl = (FieldFindBoxGridControl)control;
			base.OnInit(findBoxControl);
			findBoxControl.IsOnGrid = true;
			findBoxControl.ReadOnly = false;
			findBoxControl.AllowReadOnly = false;
		}

		protected override void HookControlEvents()
		{
			base.HookControlEvents();
			Control.FieldNameChanged += ColumnTextBoxChanged;
		}

		protected override void UnHookControlEvents()
		{
			base.UnHookControlEvents();
			Control.FieldNameChanged -= ColumnTextBoxChanged;
		}

		protected override object EditValue
		{
			get { return Control.FieldName; }
		}

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			if (!IsEditing)
			{
				OperationalActionFieldDescriptor descriptor = source.List[rowNum] as OperationalActionFieldDescriptor;

				Control.FieldName = (ZString)GetColumnValueAtRow(source, rowNum);
				Control.RootType = descriptor == null ? null : descriptor.Context.Supporter.RootType;
				Control.WorkflowType = descriptor?.Context?.WorkflowType;
			}

			base.Edit(source, rowNum, bounds, readOnly, instantText, cellIsVisible);
		}

		protected override bool ShouldColumnHandleKey(Keys keyData)
		{
			return
				keyData == Keys.Tab || keyData == (Keys.Shift | Keys.Tab) ||
				base.ShouldColumnHandleKey(keyData);
		}

		FieldFindBoxGridControl Control => (FieldFindBoxGridControl)EditControl;
	}
}
