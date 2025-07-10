namespace Enterprise.DocumentScanning.Business
{
	public class StorageMainValidation : AutoStorageMainValidation
	{
		public StorageMainValidation(AutoStorageMain parent)
			: base(parent)
		{
		}

		new StorageMain Parent
		{
			get { return (StorageMain)base.Parent; }
		}

		protected override void CheckSM_Type()
		{
			base.CheckSM_Type();

			if (!AssemblyDataLookup.IsDocManagerCodeValid(Parent.SM_Type))
			{
				Parent.SM_TypeInfo.AddError(Res.GetString("ca89a46d-e26d-4c84-81d5-c42da06e4f97", "Enter a valid Type."));
			}
		}
	}
}
