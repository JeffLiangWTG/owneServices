using System;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public class NotificationGroupGuidRegistryDataType : GuidRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, Guid proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			if (proposedValue != Guid.Empty)
			{
				try
				{
					new EmailGroupUtility().GetGroupEmailCollection(proposedValue, true);
				}
				catch (EmailSendFailedException ex)
				{
					throw new RegistryValidationException(ex.Message);
				}
			}
		}
	}
}
