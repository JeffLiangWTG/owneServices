using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class WebPrintNudgeSuspendingRegistryItemEditor : RegistryItemEditor
	{
		public WebPrintNudgeSuspendingRegistryItemEditor(IRegistryDataType dataType, IRegistryItem registryItem)
			: base(dataType)
		{
			if (registryItem is WebPrintNudgeSuspendingRegistryItem webPrintNudgeSuspendingRegistryItem)
			{
				RegistryItem = webPrintNudgeSuspendingRegistryItem;
			}
			else
			{
				ErrorReporter.ReportOnce("WebPrintNudgeSuspendingRegistryItemEditor", $"registryItem should be type of WebPrintNudgeSuspendingRegistryItem. registryItem Type:{registryItem?.GetType()?.FullName}");
			}
		}

		WebPrintNudgeSuspendingRegistryItem RegistryItem { get; }

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((WebPrintNudgeSuspendingUserControl)editorPane).NudgeSuspendingWrapper.NudgeSuspending;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new WebPrintNudgeSuspendingUserControl(new WebPrintNudgeSuspendingWrapper(RegistryItem.Value));
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((WebPrintNudgeSuspendingUserControl)editorPane).SetBusinessEntityValue((WebPrintNudgeSuspending)value);
		}
	}
}
