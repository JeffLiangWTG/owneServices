namespace Enterprise.DbUpgrader.Transformation.Common
{
	public abstract class DeleteRegistryItem : RegistryDataTransformation
	{
		protected override void OfflinePostUpgradeTransform()
		{
			DeleteRegistryItemRows(GetRegistryItemNames());
		}

		public override sealed string UserDescription
		{
			get { return "Deleting Registry Overrides: " + string.Join(", ", GetRegistryItemNames()); }
		}

		protected abstract string[] GetRegistryItemNames();
	}
}
