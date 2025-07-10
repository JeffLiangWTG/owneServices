using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.AuditDataServices.Notification;
using Enterprise.Integration;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;

[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(Enterprise.AuditDataServices.Notification.AuditSubscriberProcessorTask))]
namespace Enterprise.LogWalker
{
	[Serializable]
	public class PatternMatchingSubscriber : LogSubscriber
	{
		public override string Name => "PatternMatchingSubscriber";

		public override string[] TableNames => new string[] { OrgAddressSchema.Constants.TableName,
																OrgContactSchema.Constants.TableName,
																OrgCusCodeSchema.Constants.TableName,
																OrgBrandOrRelatedNameSchema.Constants.TableName,
																OrgWebURLSchema.Constants.TableName,
																OrgHeaderSchema.Constants.TableName,
																GlbPersonSchema.Constants.TableName,
																GlbStaffSchema.Constants.TableName,
																HRJobApplicantSchema.Constants.TableName,
																OrgContactItemSchema.Constants.TableName,
																GenRegCertAccredMaintListSchema.Constants.TableName };

		public override string[] EventTypes => new string[] { AutoEvents.AddedARecordToTheSystem.Code, AutoEvents.EditedARecord.Code };

		bool IsAuditNotificationTaskEnabled(BusinessObjectFactory factory)
		{
			return ObjectFactory.Get<IServiceManagerQuerier>()
				.CheckStateOfNamedServiceTask(AuditSubscriberProcessorTask.ServiceTaskCode) > ServiceTaskStatus.ServiceTaskIsInactive;
		}

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			var factory = queuedLogs[0].Factory;

			if (!IsAuditNotificationTaskEnabled(factory))
			{
				var tablePrefixes = GetTablePrefixes();
				var queues = queuedLogs.Where(q => tablePrefixes.Contains(q.SJ_ParentTableCode));

				foreach (var queueLog in queues)
				{
					var bizO = queueLog.Factory.Load(queueLog.SJ_ParentTableCode, queueLog.SJ_ParentID);

					if (bizO != null)
					{
						GetPatternMatchingMaintenance(bizO).CreateOrUpdatePatternMatchingTables();
					}
				}

				DefaultLogger.Log(LogType.Information, "Logs in this batch processed successfully.");
			}
			else
			{
				DefaultLogger.Log(LogType.Debug, "Could not process logs because Change Data Capture is enabled.");
			}
		}

		HashSet<string> GetTablePrefixes()
		{
			var list = new HashSet<string>();

			list.Add(OrgAddressSchema.Constants.Prefix);
			list.Add(OrgContactSchema.Constants.Prefix);
			list.Add(OrgCusCodeSchema.Constants.Prefix);
			list.Add(OrgBrandOrRelatedNameSchema.Constants.Prefix);
			list.Add(OrgWebURLSchema.Constants.Prefix);
			list.Add(OrgHeaderSchema.Constants.Prefix);
			list.Add(OrgContactItemSchema.Constants.Prefix);
			list.Add(GlbPersonSchema.Constants.Prefix);
			list.Add(GlbStaffSchema.Constants.Prefix);
			list.Add(HRJobApplicantSchema.Constants.Prefix);
			list.Add(GenRegCertAccredMaintListSchema.Constants.Prefix);

			return list;
		}

		IPatternMatchingMaintenance GetPatternMatchingMaintenance(BusinessObject bizO)
		{
			switch (bizO.TablePrefix)
			{
				case OrgAddressSchema.Constants.Prefix:
					return new PatternMatchingOrgAddressMaintenance((OrgAddress)bizO);
				case OrgContactSchema.Constants.Prefix:
					return new PatternMatchingOrgContactMaintenance((OrgContact)bizO);
				case OrgCusCodeSchema.Constants.Prefix:
					return new PatternMatchingOrgCusCodeMaintenance((OrgCusCode)bizO);
				case OrgBrandOrRelatedNameSchema.Constants.Prefix:
					return new PatternMatchingOrgBrandOrRelatedNameMaintenance((OrgBrandOrRelatedName)bizO);
				case OrgWebURLSchema.Constants.Prefix:
					return new PatternMatchingOrgWebURLMaintenance((OrgWebURL)bizO);
				case OrgHeaderSchema.Constants.Prefix:
					return new PatternMatchingOrgHeaderMaintenance((OrgHeader)bizO);
				default:
					return GetPatternMatchingPersonMaintenance(bizO);
			}
		}

		IPatternMatchingMaintenance GetPatternMatchingPersonMaintenance(BusinessObject bizO)
		{
			switch (bizO.TablePrefix)
			{
				case OrgContactItemSchema.Constants.Prefix:
					return new PatternMatchingOrgContactItemMaintenance((OrgContactItem)bizO);
				case GlbPersonSchema.Constants.Prefix:
					return new PatternMatchingGlbPersonMaintenance((GlbPerson)bizO);
				case GlbStaffSchema.Constants.Prefix:
					return new PatternMatchingGlbStaffMaintenance((GlbStaff)bizO);
				case HRJobApplicantSchema.Constants.Prefix:
					return new PatternMatchingHRJobApplicantMaintenance((HRJobApplicant)bizO);
				case GenRegCertAccredMaintListSchema.Constants.Prefix:
					return new PatternMatchingGenRegCertAccredMaintListMaintenance((GenRegCertAccredMaintList)bizO);
				default:
					return null;
			}
		}

		public override string FriendlyName => "Maintains pattern matching tables";
	}
}
