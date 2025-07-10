using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.ActiveDirectory
{
	[RegistryEditor("Enterprise.Security.ActiveDirectory.GUI.DomainCredentialsCollectionRegistryEditor, Enterprise.Security.ActiveDirectory.GUI")]
	public class DomainCredentialsCollectionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DomainCredentialsCollection>
	{
		public DomainCredentialsCollectionRegistryDataType(DomainCredentialsCollection defaultValue)
			: base(defaultValue)
		{
		}

		protected override void ValidateBeforeRegistryFormSaveCore(IRegistryItem registryItem, DomainCredentialsCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateBeforeRegistryFormSaveCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			var isValid = true;

			foreach (DomainCredentials domainCredentials in proposedValue)
			{
				isValid &= domainCredentials.Validation.AreDomainLoginDetailsValid();
				isValid &= domainCredentials.Validation.IsOrganisationalUnitValid(DomainCredentialsValidation.OrganisationalUnitType.User);
				isValid &= domainCredentials.Validation.IsOrganisationalUnitValid(DomainCredentialsValidation.OrganisationalUnitType.Group);
			}
			if (!isValid)
			{
				throw new RegistryValidationException(Res.GetString("522b42e9-6e3e-4d76-8326-4f317664e1c1", "The domains' credentials are not valid."));
			}

			if (proposedValue.Count == 0 && ActiveDirectoryRegistry.Instance.IsIntegrationEnabled)
			{
				throw new RegistryValidationException(Res.GetString("D2843A34-4084-42AE-A0A8-EC62FB6197C8", "You need to have at least one domain set in this registry while AD Integration is enabled."));
			}

			var domainsWithInvalidChanges = GetDomainsWithInvalidChanges((DomainCredentialsCollection)registryItem.Value, proposedValue);
			if (domainsWithInvalidChanges?.Count() > 0)
			{
				throw new RegistryValidationException(Res.GetString("B05B2202-ACDF-4FA8-A9F9-91AA9A0FE8B8", "You cannot rename or remove the following domains as they are still in use: {0}", string.Join(", ", domainsWithInvalidChanges)));
			}

			const string confirmationText = "CHANGEDOMAIN";
			var message = Res.GetString("dfe170c4-52d8-4d15-8b03-25f9f4ad81c9", @"Changing this registry item may cause users and groups to be created in the new domains for existing staff and group records in {0}.
Please enter the text '{1}' to continue. It is advisable to restart {0} after changing this registry item's value.", Core.Constants.ProductName, confirmationText);
			var caption = Res.GetString("8be9a52e-c9ef-4d46-9a91-39fb1443bc87", "Confirm domain change");

			if (Globals.Message.ShowConfirmation(message, caption, confirmationText, ZMessageBoxIcon.Question, ZMessageBoxButtons.OKCancel) != ZDialogResult.OK)
			{
				throw new RegistryValidationException(Res.GetString("e23477c3-8dfc-4ad1-b83c-31d446eeeb70", "Canceled saving this registry value"));
			}
		}

		IEnumerable<string> GetDomainsWithInvalidChanges(DomainCredentialsCollection originalValue, DomainCredentialsCollection proposedValue)
		{
			foreach (DomainCredentials dc in originalValue)
			{
				if (!proposedValue.Cast<DomainCredentials>().Any(x => x.DomainName.EqualsIgnoringCase(dc.DomainName)))
				{
					if (dc.Validation.IsCurrentlyInUse())
					{
						yield return dc.DomainName;
					}
				}
			}
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore => true;
	}
}
