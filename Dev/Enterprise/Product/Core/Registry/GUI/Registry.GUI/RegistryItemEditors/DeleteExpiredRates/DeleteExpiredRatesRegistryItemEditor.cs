using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class DeleteExpiredRatesRegistryItemEditor : RegistryItemEditor
	{
		public DeleteExpiredRatesRegistryItemEditor(IRegistryDataType dataType, IRegistryItem registryItem)
			: base(dataType)
		{
			if (registryItem is DeleteExpiredRatesRegistryItem deleteExpiredRatesRegistryItem)
			{
				RegistryItem = deleteExpiredRatesRegistryItem;
			}
			else
			{
				ErrorReporter.ReportOnce("DeleteExpiredRatesRegistryItemEditor", $"registryItem should be type of DeleteExpiredRatesRegistryItem. registryItem Type:{registryItem?.GetType()?.FullName}");
			}
		}

		DeleteExpiredRatesRegistryItem RegistryItem { get; }

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((DeleteExpiredRatesUserControl)editorPane).DeleteExpiredRatesWrapper.ExpiredRates;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new DeleteExpiredRatesUserControl(new DeleteExpiredRatesWrapper(RegistryItem.Value));
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((DeleteExpiredRatesUserControl)editorPane).SetBusinessEntityValue((DeleteExpiredRates)value);
		}
	}
}
