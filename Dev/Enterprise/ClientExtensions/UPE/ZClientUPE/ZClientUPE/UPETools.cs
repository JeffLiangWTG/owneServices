using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE
{
	public class UPETools
	{
		public void LogQueueMovement(Logs logs, ZString queueName, ZString reason)
		{
			if (!reason.IsEmpty)
			{
				ZString logMsg = string.Format("Moved to {0}: {1}", queueName, reason);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				logs.AddNew(Events.EditedARecord, logMsg.SubstringSafe(0, StmALogSchema.SL_Reference.MaxLength));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		public void PerformActionInCorrectBranch(GlbBranch branch, Action action)
		{
			using (branch != null && branch.GB_Code != GlbBranch.CurrentBranch.GB_Code ? DisposableEnvironment.ForBranch(branch.GB_Code) : null)
			{
				action.Invoke();
			}
		}

		public List<GlbBranch> UPECustomisationBranches(bool returnAllBranches)
		{
			var branches = new HashSet<GlbBranch>();
			foreach (GlbCompany company in GlbCompany.GetActiveCompanies().Where(company => company.HasActiveBranch && UPEDataRegistry.Instance.EnableUPECustomisationsItem.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty)))
			{
				var branchPK = UPEDataRegistry.Instance.BranchToUseForUPECustomisationsItem.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (company.HasBranchWithThisPK(branchPK))
				{
					branches.Add(company.Factory.Load<GlbBranch>(branchPK));
					if (!returnAllBranches)
					{
						continue;
					}
				}

				foreach (var branch in company.ActiveBranches)
				{
					if (branches.Add(branch) && !returnAllBranches)
					{
						break;
					}
				}
			}
			return branches.ToList();
		}

		public void SendTimeoutEmail(string serviceTaskCode)
		{
			Guid notificationGroup = UPEDataRegistry.Instance.SftpServerTimeoutItemNotificationGroup;

			if (notificationGroup != Guid.Empty)
			{
				var email = new EmailDef();
				email.Subject = $"Service Task '{serviceTaskCode}' has timed out";
				email.Body = "";
				Env.OutgoingMailManager.CreateAndSave(email, notificationGroup, GroupSourceLocator.GetFromRegistryItem(UPEDataRegistry.Instance.SftpServerTimeoutItemNotificationGroupItem));
			}
		}

		public static UPETools Instance
		{
			get
			{
				if (fUPETools == null)
				{
					fUPETools = new UPETools();
				}
				return fUPETools;
			}
		}
		static UPETools fUPETools;
	}
}
