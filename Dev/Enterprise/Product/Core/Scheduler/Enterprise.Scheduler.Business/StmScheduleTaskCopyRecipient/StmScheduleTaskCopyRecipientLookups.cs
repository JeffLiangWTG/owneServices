//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmScheduleTaskCopyRecipientLookups
//
//    This class should be used for overriding collections in AutoStmScheduleTaskCopyRecipientLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Scheduler.Business
{
	public class StmScheduleTaskCopyRecipientLookups : AutoStmScheduleTaskCopyRecipientLookups
	{
		public StmScheduleTaskCopyRecipientLookups(AutoStmScheduleTaskCopyRecipient parent) : base(parent)
		{
		}

		public CodeDescriptionPairList SCR_AvailableEmailAddress_List
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var copyRecipient = (StmScheduleTaskCopyRecipient)Parent;
				if (copyRecipient != null)
				{
					var recipient = copyRecipient.Recipient;
					if (recipient != null)
					{
						var organization = recipient.Header;
						if (organization != null)
						{
							foreach (var contact in organization.ContactsActive.Cast<OrgContact>())
							{
								result.Add(new CodeDescriptionPair(contact.Email.ToString(), contact.OC_ContactName.ToString()));
							}
						}
					}
				}
				return result;
			}
		}
	}
}