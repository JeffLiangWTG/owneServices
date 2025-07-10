using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.Common.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	internal static class DragDropManager
	{
		public static void HandleDragOver(Control control, DragEventArgs e)
		{
			if (control.FindForm() is ZForm form)
			{
				MainThreadRunner.RunOnMainThread(() => form.HandleDragOver(e));
			}

#if DEBUG
			DragOverHandledForTesting = true;
#endif
		}

		public static void HandleDragDrop(Control control, DragEventArgs e)
		{
			if (control.FindForm() is ZForm form)
			{
				MainThreadRunner.RunOnMainThread(() => form.HandleDragDrop(e));
			}

#if DEBUG
			DragDropHandledForTesting = true;
#endif
		}

#if DEBUG

		public static void Reset()
		{
			DragOverHandledForTesting = false;
			DragDropHandledForTesting = false;
		}

		[SuppressThreadStaticFieldMessage]
		public static bool DragOverHandledForTesting;
		[SuppressThreadStaticFieldMessage]
		public static bool DragDropHandledForTesting;

#endif

	}
}
