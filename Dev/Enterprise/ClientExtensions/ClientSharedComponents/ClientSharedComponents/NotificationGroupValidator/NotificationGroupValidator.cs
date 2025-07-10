using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ClientSharedComponents
{
	public static class NotificationGroupValidator
	{
		public static bool IsValid(ZGuid groupPK, BusinessObjectFactory factory)
		{
			bool result = false;
			if (!groupPK.IsEmpty && groupPK.IsValid)
			{
				GlbGroup group = factory.Load<GlbGroup>(groupPK);
				if (group != null)
				{
					foreach (GlbStaff staff in group.Staff)
					{
						if (EmailAddressValidation.IsEmailAddressValidAndNotEmpty(staff.GS_EmailAddress))
						{
							result = true;
							break;
						}
					}
				}
			}
			return result;
		}
	}
}
