using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.ActiveDirectory
{
	public class GroupSyncRegexRegistryDataType : StringRegistryDataType
	{
		public GroupSyncRegexRegistryDataType()
			: base(string.Empty)
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			try
			{
				Regex.Match("", proposedValue);
			}
			catch (ArgumentException)
			{
				throw new RegistryValidationException((NoResString)"The regex is not valid.");
			}
		}

		bool ValidateGroupWithRegex(string regexPattern)
		{
			var factory = new BusinessObjectFactory();
			var query = Synchronisation.EntitySynchroniser.GetGroupsToSynchroniseQuery(true);
			var groups = (IEnumerable<GlbGroup>)factory.Load<GlbGroup>(query);

			groups = Synchronisation.EntitySynchroniser.GetGroupsMatchWithRegex(groups, regexPattern);

			var groupsDetails = groups.Any() ? string.Join(", ", groups.Take(100).Select(g => $"{g.GG_Desc} ({g.GG_Category})")) : (NoResString)"(No matches)";

			const string confirmationText = "YES";
			var message = (NoResString)@$"This regex will match the following groups (only the first 100 shown):
{groupsDetails}";
			var caption = (NoResString)"Verify Regex";

			if (Globals.Message.ShowConfirmation(message, caption, confirmationText, ZMessageBoxIcon.Question, ZMessageBoxButtons.OKCancel) != ZDialogResult.OK)
			{
				throw new RegistryValidationException((NoResString)"Canceled saving this registry value");
			}
			return true;
		}

		protected override void ValidateBeforeRegistryFormSaveCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateBeforeRegistryFormSaveCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (ValidateGroupWithRegex(proposedValue))
			{
				ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.DefaultValue); // Ensure to sync all
			}
		}

		protected override bool AllowNullCore => false;

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore => true;
	}
}
