using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class CreditControlledDocumentsApprovalBulk : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CreditControlledDocumentsApprovalBulk(BusinessObjectFactory factory, params CreditControlledDocumentsApproval[] approvalRequests)
			: base(factory)
		{
			ApprovalRequests = approvalRequests;
		}

		readonly CreditControlledDocumentsApproval[] ApprovalRequests;

		CreditControlledDocumentsApprovalCollection CreditControlledDocumentsApprovals
		{
			get
			{
				if (creditControlledDocumentsApprovals == null)
				{
					var filter = new ZQuery(GenApprovalRequestSchema.PK, ApprovalRequests.Select(element => element.PK));
					creditControlledDocumentsApprovals = new CreditControlledDocumentsApprovalCollection(Factory, filter);
				}

				return creditControlledDocumentsApprovals;
			}
		}
		CreditControlledDocumentsApprovalCollection creditControlledDocumentsApprovals;

		public CreditControlledDocumentsApprovalCollection CreditControlledDocumentsApprovalsAllowedToProcess
		{
			get
			{
				if (creditControlledDocumentsApprovalsAllowedToProcess == null)
				{
					ReCreateCreditControlledDocumentsApprovalsAllowedToProcess(ApprovalRequests.Select(x => x.PK));
				}

				return creditControlledDocumentsApprovalsAllowedToProcess;
			}
		}
		CreditControlledDocumentsApprovalCollection creditControlledDocumentsApprovalsAllowedToProcess;

		bool SetPrivilegeRequirement(CreditControlledDocumentsApproval approvalRequest, int authorizationRequired)
		{
			var checkpoint = AccountingMasterFilesUtils.GetOnCreditHoldControllerSecurityCheckPoint(authorizationRequired);
			if (checkpoint != null && checkpoint.IsAllowed)
			{
				approvalRequest.XP_PrivledgeRequired = authorizationRequired.ToString(CultureInfo.InvariantCulture);
				return true;
			}
			return false;
		}

		public bool ChangeStatus(string status)
		{
			var permittedPKs = new List<ZGuid>();

			foreach (var approvalRequest in CreditControlledDocumentsApprovals)
			{
				ZString[] priviledes = approvalRequest.XP_PrivledgeRequired.Replace(" ", string.Empty).Split(',');

				bool isAllowed = ((status == Constants.GenApprovalRequestApprovalStatus.Cancelled) &&
								  (approvalRequest.XP_SystemCreateUser == Env.CurrentUser.Initials));

				if (!isAllowed)
				{
					if (priviledes.Length == 1 && priviledes[0].IsEmpty)
					{
						isAllowed = SetPrivilegeRequirement(approvalRequest, AccountingMasterFilesUtils.HighestOnCreditHoldControllerSecurityLevel);
					}
					else
					{
						foreach (var str in priviledes)
						{
							int authorizationRequired = ZInt.ParseSafe(str, 0);
							if (SetPrivilegeRequirement(approvalRequest, authorizationRequired))
							{
								isAllowed = true;
								break;
							}
						}
					}
				}

				if (!isAllowed)
				{
					continue;
				}

				permittedPKs.Add(approvalRequest.PK);

				approvalRequest.XP_ApprovalStatus = status;
				switch (status)
				{
					case Constants.GenApprovalRequestApprovalStatus.Approved:
					case Constants.GenApprovalRequestApprovalStatus.Rejected:
						approvalRequest.XP_GS_NKApprovingUser1 = GlbStaff.CurrentUser.GS_Code;
						approvalRequest.XP_ApprovalDate = ZDateTime.Now;
						break;

					case Constants.GenApprovalRequestApprovalStatus.Cancelled:
						approvalRequest.XP_GS_NKApprovingUser1 = ZString.Empty;
						approvalRequest.XP_ApprovalDate = ZDateTime.Empty;
						break;
				}
			}

			ReCreateCreditControlledDocumentsApprovalsAllowedToProcess(permittedPKs);
			return CreditControlledDocumentsApprovalsAllowedToProcess.Count != 0;
		}

		void ReCreateCreditControlledDocumentsApprovalsAllowedToProcess(IEnumerable<ZGuid> permittedPKs)
		{
			if (creditControlledDocumentsApprovalsAllowedToProcess != null)
			{
				UnRegisterEditableChildObject(creditControlledDocumentsApprovalsAllowedToProcess);
				creditControlledDocumentsApprovalsAllowedToProcess = null;
			}

			var filter = new ZQuery(GenApprovalRequestSchema.PK, permittedPKs);
			creditControlledDocumentsApprovalsAllowedToProcess = new CreditControlledDocumentsApprovalCollection(Factory, filter);

			RegisterEditableChildObject(creditControlledDocumentsApprovalsAllowedToProcess);
		}
	}
}
