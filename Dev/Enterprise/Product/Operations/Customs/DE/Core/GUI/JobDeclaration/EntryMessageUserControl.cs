using Enterprise.Customs.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class EntryMessageUserControl : EU.GUI.EntryMessageUserControl
	{
		public EntryMessageUserControl()
		{
		}

		protected override BaseCustomsEntryUserControl GetMessageUserControl()
		{
			BaseCustomsEntryUserControl result;
			if (JobDeclaration.IsImport)
			{
				result = new ImportMessageUserControl();
			}
			else if (JobDeclaration.IsExport)
			{
				result = new ExportMessageUserControl();
			}
			else
			{
				result = base.GetMessageUserControl();
			}
			return result;
		}
	}
}
