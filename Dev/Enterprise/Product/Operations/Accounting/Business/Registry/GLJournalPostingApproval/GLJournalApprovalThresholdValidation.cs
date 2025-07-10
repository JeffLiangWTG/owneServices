using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Res = Enterprise.Accounting.Business.Res;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	public class GLJournalApprovalThresholdValidation
	{
		public GLJournalApprovalThresholdValidation(GLJournalApprovalThreshold parent)
		{
			this.Parent = parent;
		}

		readonly GLJournalApprovalThreshold Parent;

		public void ValidateRow()
		{
			var errorMessage = Res.GetString("8e34dcf0-33bf-4980-ac34-62926998cad2", "The corresponding threshold amounts must be defined.");
			Parent.RemoveRowError(errorMessage);

			if (!Parent.HasErrors
				&& Parent.Type != GLJournalApprovalThreshold.TypeCodes.AnyChanges
				&& Parent.AuthorisationSettings.Count == 0)
			{
				Parent.AddRowError(errorMessage);
			}
		}

		public void ValidateType()
		{
			MandatoryValidation.CheckEntered(Parent.TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.TypeInfo, Parent.TypeList);
			if (!Parent.TypeInfo.HasErrors())
			{
				BusinessObjectCollection parentCollection = Parent.ParentCollection;
				if (parentCollection != null && parentCollection.Count > 1)
				{
					foreach (GLJournalApprovalThreshold config in parentCollection)
					{
						if (config != Parent)
						{
							if (Parent.Type == GLJournalApprovalThreshold.TypeCodes.AnyChanges)
							{
								if (Parent.Type == config.Type)
								{
									Parent.TypeInfo.AddError(ResString.GetMultilingualString("D001D060-90B8-444C-91D1-F33C44776CBC", "You can't have more than one record with type '{0}'.", Parent.Type));
								}
								else
								{
									Parent.TypeInfo.AddError(ResString.GetMultilingualString("11A559B0-C797-4129-B761-AFC4E3061D42", "'{0}' type cannot be used in combination with other types.", Parent.Type));
								}
							}
							else if (Parent.Type == config.Type)
							{
								if (Parent.Type == GLJournalApprovalThreshold.TypeCodes.All)
								{
									Parent.TypeInfo.AddError(ResString.GetMultilingualString("c3dd80ab-2abf-4883-b6fe-9618b881a38c", "You can't have more than one record with type 'All'."));
									break;
								}
								if (!Parent.GLAccount.IsEmpty && (Parent.GLAccount == config.GLAccount))
								{
									Parent.TypeInfo.AddError(ResString.GetMultilingualString("1636a634-7b6c-4564-bfc2-5028c7d875be", "At least one more record already exists with the same GL Account."));
									break;
								}
								else if (!Parent.ReportSection.IsEmpty && (Parent.ReportSection == config.ReportSection))
								{
									Parent.TypeInfo.AddError(ResString.GetMultilingualString("5ef78314-2fbb-43b1-a2e3-1577913dbb28", "At least one more record already exists with the same Report Section."));
									break;
								}
							}
						}
					}
				}
			}
		}

		public void ValidateGLAccount()
		{
			if (!Parent.GLAccount_ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.GLAccountInfo);
				ListValidation.ErrorIfInvalidPK(Parent.GLAccountInfo, Parent.GLAccounts, GLJournalApprovalThreshold.GLAccountsAdditionalFilterInvalidMessage);
				if (!Parent.GLAccountInfo.HasErrors())
				{
					BusinessObjectCollection parentCollection = Parent.ParentCollection;
					if (parentCollection != null)
					{
						foreach (GLJournalApprovalThreshold config in parentCollection)
						{
							if (config != Parent)
							{
								if (config.Type == GLJournalApprovalThreshold.TypeCodes.GLAccount && config.GLAccount == Parent.GLAccount)
								{
									Parent.GLAccountInfo.AddError(ResString.GetMultilingualString("1636a634-7b6c-4564-bfc2-5028c7d875be", "At least one more record already exists with the same GL Account."));
									break;
								}
							}
						}
					}
				}
			}
		}

		public void ValidateReportSection()
		{
			if (!Parent.ReportSection_ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.ReportSectionInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ReportSectionInfo, Parent.ReportSectionList);
				BusinessObjectCollection parentCollection = Parent.ParentCollection;
				if (parentCollection != null)
				{
					foreach (GLJournalApprovalThreshold config in parentCollection)
					{
						if (config != Parent)
						{
							if (config.Type == GLJournalApprovalThreshold.TypeCodes.ReportSection && config.ReportSection == Parent.ReportSection)
							{
								Parent.ReportSectionInfo.AddError(ResString.GetMultilingualString("5ef78314-2fbb-43b1-a2e3-1577913dbb28", "At least one more record already exists with the same Report Section."));
								break;
							}
						}
					}
				}
			}
		}
	}
}