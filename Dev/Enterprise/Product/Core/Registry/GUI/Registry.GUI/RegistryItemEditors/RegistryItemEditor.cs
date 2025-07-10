using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Integration;

namespace Enterprise.Registry.GUI
{
	public abstract class RegistryItemEditor
	{
		protected RegistryItemEditor(IRegistryDataType dataType)
		{
			this.DataType = dataType;
		}

		public Control NewWinFormsEditorPane()
		{
			return NewWinFormsEditorPaneCore();
		}

		public object GetValueFromEditorPane(Control editorPane)
		{
			return GetValueFromEditorPaneCore(editorPane);
		}

		public void SetValueFromEditorPane(Control editorPane, object value)
		{
			SetValueFromEditorPaneCore(editorPane, value);
		}

		public void EnableEditorPane(Control editorPane, bool enabled)
		{
			EnableEditorPaneCore(editorPane, enabled);
		}

		protected internal static void NotifyChanges(Control editorPane)
			=> (editorPane.FindForm() as IRegistryForm)?.UpdateHasChanges();

		public virtual void SetEditorPaneLayout(Control editorPane, int width, int height)
		{
			switch (Anchor)
			{
				case (EditorPaneAnchor.TopLeftRight):
					ControlDpiScalingHelper.SetWidth(ref editorPane, width, false);
					editorPane.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
					break;

				case (EditorPaneAnchor.All):
					ControlDpiScalingHelper.SetWidth(ref editorPane, width, false);
					ControlDpiScalingHelper.SetHeight(ref editorPane, height, false);
					editorPane.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
					break;

				default:
					break;
			}
		}

		// Temporary only - will be changed once all validation is implemented in the business layer
		public virtual string GetCustomValidation(Control editorPane)
		{
			return "";
		}

		public enum EditorPaneAnchor
		{
			TopLeft,
			TopLeftRight,
			All
		}

		protected virtual EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.TopLeft; }
		}

		protected readonly IRegistryDataType DataType;
		protected abstract Control NewWinFormsEditorPaneCore();
		protected abstract object GetValueFromEditorPaneCore(Control editorPane);
		protected abstract void SetValueFromEditorPaneCore(Control editorPane, object value);

		protected virtual void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			editorPane.SetReadOnly(!enabled);
		}
	}
}