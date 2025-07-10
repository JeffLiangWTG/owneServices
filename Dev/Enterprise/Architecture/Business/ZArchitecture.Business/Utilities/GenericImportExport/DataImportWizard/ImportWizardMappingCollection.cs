namespace Enterprise.ZArchitecture.DataMapping
{
	public class ImportWizardMappingCollection : ImportExportMappingCollection<ImportWizardMapping, ImportWizard>
	{
		public ImportWizardMappingCollection(ImportWizard wizard)
			: base(wizard)
		{ }

		public void ClearColumnIndexes()
		{
			foreach (ImportWizardMapping m in this)
			{
				m.ClearAllFileColumnIndex();
			}
		}
	}
}
