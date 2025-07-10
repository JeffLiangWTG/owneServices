using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public class TransactionApprovalRequestValidation : GenApprovalRequestValidation
	{
		public TransactionApprovalRequestValidation(GenApprovalRequest parent)
			: base(parent)
		{
		}

		protected override void CheckXP_ReasonDescription()
		{
			base.CheckXP_ReasonDescription();

			MandatoryValidation.CheckEntered(Parent.XP_ReasonDescriptionInfo);
		}

		protected override void CheckXP_GS_NKApprovingUser2()
		{
			base.CheckXP_GS_NKApprovingUser2();
			if (!Parent.XP_GS_NKApprovingUser2.IsEmpty)
			{
				CheckApprovingUsersContainDuplicate(Parent.XP_GS_NKApprovingUser2Info);
			}
		}

		protected override void CheckXP_GS_NKApprovingUser3()
		{
			base.CheckXP_GS_NKApprovingUser3();
			if (!Parent.XP_GS_NKApprovingUser3.IsEmpty)
			{
				CheckApprovingUsersContainDuplicate(Parent.XP_GS_NKApprovingUser3Info);
			}
		}

		protected override void CheckXP_GS_NKApprovingUser4()
		{
			base.CheckXP_GS_NKApprovingUser4();
			if (!Parent.XP_GS_NKApprovingUser4.IsEmpty)
			{
				CheckApprovingUsersContainDuplicate(Parent.XP_GS_NKApprovingUser4Info);
			}
		}

		protected override void CheckXP_GS_NKApprovingUser5()
		{
			base.CheckXP_GS_NKApprovingUser5();
			if (!Parent.XP_GS_NKApprovingUser5.IsEmpty)
			{
				CheckApprovingUsersContainDuplicate(Parent.XP_GS_NKApprovingUser5Info);
			}
		}

		protected override void CheckXP_GS_NKApprovingUser6()
		{
			base.CheckXP_GS_NKApprovingUser6();
			if (!Parent.XP_GS_NKApprovingUser6.IsEmpty)
			{
				CheckApprovingUsersContainDuplicate(Parent.XP_GS_NKApprovingUser6Info);
			}
		}

		void CheckApprovingUsersContainDuplicate(ZPropertyInfo approvingUserInfo)
		{
			var users = new HashSet<ZString>();
			var approvingUsers = new ZString[] { Parent.XP_GS_NKApprovingUser1, Parent.XP_GS_NKApprovingUser2, Parent.XP_GS_NKApprovingUser3,
												Parent.XP_GS_NKApprovingUser4, Parent.XP_GS_NKApprovingUser5, Parent.XP_GS_NKApprovingUser6 };
			foreach (var approvingUser in approvingUsers)
			{
				if (!approvingUser.IsEmpty && !users.Add(approvingUser))
				{
					approvingUserInfo.AddError(Res.GetString("1186022c-aa8c-460b-9b61-bf3f6895d8a1", "Approving users can not be the same user."));
					break;
				}
			}
		}
	}
}
