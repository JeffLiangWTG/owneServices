using Enterprise.Registry.Business;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageDatabaseInfoValidation : AutoStorageDatabaseInfoValidation
	{
		public StorageDatabaseInfoValidation(AutoStorageDatabaseInfo parent)
			: base(parent) { }

		#region Implementation

		public new StorageDatabaseInfo Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (StorageDatabaseInfo)base.Parent; }
		}

		protected override void CheckNewReadOnly()
		{
			base.CheckNewReadOnly();
			if (Parent.NewReadOnly && SystemDataRegistry.Instance.EDocsStorageProvider.Value == Core.Constants.EDocsStorageProviders.Code.S3)
			{
				Parent.NewReadOnlyInfo.AddError(Res.GetString("82135D3A-C8C3-4429-BED9-F6A2A52ED246", "Cannot set eDocs database to read-only if {0} is enabled.", Core.Constants.EDocsStorageProviders.Description.S3));
			}
		}

		#endregion
	}
}
