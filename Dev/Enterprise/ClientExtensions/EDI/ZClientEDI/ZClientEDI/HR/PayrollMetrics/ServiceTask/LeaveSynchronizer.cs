using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using LogType = Enterprise.Integration.LogType;

namespace Enterprise.Client.EDI.HR.PayrollMetrics
{
	public interface ILeaveSynchronizer
	{
		void Process(IList<EmployeeLeave> leaveList);
	}

	public class LeaveSynchronizer : ILeaveSynchronizer
	{
		public LeaveSynchronizer(BusinessObjectFactory factory = null, ILogger logger = null)
		{
			Factory = factory ?? new BusinessObjectFactory() { RefreshEnabled = false };
			Logger = logger;
		}

		readonly BusinessObjectFactory Factory;
		readonly ILogger Logger;

		public void Process(IList<EmployeeLeave> leaveList)
		{
			foreach (var employee in leaveList)
			{
				employee.FirstName = employee.FirstName.Trim();
				employee.LastName = employee.LastName.Trim();
			}

			var employeeToStaff = LoadStaff(leaveList);
			foreach (var employee in leaveList)
			{
				if (employeeToStaff.TryGetValue(employee, out var staff))
				{
					foreach (var request in employee.LeaveRequests.OrderBy(x => x.StatusModifiedTime))
					{
						ProcessAndLog(staff, employee, request);
					}
				}
				else
				{
					Log(LogType.Warning, "Employee not found #" + employee.EmployeeNumber + " " + employee.FirstName + " " + employee.LastName);
				}
			}
		}

		const string LeaveStatusApproved = "Approved";
		const string LeaveStatusCancelled = "Cancelled";
		const string LeaveDateTimeFormat = "yyyy-MM-dd HH:mm";

		void ProcessAndLog(PayrollMetricsWorkingStaff staff, EmployeeLeave employee, Leave leave)
		{
			try
			{
				Log(LogType.Information, LeaveAsText(employee, leave));

				Process(staff, leave);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (Logger != null)
				{
					Logger.Log(LogType.Error, "", ex);
				}

				ErrorReporter.ReportOnce("Unhandled Exception in LeaveSynchronizer", ex.Message, ex);
			}
		}

		string LeaveAsText(EmployeeLeave employee, Leave leave)
		{
			return "Emp #" + employee.EmployeeNumber
				+ " " + employee.FirstName
				+ " " + employee.LastName
				+ " " + leave.LeavePayElement
				+ " " + leave.LeaveStatus
				+ " " + leave.DateFrom.ToString(LeaveDateTimeFormat, CultureInfo.InvariantCulture)
				+ " to " + leave.DateTo.ToString(LeaveDateTimeFormat, CultureInfo.InvariantCulture)
				+ " (" + leave.HoursOrWeeks + ")";
		}

		void Process(PayrollMetricsWorkingStaff staff, Leave leave)
		{
			var internalLeaveType = ConvertToInternalLeaveType(leave.LeavePayElement);
			var factoryForWrite = new BusinessObjectFactory() { RefreshEnabled = false };

			if (leave.LeaveStatus == LeaveStatusCancelled)
			{
				var existingHoliday = Find(factoryForWrite, staff.Staff, leave, internalLeaveType);
				if (existingHoliday != null)
				{
					existingHoliday.Delete();
					Log(LogType.Information, "Leave deleted");
					factoryForWrite.Save();
				}
			}
			else if (leave.LeaveStatus == LeaveStatusApproved)
			{
				// Do calculations that may hit the database first,
				// to minimize the time between reading and writing GlbStaffHoliday
				// and so reduce concurrency problems.
				var days = staff.CalculateDays(leave);

				var existingHoliday = Find(factoryForWrite, staff.Staff, leave, internalLeaveType);
				if (existingHoliday == null)
				{
					var holiday = factoryForWrite.New<GlbStaffHoliday>();
					holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
					holiday.GA_GS = staff.Staff.PK;
					PopulateStartEnd(holiday, staff, leave);
					holiday.GA_DaysLeaveTaken = days;
					holiday.GA_WorkHolidayType = internalLeaveType;
					holiday.GA_LeaveComment = "PM autosync " + ZDateTime.UtcNow.ToString("u", CultureInfo.InvariantCulture) + " - do not alter";

					factoryForWrite.Save();
					Log(LogType.Information, "Leave created");
				}
				else
				{
					Log(LogType.Information, "Leave already exists");
				}
			}
			else
			{
				Log(LogType.Warning, "Unknown leave status: " + leave.LeaveStatus);
			}
		}

