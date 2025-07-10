using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TimeEngineScheduler.Integration;

namespace Enterprise.TimeEngineScheduler.Business
{
	public class TimeActionSchedule : AutoTimeActionSchedule, IActionSchedule
	{
		public TimeActionSchedule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region IActionSchedule members

		ZString IActionSchedule.ActionCode
		{
			get => TAS_ActionCode;
			set => TAS_ActionCode = value;
		}

		ZDateTime IActionSchedule.ExecutionDateTimeUtc
		{
			get => TAS_ExecutionDateTimeUtc;
			set => TAS_ExecutionDateTimeUtc = value;
		}

		ZString IActionSchedule.ExecutionResult
		{
			get => TAS_ExecutionResult;
			set => TAS_ExecutionResult = value;
		}

		ZString IActionSchedule.ExecutionStatus
		{
			get => TAS_ExecutionStatus;
			set => TAS_ExecutionStatus = value;
		}

		ZString IActionSchedule.JsonParameter
		{
			get => TAS_JsonParameter;
			set => TAS_JsonParameter = value;
		}

		ZByte IActionSchedule.RetryAttempts
		{
			get => TAS_RetryAttempts;
			set => TAS_RetryAttempts = value;
		}

		ZGuid IActionSchedule.TargetPK
		{
			get => TAS_TargetPK;
			set => TAS_TargetPK = value;
		}

		ZString IActionSchedule.TargetTableCode
		{
			get => TAS_TargetTableCode;
			set => TAS_TargetTableCode = value;
		}

		ZDateTime IActionSchedule.SystemCreateTimeUtc => TAS_SystemCreateTimeUtc;

		ZGuid IActionSchedule.ExecutionBranch
		{
			get => TAS_GB_Branch;
			set => TAS_GB_Branch = value;
		}

		ZGuid IActionSchedule.ExecutionDepartment
		{
			get => TAS_GE_Department;
			set => TAS_GE_Department = value;
		}

		ZString IActionSchedule.Token
		{
			get => TAS_Token;
			set => TAS_Token = value;
		}

		public TimeActionSchedulingState NonPersistentSchedulingState { get; internal set; }

		#endregion

		public override string ToString() => FormattableString.Invariant($"{nameof(TAS_ActionCode)}: {TAS_ActionCode}, {nameof(TAS_ExecutionDateTimeUtc)}: {TAS_ExecutionDateTimeUtc}, {nameof(TAS_TargetPK)}: {TAS_TargetPK}, {nameof(TAS_TargetTableCode)}: {TAS_TargetTableCode}, {nameof(TAS_ExecutionResult)}: {TAS_ExecutionResult}, {nameof(TAS_ExecutionStatus)}: {TAS_ExecutionStatus}, {nameof(TAS_JsonParameter)}: {TAS_JsonParameter}, {nameof(TAS_Token)}: {TAS_Token}");
	}
}
