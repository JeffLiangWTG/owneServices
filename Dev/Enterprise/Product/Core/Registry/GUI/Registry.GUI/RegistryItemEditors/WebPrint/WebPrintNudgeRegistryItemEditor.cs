using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class WebPrintNudgeRegistryItemEditor : RegistryItemEditor
	{
		public WebPrintNudgeRegistryItemEditor(IRegistryDataType dataType, IRegistryItem registryItem)
			: base(dataType)
		{
			if (registryItem is WebPrintNudgeRegistryItem webPrintNudgeRegistryItem)
			{
				RegistryItem = webPrintNudgeRegistryItem;
			}
			else
			{
				ErrorReporter.ReportOnce("WebPrintNudgeRegistryItemEditor", $"registryItem should be type of WebPrintNudgeRegistryItem. registryItem Type:{registryItem?.GetType()?.FullName}");
			}
		}

		WebPrintNudgeRegistryItem RegistryItem { get; }

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((WebPrintNudgeUserControl)editorPane).NudgeWrapper.Nudge;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new WebPrintNudgeUserControl(new WebPrintNudgeWrapper(RegistryItem.Value));
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((WebPrintNudgeUserControl)editorPane).SetBusinessEntityValue((WebPrintNudge)value);
		}
	}
}