		void PopulateStartEnd(GlbStaffHoliday holiday, PayrollMetricsWorkingStaff staff, Leave leave)
		{
			if (!leave.IsPartDayLeave)
			{
				holiday.GA_StartTime = staff.StartWorkTime(leave.DateFrom);
				holiday.GA_EndTime = staff.EndWorkTime(leave.DateTo);
			}
			else
			{
				var startTime = leave.DateFrom;
				var endTime = leave.DateTo;
				var startWorkTime = staff.StartWorkTime(leave.DateFrom);

				if (startTime < startWorkTime)
				{
					endTime += (startWorkTime - startTime);
					startTime = startWorkTime;
				}

				holiday.GA_StartTime = startTime;
				holiday.GA_EndTime = endTime;
			}
		}

		LeaveTypeConverter LeaveTypeConvert => leaveTypeConvert ?? (leaveTypeConvert = new LeaveTypeConverter(EDIDataRegistry.Instance.PayrollMetricsLeaveTypes.Value));
		LeaveTypeConverter leaveTypeConvert;
		string ConvertToInternalLeaveType(string payrollMetricsLeaveType) => LeaveTypeConvert.ConvertToInternalLeaveType(payrollMetricsLeaveType);

		public static GlbStaffHoliday Find(BusinessObjectFactory factory, GlbStaff staff, Leave leave, string leaveType)
		{
			// Time-of-day can differ in ediProd since working hours may differ.
			// So use date part only.
			var fromDay = leave.DateFrom.Date;
			var toDay = leave.DateTo.Date;

			var query = new ZQuery(GlbStaffHolidaySchema.GA_GS, staff.PK);
			query.AddToFilter(GlbStaffHolidaySchema.GA_StartTime, SQLComparisonOperator.GreaterThanOrEqualTo, fromDay);
			query.AddToFilter(GlbStaffHolidaySchema.GA_StartTime, SQLComparisonOperator.LessThan, fromDay.AddDays(1));
			query.AddToFilter(GlbStaffHolidaySchema.GA_EndTime, SQLComparisonOperator.GreaterThanOrEqualTo, toDay);
			query.AddToFilter(GlbStaffHolidaySchema.GA_EndTime, SQLComparisonOperator.LessThan, toDay.AddDays(1));
			query.AddToFilter(GlbStaffHolidaySchema.GA_RecordType, GlbStaffHolidayLookups.RecordTypes.Leave);
			query.AddToFilter(GlbStaffHolidaySchema.GA_ApprovalStatus, GlbStaffHolidayLookupsReal.Approved);
			var holidays = factory.Load<GlbStaffHoliday>(query);

			if (holidays.Length <= 1)
			{
				return holidays.FirstOrDefault();
			}
			else
			{
				var sameLeaveType = holidays.Where(x => x.GA_WorkHolidayType == leaveType);
				var sameLeaveTypeCount = sameLeaveType.Count();
				if (sameLeaveTypeCount == 1)
				{
					return sameLeaveType.First();
				}
				else
				{
					if (sameLeaveTypeCount > 1)
					{
						holidays = sameLeaveType.ToArray();
					}

					// pick the closest in start time
					return holidays
						.OrderBy(x => Math.Abs((x.GA_StartTime.ToDateTime() - leave.DateFrom).TotalHours))
						.First();
				}
			}
		}

