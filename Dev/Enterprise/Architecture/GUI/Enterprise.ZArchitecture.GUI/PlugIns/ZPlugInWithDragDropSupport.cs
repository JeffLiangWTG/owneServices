using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.PlugIn
{
	public abstract class ZPlugInWithDragDropSupport : ZPlugIn
	{
		protected ZPlugInWithDragDropSupport(IBusiness hostBusinessEntity)
			: base(hostBusinessEntity)
		{
		}

		ZForm ParentForm;

		protected override void HookFormEventsCore()
		{
			base.HookFormEventsCore();
			if (TopLevelTabControl != null)
			{
				var form = TopLevelTabControl.FindForm();

				if (form != null && form is ZForm)
				{
					ParentForm = (ZForm)form;
					ParentForm.DragDrop += new DragEventHandler(ParentForm_DragDrop);
					ParentForm.DragOver += new DragEventHandler(ParentForm_DragOver);
					ParentForm.DataObjectPasted += ParentForm_DataObjectPasted;
				}
			}
		}

		protected override void UnHookFormEventsCore()
		{
			base.UnHookFormEventsCore();
			if (ParentForm != null)
			{
				ParentForm.DragOver -= new DragEventHandler(ParentForm_DragOver);
				ParentForm.DragDrop -= new DragEventHandler(ParentForm_DragDrop);
				ParentForm.DataObjectPasted -= ParentForm_DataObjectPasted;
			}
		}

		void ParentForm_DragOver(object sender, DragEventArgs e)
		{
			OnParentFormDragOver((ZForm)sender, e);
		}

		protected virtual void OnParentFormDragOver(ZForm form, DragEventArgs args)
		{
		}

		void ParentForm_DragDrop(object sender, DragEventArgs e)
		{
			OnParentFormDragDrop((ZForm)sender, e);
		}

		protected virtual void OnParentFormDragDrop(ZForm form, DragEventArgs args)
		{
			Setup();
			SetupUserControl();
		}

		void ParentForm_DataObjectPasted(object sender, DataObjectPastedEventArgs e)
		{
			OnParentFormDataObjectPasted((ZForm)sender, e);
		}

		protected virtual void OnParentFormDataObjectPasted(ZForm form, DataObjectPastedEventArgs e)
		{
			Setup();
			SetupUserControl();
		}
	}
}
