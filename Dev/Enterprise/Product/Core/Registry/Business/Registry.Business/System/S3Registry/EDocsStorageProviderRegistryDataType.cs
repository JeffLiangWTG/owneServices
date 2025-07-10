using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	internal class EDocsStorageProviderRegistryDataType : CodePairRegistryDataType
	{
		public EDocsStorageProviderRegistryDataType()
			: base(OLookUpEditType.EDocsStorageProvider, false, true)
		{
		}

		protected override void ValidateBeforeRegistryFormSaveCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateBeforeRegistryFormSaveCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (proposedValue == Core.Constants.EDocsStorageProviders.Code.S3)
			{
				if (Db.Connection.GetDatabases(DatabaseType.SD).Any(docManagerDb => !DocManagerUtils.IsDbWriteableForDocManager(docManagerDb)))
				{
					throw new RegistryValidationException(Res.GetString("48EED09A-79D8-490C-AC6C-73EB4C09C9C3", "Some of the eDocs storage databases are read-only, please ensure all eDocs storage databases are writable before enabling {0}.", Core.Constants.EDocsStorageProviders.Description.S3));
				}

				CheckCriticalS3RegistriesAreEntered();
			}
			else if (registryItem.Value.ToString() == Core.Constants.EDocsStorageProviders.Code.S3)
			{
				var result = ObjectFactory.Get<IRegistryChangesNotifier>().ShowConfirmation(
					Res.GetString("46A7057A-3F37-487E-9822-D1D4F4DFF667", "Disabling S3 Storage will cause all eDocs stored in S3 to be inaccessible."),
					Res.GetString("6F1F8025-A100-4855-8E88-2520ACDAE47A", "Disable S3 Storage"),
					SystemDataRegistry.Instance.UpdateS3ConfigContinueString,
					ZMessageBoxIcon.Warning);

				if (result != ZDialogResult.OK)
				{
					throw new RegistryValidationException(Res.GetString("BC605A64-5C2E-41A9-B2DC-721987656936", "Disabling S3 Storage canceled."));
				}
			}
		}

		void CheckCriticalS3RegistriesAreEntered()
		{
			var registriesNotSet = new List<StringRegistryItem>();
			if (SystemDataRegistry.Instance.EDocsStorageServiceUrl.Value.IsNullOrEmpty())
			{
				registriesNotSet.Add(SystemDataRegistry.Instance.EDocsStorageServiceUrl);
			}

			if (SystemDataRegistry.Instance.DocManagerStorageBucketName.Value.IsNullOrEmpty())
			{
				registriesNotSet.Add(SystemDataRegistry.Instance.DocManagerStorageBucketName);
			}

			if (SystemDataRegistry.Instance.EDocsStorageAccess.Value.IsNullOrEmpty())
			{
				registriesNotSet.Add(SystemDataRegistry.Instance.EDocsStorageAccess);
			}

			if (registriesNotSet.Count > 0)
			{
				throw new RegistryValidationException(Res.GetString("571AC3D1-F5F5-4032-AE6A-621E44D5483C", @"The following Registry item(s) must be configured before enabling {1}:
{0}",
						MultilingualString.Join(System.Environment.NewLine, registriesNotSet.Select(r => ((IMultilingualRegistryItem)r).LocationMultilingual).ToArray()),
						Enterprise.Core.Constants.EDocsStorageProviders.Description.S3));
			}
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore => true;
	}
}