		Dictionary<EmployeeLeave, PayrollMetricsWorkingStaff> LoadStaff(IList<EmployeeLeave> leaveList)
		{
			var cert = LoadCertByNumber(leaveList.Select(x => x.EmployeeNumber));
			var staffList = LoadStaffByPk(cert.Select(x => x.XZ_ParentID));

			var employeesToMatchByName = new List<EmployeeLeave>();
			var staffPkToWorkingStaff = new Dictionary<Guid, PayrollMetricsWorkingStaff>();
			var result = new Dictionary<EmployeeLeave, PayrollMetricsWorkingStaff>();

			try
			{
				var staffPkToNumber = cert.GroupBy(c => c.XZ_ParentID)
										  .ToDictionary(k => k.Key.ToGuid(),
														v => v.OrderByDescending(x => x.XZ_SystemCreateTimeUtc)
															  .ThenByDescending(y => y.XZ_RefNumber)
															  .First().XZ_RefNumber.ToLower().ToString());

				var numberToStaff = staffList.Select(x => new { Num = staffPkToNumber[x.PK.ToGuid()], Staff = x })
											.GroupBy(x => x.Num)
											.ToDictionary(k => k.Key,
														  v => v.OrderByDescending(x => x.Staff.GS_IsActive ? 1 : 0)
																.ThenByDescending(x => x.Staff.GS_LastActivityDate)
																.First().Staff);
				foreach (var employee in leaveList)
				{
					if (numberToStaff.TryGetValue(employee.EmployeeNumber.ToLower(), out var staff))
					{
						var workStaff = new PayrollMetricsWorkingStaff(staff);
						result.Add(employee, workStaff);
						staffPkToWorkingStaff.Add(staff.PK.ToGuid(), workStaff);
					}
					else
					{
						employeesToMatchByName.Add(employee);
					}
				}
			}
			catch (ArgumentException ex)
			{
				var invalidArguments = cert.GroupBy(x => x.XZ_ParentID).Where(g => g.Count() > 1);
				if (invalidArguments.Any())
				{
					var message = new StringBuilder();
					message.AppendLine("The following XZ_ParentID are duplicated");
					foreach (var group in invalidArguments)
					{
						message.AppendLine($"[ParentID]{group.Key}:[RowPKs]{string.Join(", ", group.Select(x => x.PK))}");
					}
					throw new ArgumentException(message.ToString(), ex);
				}

				throw;
			}

			if (employeesToMatchByName.Count > 0)
			{
				// Note: can have employee with same name, but different number
				var fullNames = employeesToMatchByName.Select(x => x.FirstName + " " + x.LastName).ToArray();
				staffList = LoadStaffByName(fullNames);
				if (staffList.Length > 0)
				{
					var nameToStaff = staffList.
						GroupBy(x => x.GS_FullName.ToLower().ToString())
						.ToDictionary(k => k.Key,
									  v => v.OrderByDescending(x => x.GS_LastActivityDate)
											.First());

					for (var i = 0; i < employeesToMatchByName.Count; ++i)
					{
						var employee = employeesToMatchByName[i];
						var fullName = fullNames[i].ToLower();
						if (nameToStaff.TryGetValue(fullName, out var staff))
						{
							PayrollMetricsWorkingStaff workStaff;
							if (!staffPkToWorkingStaff.TryGetValue(staff.PK.ToGuid(), out workStaff))
							{
								workStaff = new PayrollMetricsWorkingStaff(staff);
								staffPkToWorkingStaff.Add(staff.PK.ToGuid(), workStaff);
							}
							result.Add(employee, workStaff);
						}
					}
				}
			}

			return result;
		}

		EDIGlbStaff[] LoadStaffByName(IEnumerable<string> names)
		{
			var staffQuery = new ZQuery(GlbStaffSchema.GS_FullName, names);
			staffQuery.AddToFilter(GlbStaffSchema.GS_IsActive, true);
			return Factory.Load<EDIGlbStaff>(staffQuery);
		}

		EDIGlbStaff[] LoadStaffByPk(IEnumerable<ZGuid> pks)
		{
			return HR.StaffQuery.LoadStaffByPks(Factory, pks);
		}

		GenRegCertAccredMaintList[] LoadCertByNumber(IEnumerable<string> numbers)
		{
			return HR.StaffQuery.LoadCertByNumber(Factory, Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.PID, numbers);
		}

		void Log(LogType logType, string msg)
		{
			if (Logger != null)
			{
				Logger.Log(logType, msg);
			}
		}
	}
}
