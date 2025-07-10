using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CargoWise.ActiveDirectory;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.ActiveDirectory
{
	public class ADMonitor : IADMonitor
	{
		public ADMonitor(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");
			Factory = factory;
		}

		protected BusinessObjectFactory Factory { get; }

		public void CheckSynchronisedStatusInRange(Guid lowerPk, Guid upperPk)
		{
			var entitiesToSync = ActiveDirectoryRegistry.Instance.EntitiesToSync;
			var shouldSyncGroups = entitiesToSync != EntitiesToSync.UsersOnly;

			var staffToCheck = Factory.Load<GlbStaff>(GetStaffForCheckingQuery(lowerPk, upperPk));
			CheckStaffSynchronisedStatus(staffToCheck);

			if (shouldSyncGroups)
			{
				var groupToCheck = Factory.Load<GlbGroup>(GetGroupsForCheckingQuery(lowerPk, upperPk));
				CheckGroupSynchronisedStatus(groupToCheck);
			}
		}

		void CheckStaffSynchronisedStatus(IEnumerable<GlbStaff> staffToCheck)
		{
			foreach (var staff in staffToCheck)
			{
				CheckSynchronisedStatus(staff);
			}
		}

		void CheckGroupSynchronisedStatus(IEnumerable<GlbGroup> groupToCheck)
		{
			foreach (var group in groupToCheck)
			{
				CheckSynchronisedStatus(group);
			}
		}

		void CheckSynchronisedStatus(BusinessObject biz)
		{
			var adException = string.Empty;
			IDirectoryEntry adEntity = null;
			Type type = null;
			var code = ZString.Empty;
			var isActive = true;
			try
			{
				DirectoryExceptionHandler.ExecuteWithExceptionHandling(
				() =>
				{
					if (biz is GlbStaff staff)
					{
						type = typeof(GlbStaff);
						isActive = staff.GS_IsActive;
						code = staff.GS_Code;
						adEntity = LoadADEntity(staff);
					}
					else if (biz is GlbGroup group) 
					{
						type = typeof(GlbGroup);
						isActive = group.GG_IsActive;
						code = group.GG_Code;
						adEntity = LoadADEntity((GlbGroup)biz);
					}
				}, biz.PK.ToString());
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				adException = ex.GetType().ToString() + ": " + ex.ToString();
			}

			if (adEntity == null || adEntity.IsActive != isActive || !string.IsNullOrEmpty(adException))
			{
				var messageToReport = GetMismatchActiveStatusMessage(adEntity, type, code, isActive, adException);
				Synchronisation.EntitySynchroniser.ReportInconsistentActiveStateAfterADSync(messageToReport);
				ADNotification.ShowError(messageToReport);
			}
		}

		ZQuery GetStaffForCheckingQuery(Guid lowerPk, Guid upperPk)
		{
			var query = new ZQuery(GlbStaffSchema.GS_IsSystemAccount, false);
			query.ReLoadExistingRows = true; // To get the latest data
			query.AddToFilter(GlbStaffSchema.GS_IsResource, false);
			query.AddToFilter(new ZQuery(GlbStaffSchema.PK, SQLComparisonOperator.GreaterThanOrEqualTo, lowerPk));
			query.AddToFilter(new ZQuery(GlbStaffSchema.PK, SQLComparisonOperator.LessThanOrEqualTo, upperPk));
			query.AddToFilter(new ZQuery(GlbStaffSchema.GS_ActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, null));
			query.AddToFilter(new ZQuery(GlbStaffSchema.GS_ActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, ZGuid.Invalid));
			return query;
		}

		ZQuery GetGroupsForCheckingQuery(Guid lowerPk, Guid upperPk)
		{
			var query = new ZQuery(GlbGroupSchema.GG_IsSystemDefined, false);
			query.ReLoadExistingRows = true; // To get the latest data
			query.AddToFilter(GlbGroupSchema.GG_Type, GlbGroupTypeList.Codes.Staff);
			query.AddToFilter(new ZQuery(GlbGroupSchema.PK, SQLComparisonOperator.GreaterThanOrEqualTo, lowerPk));
			query.AddToFilter(new ZQuery(GlbGroupSchema.PK, SQLComparisonOperator.LessThanOrEqualTo, upperPk));
			query.AddToFilter(new ZQuery(GlbGroupSchema.GG_ActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, null));
			query.AddToFilter(new ZQuery(GlbGroupSchema.GG_ActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, ZGuid.Invalid));
			return query;
		}

		IDirectoryEntry LoadADEntity(GlbStaff staff)
		{
			var adUser = new ADUser(staff);
			var entry = adUser.GetDirectoryEntry();
			return entry;
		}

		IDirectoryEntry LoadADEntity(GlbGroup group)
		{
			var adGroup = new ADGroup(group);
			var entry = adGroup.GetDirectoryEntry();
			return entry;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is log message")]
		string GetMismatchActiveStatusMessage(IDirectoryEntry entity, Type bizType, ZString code, bool bizIsActive, string adException)
		{
			var message = new StringBuilder();

			var activeStatus = bizIsActive ? "active" : "inactive";
			message.Append($"{DataBoundResourceStrings.GetStringForTable(bizType)} '{code}' is {activeStatus} in CW1");

			if (entity == null || !string.IsNullOrEmpty(adException))
			{
				message.Append(", and an exception happened when trying to retrieve the AD value: " + adException);
			}
			else
			{
				var adStatus = entity.IsActive ? "active" : "inactive";
				message.Append($", but it is still {adStatus} in AD.");
			}

			return message.ToString();
		}
	}
}
