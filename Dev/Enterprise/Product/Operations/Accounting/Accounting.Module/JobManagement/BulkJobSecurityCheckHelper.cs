using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
#if DEBUG
	internal
#endif
	static class BulkJobSecurityCheckHelper
	{
		public static bool CheckSecurityCheckPoint(BusinessObject[] selectedElements, Func<BusinessObject[], SecurityCheckpoint[]> getSecurityCheckPoints)
		{
			bool result = false;
			var securityCheckpoints = getSecurityCheckPoints(selectedElements);
			if (!securityCheckpoints.Any())
			{
				throw new InvalidOperationException("Security checkpoint can't be found.");
			}

			var securityCheckpointsIsNotAllowedFilter = from checkPoint in securityCheckpoints
														where checkPoint != null && !checkPoint.IsAllowed
														select checkPoint.DisplayTextPathToSecurityRight;
			var securityCheckpointsIsNotAllowed = securityCheckpointsIsNotAllowedFilter.ToArray();
			if (securityCheckpointsIsNotAllowed.Any())
			{
				MultilingualString errorMessage = MultilingualString.Join(System.Environment.NewLine + System.Environment.NewLine, SecurityCore.SecurityErrorMessage, MultilingualString.Join(System.Environment.NewLine, securityCheckpointsIsNotAllowed));
				Globals.Message.ShowError(errorMessage, Res.GetString("00053003-9AB5-466D-8146-C9C557DD79E0", "Access Denied"));
			}
			else
			{
				result = true;
			}

			return result;
		}

		public static SecurityCheckpoint[] GetSecurityCheckPoints(string securityCheckpointName, BusinessObject[] selectedElements)
		{
			var securityCheckpoints = GetUniqueSecurityHelpers(GetSecurityHelpers(selectedElements));
			return securityCheckpoints.Select(securityCheckpoint => GetNewSecurityHelper(securityCheckpoint).GetInvSecurity(securityCheckpointName)).ToArray();
		}

		static SecurityCheckpoint[] GetUniqueSecurityHelpers(SecurityCheckpoint[] securityCheckpoints)
		{
			var uniqueSecurityHelpers = new Dictionary<CheckpointLookupKey, SecurityCheckpoint>();
			foreach (var checkpoint in securityCheckpoints)
			{
				var key = checkpoint.LookupKey;
				if (!uniqueSecurityHelpers.ContainsKey(key))
				{
					uniqueSecurityHelpers.Add(key, checkpoint);
				}
			}

			return uniqueSecurityHelpers.Values.ToArray();
		}

		static SecurityCheckpoint[] GetSecurityHelpers(BusinessObject[] selectedElements)
		{
			var securityHelpers = new List<SecurityCheckpoint>();
			foreach (var bizObj in selectedElements)
			{
				var supporter = GetJobInvoicingSupporter(bizObj);
				if (supporter != null)
				{
					securityHelpers.Add(supporter.JobInvoicingSecurity);
				}
				else
				{
					throw new InvalidOperationException("Security checkpoint is not defined.");
				}
			}

			return securityHelpers.ToArray();
		}

		static IJobInvoicingSupporter GetJobInvoicingSupporter(BusinessObject bizObj)
		{
			IJobInvoicingSupporter result = null;
			var job = bizObj as JobHeader;
			if (job != null)
			{
				job.InitializeParentFromGenericJobWithSettingDefaults();
				var plugin = job.Parent as IJobInvoicingPlugIn;
				if (plugin != null)
				{
					result = plugin.InvoicingSupporter;
				}
			}
			else
			{
				IJobInvoicingPlugIn jobInvoicing = bizObj as IJobInvoicingPlugIn;
				if (jobInvoicing != null)
				{
					result = jobInvoicing.InvoicingSupporter;
				}
			}

			return result;
		}

		static JobInvoicingSecurityHelper GetNewSecurityHelper(SecurityCheckpoint checkpoint)
		{
			return new JobInvoicingSecurityHelper(checkpoint, false);
		}
	}
}
