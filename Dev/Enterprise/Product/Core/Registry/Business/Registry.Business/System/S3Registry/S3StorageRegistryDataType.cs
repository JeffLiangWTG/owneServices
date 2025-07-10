using System;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	internal class S3StorageRegistryDataType : StringRegistryDataType
	{
		public S3StorageRegistryDataType()
			: this(string.Empty)
		{
		}

		public S3StorageRegistryDataType(string defaultValue)
			: base(defaultValue)
		{
		}

		protected override void ValidateBeforeRegistryFormSaveCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateBeforeRegistryFormSaveCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (SystemDataRegistry.Instance.EDocsStorageProvider.Value == Core.Constants.EDocsStorageProviders.Code.S3
				&& proposedValue != registryItem.Value.ToString()
				&& registryItem.Value.ToString() != DefaultValue)
			{
				var result = ObjectFactory.Get<IRegistryChangesNotifier>().ShowConfirmation(
					Res.GetString("AFBEFE70-7961-4439-A714-D00752D785AE", "Changing {0} might cause all eDocs stored in S3 to be inaccessible.", registryItem.Caption),
					Res.GetString("AD29880C-1D00-4613-9A9E-31B385DB44F6", "Change {0}", registryItem.Caption),
					SystemDataRegistry.Instance.UpdateS3ConfigContinueString,
					ZMessageBoxIcon.Warning);

				if (result != ZDialogResult.OK)
				{
					throw new RegistryValidationException(Res.GetString("D912E8BA-45BA-44B3-BB4E-7F8E524B4573", "Changing {0} canceled.", registryItem.Caption));
				}
			}
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore => true;
	}
}
