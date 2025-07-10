using System;

namespace Enterprise.Client.EDI.HR.PayrollMetrics
{
	/// <summary>
	/// Params to Api/EssLeaveRequestStatusModified 
	/// Used to get the employees' ESS leave requests based on parameters provided
	/// </summary>
	public class EssLeaveRequestStatusModifiedApiParam
	{
		/// <summary>
		/// A valid Payroll Name belonging to the Customer. 
		/// </summary>
		public string PayrollName;

		/// <summary>
		/// A valid Employee Number belonging to the input Payroll.
		/// If not supplied will return all employees’ ESS leave requests for the nominated Payroll.
		/// </summary>
		public string EmployeeNumber;

		/// <summary>
		/// Status of the leave request. 
		/// One or more leave status can be supplied.
		/// If more than one leave status, it needs to be a comma separated string.
		/// Eg.
		/// Approved, Cancelled
		/// If not supplied, the system will return approved and cancelled status.
		/// Valid Values: Approved, Cancelled
		/// </summary>
		public string LeaveStatus;

		/// <summary>
		/// One or more leave indicators.
		/// If more than one leave indicator, it needs to be a comma separated string. Eg.
		/// Annual, Personal
		/// If not supplied, the system will return all leave indicators.
		/// 
		/// Valid Values
		///   Annual Leave,
		///   Personal Leave, 
		///   Long Service Leave, 
		///   Rdo, 
		///   Days In Lieu, 
		///   Leave Loading,
		///   Special Leave 1, 
		///   Special Leave 2,
		///   Special Leave 3, 
		///   Special Leave 4,
		///   Special Leave 5, 
		///   Special Leave 6,
		///   Special Leave 7, 
		///   Special Leave 8,
		///   Special Leave 9, 
		///   Special Leave 10
		/// </summary>
		public string LeaveIndicator;

		public DateTime? StatusModifiedFrom;
		public DateTime? StatusModifiedTo;
	}
}
