using System;
using System.Data;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Common.CW;

record NativeServiceTaskDTO
{
	public NativeServiceTaskDTO(
		IHostedServiceAttribute hostedServiceAttribute,
		Guid? branchPk)
		: this(
			hostedServiceAttribute.Code,
			Guid.Empty,
			false,
			DateTimeOffset.MinValue,
			DateTimeOffset.MinValue,
			branchPk,
			string.Empty,
			string.Empty,
			string.Empty)
	{
	}

	internal NativeServiceTaskDTO(
		string code,
		Guid pk,
		bool isActive,
		DateTimeOffset nextRunTime,
		DateTimeOffset? lastRunTime,
		Guid? branchPk,
		string branchName,
		string branchErrorMsg,
		string settingsXml)
	{
		Pk = pk;
		IsActive = isActive;
		NextRunTime = nextRunTime;
		LastRunTime = lastRunTime;
		ServiceTaskCode = code;
		BranchPk = branchPk;
		BranchName = branchName;
		BranchErrorMessage = branchErrorMsg;
		SettingsXml = settingsXml;
	}

	public Guid Pk { get; }
	public bool IsActive { get; }
	public DateTimeOffset NextRunTime { get; }
	public DateTimeOffset? LastRunTime { get; }
	public string ServiceTaskCode { get; }
	public Guid? BranchPk { get; }
	public string BranchName { get; }
	public string BranchErrorMessage { get; }
	public string SettingsXml { get; }

	public static NativeServiceTaskDTO CreateFromReader(IDataReader reader)
	{
		var code = (string)reader["SST_ServiceTaskCode"];
		var taskPk = (Guid)reader["SST_PK"];
		var isActive = (bool)reader["SST_Active"];
		var nextRunTime = (DateTimeOffset)reader["SST_NextRunTime"];
		var lastRunTime = reader["SST_LastRunTime"] == DBNull.Value
			? null
			: (DateTimeOffset?)reader["SST_LastRunTime"];
		var branchPk = reader["GB_PK"] == DBNull.Value ? null : (Guid?)reader["GB_PK"];
		var branchName = reader["GB_Code"] == DBNull.Value ? string.Empty : (string)reader["GB_Code"];
		var branchErrorMessage = string.Empty;
		var settingsXml = (string)reader["SST_Configuration"];

		return new NativeServiceTaskDTO(code, taskPk, isActive, nextRunTime, lastRunTime, branchPk, branchName, branchErrorMessage, settingsXml);
	}
}
